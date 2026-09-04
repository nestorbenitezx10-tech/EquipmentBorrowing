using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using System;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public EquipmentViewModel EquipmentViewModel { get; }
    public BorrowingsViewModel BorrowingsViewModel { get; }

    [ObservableProperty]
    private ViewModelBase? currentViewModel;

    [ObservableProperty]
    private string feedbackMessage = "Select equipment to begin.";

    [ObservableProperty]
    private bool hasError;

    public MainWindowViewModel()
    {
        EquipmentViewModel = null!;
        BorrowingsViewModel = null!;
    }

    public MainWindowViewModel(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        var borrowService = new BorrowEquipmentService(
            studentRepository,
            equipmentRepository,
            borrowingRepository);
        var returnService = new ReturnEquipmentService(
            borrowingRepository,
            equipmentRepository);

        EquipmentViewModel = new EquipmentViewModel(
            equipmentRepository,
            borrowService,
            SetFeedback,
            RefreshDataAsync);
        BorrowingsViewModel = new BorrowingsViewModel(
            borrowingRepository,
            returnService,
            SetFeedback,
            RefreshDataAsync);

        CurrentViewModel = EquipmentViewModel;
        RefreshDataAsync().GetAwaiter().GetResult();
    }

    [RelayCommand]
    private void ShowEquipment()
    {
        CurrentViewModel = EquipmentViewModel;
    }

    [RelayCommand]
    private async Task ShowBorrowings()
    {
        CurrentViewModel = BorrowingsViewModel;
        await BorrowingsViewModel.LoadActiveBorrowingsAsync();
    }

    private async Task RefreshDataAsync()
    {
        await EquipmentViewModel.LoadAsync();
        await BorrowingsViewModel.LoadAsync();
    }

    private void SetFeedback(string message, bool isError = false)
    {
        FeedbackMessage = message;
        HasError = isError;
    }
}