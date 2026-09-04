using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ViewModelBase
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly ReturnEquipmentService _returnService;
    private readonly Action<string, bool> _setFeedback;
    private readonly Func<Task> _refreshData;

    public ObservableCollection<Borrowing> ActiveBorrowings { get; } = new();

    public BorrowingsViewModel(
        IBorrowingRepository borrowingRepository,
        ReturnEquipmentService returnService,
        Action<string, bool> setFeedback,
        Func<Task> refreshData)
    {
        _borrowingRepository = borrowingRepository;
        _returnService = returnService;
        _setFeedback = setFeedback;
        _refreshData = refreshData;
    }

    public async Task LoadActiveBorrowingsAsync()
    {
        ActiveBorrowings.Clear();
        var activeBorrowings = await _borrowingRepository.GetActiveBorrowingsAsync();
        foreach (var borrowing in activeBorrowings)
            ActiveBorrowings.Add(borrowing);
    }

    public Task LoadAsync()
    {
        return LoadActiveBorrowingsAsync();
    }

    [RelayCommand]
    private async Task ReturnEquipmentAsync(Borrowing borrowing)
    {
        var result = await _returnService.ReturnAsync(borrowing.Id);
        _setFeedback(result.Message, !result.Success);
        await _refreshData();
    }
}