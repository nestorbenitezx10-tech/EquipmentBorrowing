using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ViewModelBase
{
    private readonly BorrowEquipmentService _borrowService;
    private readonly ReturnEquipmentService _returnService;
    private readonly StudentManagementService _studentService;
    private readonly EquipmentManagementService _equipmentService;
    private readonly Action<string, bool> _setFeedback;
    private readonly Func<Task> _refreshAll;

    public ObservableCollection<Borrowing> Borrowings { get; } = new();
    public ObservableCollection<Student> Students { get; } = new();
    public ObservableCollection<Equipment> AvailableEquipment { get; } = new();

    public IReadOnlyList<BorrowingStatusFilter> StatusFilters { get; } =
        Enum.GetValues<BorrowingStatusFilter>();

    public IReadOnlyList<BorrowingSortField> SortFields { get; } =
        Enum.GetValues<BorrowingSortField>();

    [ObservableProperty]
    private Borrowing? selectedBorrowing;

    [ObservableProperty]
    private Student? selectedCheckoutStudent;

    [ObservableProperty]
    private DateTimeOffset expectedReturnDateOffset = DateTimeOffset.Now.Date.AddDays(7);

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private BorrowingStatusFilter selectedStatusFilter = BorrowingStatusFilter.All;

    [ObservableProperty]
    private BorrowingSortField selectedSortField = BorrowingSortField.CheckoutDate;

    [ObservableProperty]
    private bool sortAscending;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? listStatusMessage;

    [ObservableProperty]
    private bool hasListError;

    public BorrowingsViewModel(
        BorrowEquipmentService borrowService,
        ReturnEquipmentService returnService,
        StudentManagementService studentService,
        EquipmentManagementService equipmentService,
        Action<string, bool> setFeedback,
        Func<Task> refreshAll)
    {
        _borrowService = borrowService ?? throw new ArgumentNullException(nameof(borrowService));
        _returnService = returnService ?? throw new ArgumentNullException(nameof(returnService));
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
        _setFeedback = setFeedback ?? throw new ArgumentNullException(nameof(setFeedback));
        _refreshAll = refreshAll ?? throw new ArgumentNullException(nameof(refreshAll));
    }

    partial void OnSelectedStatusFilterChanged(BorrowingStatusFilter value)
    {
        _ = LoadBorrowingsAsync();
    }

    partial void OnSelectedSortFieldChanged(BorrowingSortField value)
    {
        _ = LoadBorrowingsAsync();
    }

    partial void OnSortAscendingChanged(bool value)
    {
        _ = LoadBorrowingsAsync();
    }

    public async Task LoadAsync()
    {
        var selectedStudentId = SelectedCheckoutStudent?.Id;

        var allStudents = await _studentService.GetAllStudentsAsync();
        Students.Clear();
        foreach (var student in allStudents)
            Students.Add(student);

        if (!string.IsNullOrWhiteSpace(selectedStudentId))
            SelectedCheckoutStudent = Students.FirstOrDefault(s => s.Id == selectedStudentId);
        else
            SelectedCheckoutStudent ??= Students.FirstOrDefault();

        var available = await _equipmentService.GetEquipmentAsync(availableOnly: true);
        AvailableEquipment.Clear();
        foreach (var item in available)
            AvailableEquipment.Add(item);

        await LoadBorrowingsAsync();
    }

    public async Task LoadBorrowingsAsync()
    {
        IsLoading = true;
        HasListError = false;
        ListStatusMessage = null;
        Borrowings.Clear();

        try
        {
            var results = await _borrowService.SearchBorrowingsAsync(
                SearchText,
                SelectedStatusFilter,
                SelectedSortField,
                SortAscending);

            foreach (var borrowing in results)
                Borrowings.Add(borrowing);

            if (Borrowings.Count == 0)
                ListStatusMessage = "No borrowing records match the current search or filter criteria.";
        }
        catch (Exception ex)
        {
            HasListError = true;
            ListStatusMessage = $"Failed to load borrowings: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ApplySearchOrFilterAsync()
    {
        await LoadBorrowingsAsync();
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        SelectedStatusFilter = BorrowingStatusFilter.All;
        SelectedSortField = BorrowingSortField.CheckoutDate;
        SortAscending = false;
        await LoadBorrowingsAsync();
    }

    [RelayCommand]
    private async Task CheckoutSelectedItemsAsync(System.Collections.IList? selectedItems)
    {
        if (SelectedCheckoutStudent is null)
        {
            _setFeedback("Select a student for checkout.", true);
            return;
        }

        var chosenIds = selectedItems?
            .OfType<Equipment>()
            .Select(e => e.Id)
            .Distinct()
            .ToList() ?? new List<string>();

        if (chosenIds.Count < 2)
        {
            _setFeedback("Select at least 2 distinct available equipment items for multi-item checkout.", true);
            return;
        }

        var expectedReturnDate = DateOnly.FromDateTime(ExpectedReturnDateOffset.LocalDateTime);

        try
        {
            OpResult result = await _borrowService.RequestMultiBorrowingAsync(
                SelectedCheckoutStudent.Id,
                chosenIds,
                expectedReturnDate);

            _setFeedback(result.Message, !result.IsSuccess);
            if (result.IsSuccess)
                await _refreshAll();
        }
        catch (Exception ex)
        {
            _setFeedback($"Multi-item checkout failed: {ex.Message}", true);
        }
    }

    [RelayCommand]
    private async Task ReturnSelectedEquipmentAsync()
    {
        if (SelectedBorrowing is null)
        {
            _setFeedback("Select a borrowing record to return.", true);
            return;
        }

        try
        {
            OpResult result = await _returnService.ReturnAsync(SelectedBorrowing.Id);
            _setFeedback(result.Message, !result.IsSuccess);
            if (result.IsSuccess)
                await _refreshAll();
        }
        catch (Exception ex)
        {
            _setFeedback($"Return failed: {ex.Message}", true);
        }
    }
}