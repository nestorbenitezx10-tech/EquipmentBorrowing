using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ViewModelBase
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly BorrowEquipmentService _borrowService;
    private readonly Action<string, bool> _setFeedback;
    private readonly Func<Task> _refreshData;

    public ObservableCollection<Equipment> Equipment { get; } = new();

    [ObservableProperty]
    private string studentId = "STU101";

    [ObservableProperty]
    private int borrowingDays = 7;

    public EquipmentViewModel(
        IEquipmentRepository equipmentRepository,
        BorrowEquipmentService borrowService,
        Action<string, bool> setFeedback,
        Func<Task> refreshData)
    {
        _equipmentRepository = equipmentRepository;
        _borrowService = borrowService;
        _setFeedback = setFeedback;
        _refreshData = refreshData;
    }

    public async Task LoadAsync()
    {
        Equipment.Clear();
        foreach (var equipment in await _equipmentRepository.GetAllAsync())
            Equipment.Add(equipment);
    }

    [RelayCommand]
    private async Task BorrowEquipmentAsync(Equipment equipment)
    {
        try
        {
            var borrowing = await _borrowService.RequestBorrowingAsync(
                StudentId,
                equipment.Id,
                BorrowingDays);
            _setFeedback($"Borrowing requested for {equipment.Name}. ID: {borrowing.Id}", false);
            await _refreshData();
        }
        catch (InvalidOperationException exception)
        {
            _setFeedback(exception.Message, true);
        }
    }
}