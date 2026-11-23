using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using System;
using System.Threading.Tasks;
using EquipmentBorrowing.Desktop.Services;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public EquipmentViewModel EquipmentViewModel { get; }
    public BorrowingsViewModel BorrowingsViewModel { get; }
    public StudentsViewModel StudentsViewModel { get; }

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
        StudentsViewModel = null!;
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
            borrowingRepository);
        var studentService = new StudentManagementService(
            studentRepository,
            borrowingRepository);
        var equipmentService = new EquipmentManagementService(
            equipmentRepository,
            borrowingRepository);

        EquipmentViewModel = new EquipmentViewModel(
            equipmentService,
            borrowService,
            SetFeedback,
            RefreshDataAsync);
        BorrowingsViewModel = new BorrowingsViewModel(
            borrowService,
            returnService,
            studentService,
            equipmentService,
            SetFeedback,
            RefreshDataAsync);
        StudentsViewModel = new StudentsViewModel(
            studentService,
            SetFeedback,
            RefreshDataAsync);

        CurrentViewModel = EquipmentViewModel;
        _ = InitializeAsync();
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
        await BorrowingsViewModel.LoadAsync();
    }

    [RelayCommand]
    private async Task ShowStudents()
    {
        CurrentViewModel = StudentsViewModel;
        await StudentsViewModel.LoadAsync();
    }

    private async Task RefreshDataAsync()
    {
        await EquipmentViewModel.LoadAsync();
        await StudentsViewModel.LoadAsync();
        await BorrowingsViewModel.LoadAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            await RefreshDataAsync();
        }
        catch (Exception ex)
        {
            var message = ex switch
            {
                ApiUnavailableException => ex.Message,
                ApiServerException => ex.Message,
                _ => $"Failed to load application data: {ex.Message}"
            };
            SetFeedback(message, true);
        }
    }

    private void SetFeedback(string message, bool isError = false)
    {
        FeedbackMessage = message;
        HasError = isError;
    }
}
