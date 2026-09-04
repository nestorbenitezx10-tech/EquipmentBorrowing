using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Repositories;

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
            var studentRepository = new InMemoryStudentRepository();
            var equipmentRepository = new InMemoryEquipmentRepository();
            var borrowingRepository = new InMemoryBorrowingRepository();

            studentRepository.SaveAsync(new Student
            {
                Id = "STU101",
                Name = "Alice Smith",
                Email = "alice@uni.edu"
            });
            equipmentRepository.SaveAsync(new Equipment
            {
                Id = "EQ-LAP-01",
                Name = "Dell XPS Laptop",
                AssetTag = "TAG-00921"
            });
            equipmentRepository.SaveAsync(new Equipment
            {
                Id = "EQ-CAM-02",
                Name = "Canon Mirrorless Camera",
                AssetTag = "TAG-00922"
            });

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(studentRepository, equipmentRepository, borrowingRepository),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}