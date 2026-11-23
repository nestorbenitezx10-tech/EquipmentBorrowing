using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ViewModelBase
{
    private readonly EquipmentManagementService _equipmentService;
    private readonly BorrowEquipmentService _borrowService;
    private readonly Action<string, bool> _setFeedback;
    private readonly Func<Task> _refreshAll;

    public ObservableCollection<Equipment> EquipmentList { get; } = new();
    public ObservableCollection<Borrowing> SelectedEquipmentHistory { get; } = new();

    [ObservableProperty]
    private Equipment? selectedEquipment;

    [ObservableProperty]
    private bool showAvailableOnly;

    [ObservableProperty]
    private string studentIdInput = "STU-001";

    [ObservableProperty]
    private int borrowingDays = 7;

    [ObservableProperty]
    private string identifierInput = string.Empty;

    [ObservableProperty]
    private string nameInput = string.Empty;

    [ObservableProperty]
    private string descriptionInput = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isHistoryLoading;

    [ObservableProperty]
    private string? historyStatusMessage;

    [ObservableProperty]
    private bool hasHistoryError;

    public EquipmentViewModel(
        EquipmentManagementService equipmentService,
        BorrowEquipmentService borrowService,
        Action<string, bool> setFeedback,
        Func<Task> refreshAll)
    {
        _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
        _borrowService = borrowService ?? throw new ArgumentNullException(nameof(borrowService));
        _setFeedback = setFeedback ?? throw new ArgumentNullException(nameof(setFeedback));
        _refreshAll = refreshAll ?? throw new ArgumentNullException(nameof(refreshAll));
    }

    partial void OnShowAvailableOnlyChanged(bool value)
    {
        _ = LoadAsync();
    }

    partial void OnSelectedEquipmentChanged(Equipment? value)
    {
        if (value is not null)
        {
            IdentifierInput = value.Identifier;
            NameInput = value.Name;
            DescriptionInput = value.Description ?? string.Empty;
            _ = LoadHistoryForEquipmentAsync(value.Id);
        }
        else
        {
            SelectedEquipmentHistory.Clear();
            HistoryStatusMessage = "Select equipment to view borrowing history.";
            HasHistoryError = false;
        }
    }

    public async Task LoadAsync()
    {
        var selectedId = SelectedEquipment?.Id;
        IsLoading = true;
        try
        {
            var items = await _equipmentService.GetEquipmentAsync(ShowAvailableOnly);
            EquipmentList.Clear();
            foreach (var item in items)
                EquipmentList.Add(item);

            if (!string.IsNullOrWhiteSpace(selectedId))
            {
                Equipment? match = null;
                foreach (var eq in EquipmentList)
                {
                    if (eq.Id == selectedId)
                    {
                        match = eq;
                        break;
                    }
                }
                SelectedEquipment = match;
            }
            else if (SelectedEquipment is null)
            {
                HistoryStatusMessage = "Select equipment to view borrowing history.";
                HasHistoryError = false;
            }
        }
        catch (Exception ex)
        {
            _setFeedback($"Failed to load equipment: {ex.Message}", true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadHistoryForEquipmentAsync(string equipmentId)
    {
        IsHistoryLoading = true;
        HasHistoryError = false;
        HistoryStatusMessage = null;
        SelectedEquipmentHistory.Clear();

        try
        {
            var history = await _equipmentService.GetEquipmentHistoryAsync(equipmentId);
            foreach (var entry in history)
                SelectedEquipmentHistory.Add(entry);

            if (SelectedEquipmentHistory.Count == 0)
                HistoryStatusMessage = "No borrowing history found for this equipment item.";
        }
        catch (Exception ex)
        {
            HasHistoryError = true;
            HistoryStatusMessage = $"Unable to load equipment history: {ex.Message}";
        }
        finally
        {
            IsHistoryLoading = false;
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedEquipment = null;
        IdentifierInput = string.Empty;
        NameInput = string.Empty;
        DescriptionInput = string.Empty;
    }

    [RelayCommand]
    private async Task RegisterEquipmentAsync()
    {
        try
        {
            OpResult result = await _equipmentService.RegisterEquipmentAsync(
                IdentifierInput,
                NameInput,
                string.IsNullOrWhiteSpace(DescriptionInput) ? null : DescriptionInput);

            _setFeedback(result.Message, !result.IsSuccess);
            if (result.IsSuccess)
            {
                IdentifierInput = string.Empty;
                NameInput = string.Empty;
                DescriptionInput = string.Empty;
                await _refreshAll();
            }
        }
        catch (Exception ex)
        {
            _setFeedback($"Equipment registration failed: {ex.Message}", true);
        }
    }

    [RelayCommand]
    private async Task UpdateEquipmentAsync()
    {
        if (SelectedEquipment is null)
        {
            _setFeedback("Select equipment to update.", true);
            return;
        }

        try
        {
            var targetId = SelectedEquipment.Id;
            OpResult result = await _equipmentService.UpdateEquipmentAsync(
                targetId,
                IdentifierInput,
                NameInput,
                string.IsNullOrWhiteSpace(DescriptionInput) ? null : DescriptionInput);

            _setFeedback(result.Message, !result.IsSuccess);
            if (result.IsSuccess)
            {
                await _refreshAll();
            }
        }
        catch (Exception ex)
        {
            _setFeedback($"Equipment update failed: {ex.Message}", true);
        }
    }

    [RelayCommand]
    private async Task BorrowSelectedEquipmentAsync()
    {
        if (SelectedEquipment is null)
        {
            _setFeedback("Select equipment to borrow.", true);
            return;
        }

        try
        {
            var checkoutDate = DateOnly.FromDateTime(DateTime.Today);
            OpResult result = await _borrowService.RequestCheckoutAsync(
                StudentIdInput,
                new[] { SelectedEquipment.Id },
                checkoutDate,
                checkoutDate.AddDays(BorrowingDays));

            _setFeedback(result.Message, !result.IsSuccess);
            if (result.IsSuccess)
                await _refreshAll();
        }
        catch (Exception ex)
        {
            _setFeedback($"Borrow request failed: {ex.Message}", true);
        }
    }
}