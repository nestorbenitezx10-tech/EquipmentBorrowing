using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class StudentsViewModel : ViewModelBase
{
    private readonly StudentManagementService _studentService;
    private readonly Action<string, bool> _setFeedback;
    private readonly Func<Task> _refreshAll;

    public ObservableCollection<Student> Students { get; } = new();
    public ObservableCollection<Borrowing> SelectedStudentHistory { get; } = new();

    [ObservableProperty]
    private Student? selectedStudent;

    [ObservableProperty]
    private string studentNumberInput = string.Empty;

    [ObservableProperty]
    private string fullNameInput = string.Empty;

    [ObservableProperty]
    private string contactInput = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isHistoryLoading;

    [ObservableProperty]
    private string? historyStatusMessage;

    [ObservableProperty]
    private bool hasHistoryError;

    public StudentsViewModel(
        StudentManagementService studentService,
        Action<string, bool> setFeedback,
        Func<Task> refreshAll)
    {
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _setFeedback = setFeedback ?? throw new ArgumentNullException(nameof(setFeedback));
        _refreshAll = refreshAll ?? throw new ArgumentNullException(nameof(refreshAll));
    }

    partial void OnSelectedStudentChanged(Student? value)
    {
        if (value is not null)
        {
            StudentNumberInput = value.StudentNumber;
            FullNameInput = value.FullName;
            ContactInput = value.Contact ?? string.Empty;
            _ = LoadHistoryForStudentAsync(value.Id);
        }
        else
        {
            SelectedStudentHistory.Clear();
            HistoryStatusMessage = "Select a student to view borrowing history.";
            HasHistoryError = false;
        }
    }

    public async Task LoadAsync()
    {
        var selectedId = SelectedStudent?.Id;
        IsLoading = true;
        try
        {
            var list = await _studentService.GetAllStudentsAsync();
            Students.Clear();
            foreach (var student in list)
                Students.Add(student);

            if (!string.IsNullOrWhiteSpace(selectedId))
            {
                Student? match = null;
                foreach (var s in Students)
                {
                    if (s.Id == selectedId)
                    {
                        match = s;
                        break;
                    }
                }
                SelectedStudent = match;
            }
            else if (SelectedStudent is null)
            {
                HistoryStatusMessage = "Select a student to view borrowing history.";
                HasHistoryError = false;
            }
        }
        catch (Exception ex)
        {
            _setFeedback($"Failed to load students: {ex.Message}", true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadHistoryForStudentAsync(string studentId)
    {
        IsHistoryLoading = true;
        HasHistoryError = false;
        HistoryStatusMessage = null;
        SelectedStudentHistory.Clear();

        try
        {
            var history = await _studentService.GetStudentHistoryAsync(studentId);
            foreach (var item in history)
                SelectedStudentHistory.Add(item);

            if (SelectedStudentHistory.Count == 0)
                HistoryStatusMessage = "No borrowing records found for this student.";
        }
        catch (Exception ex)
        {
            HasHistoryError = true;
            HistoryStatusMessage = $"Unable to load borrowing history: {ex.Message}";
        }
        finally
        {
            IsHistoryLoading = false;
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedStudent = null;
        StudentNumberInput = string.Empty;
        FullNameInput = string.Empty;
        ContactInput = string.Empty;
    }

    [RelayCommand]
    private async Task RegisterStudentAsync()
    {
        try
        {
            OpResult result = await _studentService.RegisterStudentAsync(
                StudentNumberInput,
                FullNameInput,
                string.IsNullOrWhiteSpace(ContactInput) ? null : ContactInput);

            _setFeedback(result.Message, !result.IsSuccess);
            if (result.IsSuccess)
            {
                StudentNumberInput = string.Empty;
                FullNameInput = string.Empty;
                ContactInput = string.Empty;
                await _refreshAll();
            }
        }
        catch (Exception ex)
        {
            _setFeedback($"Registration failed: {ex.Message}", true);
        }
    }

    [RelayCommand]
    private async Task UpdateStudentAsync()
    {
        if (SelectedStudent is null)
        {
            _setFeedback("Select a student to update.", true);
            return;
        }

        try
        {
            var targetId = SelectedStudent.Id;
            OpResult result = await _studentService.UpdateStudentAsync(
                targetId,
                StudentNumberInput,
                FullNameInput,
                string.IsNullOrWhiteSpace(ContactInput) ? null : ContactInput);

            _setFeedback(result.Message, !result.IsSuccess);
            if (result.IsSuccess)
            {
                await _refreshAll();
            }
        }
        catch (Exception ex)
        {
            _setFeedback($"Update failed: {ex.Message}", true);
        }
    }

    [RelayCommand]
    private async Task PermitStudentAsync()
    {
        if (SelectedStudent is null)
        {
            _setFeedback("Select a student to permit.", true);
            return;
        }

        try
        {
            OpResult result = await _studentService.SetEligibilityAsync(SelectedStudent.Id, true);
            _setFeedback(result.Message, !result.IsSuccess);
            if (result.IsSuccess)
                await _refreshAll();
        }
        catch (Exception ex)
        {
            _setFeedback($"Failed to permit student: {ex.Message}", true);
        }
    }

    [RelayCommand]
    private async Task RestrictStudentAsync()
    {
        if (SelectedStudent is null)
        {
            _setFeedback("Select a student to restrict.", true);
            return;
        }

        try
        {
            OpResult result = await _studentService.SetEligibilityAsync(SelectedStudent.Id, false);
            _setFeedback(result.Message, !result.IsSuccess);
            if (result.IsSuccess)
                await _refreshAll();
        }
        catch (Exception ex)
        {
            _setFeedback($"Failed to restrict student: {ex.Message}", true);
        }
    }
}