using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Desktop.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var http = ApiClientFactory.Create();
            var studentRepository = new HttpStudentRepository(http);
            var equipmentRepository = new HttpEquipmentRepository(http);
            var borrowingRepository = new HttpBorrowingRepository(http);

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(
                    studentRepository,
                    equipmentRepository,
                    borrowingRepository)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

}
