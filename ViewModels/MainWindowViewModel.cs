using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HairSalon.Data;
using HairSalon.Models;
using Microsoft.EntityFrameworkCore;

namespace HairSalon.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // === Collections ===
    public ObservableCollection<Client> Clients { get; } = new();
    public ObservableCollection<Master> Masters { get; } = new();
    public ObservableCollection<Service> Services { get; } = new();
    public ObservableCollection<Appointment> Appointments { get; } = new();

    // === Client fields ===
    [ObservableProperty] private string _newClientName = string.Empty;
    [ObservableProperty] private string _newClientPhone = string.Empty;

    // === Master fields ===
    [ObservableProperty] private string _newMasterName = string.Empty;
    [ObservableProperty] private string _newMasterSpecialization = string.Empty;
    [ObservableProperty] private string _newMasterExperience = string.Empty;

    // === Service fields ===
    [ObservableProperty] private string _newServiceName = string.Empty;
    [ObservableProperty] private string _newServicePrice = string.Empty;
    [ObservableProperty] private string _newServiceDuration = string.Empty;

    // === Appointment fields ===
    [ObservableProperty] private Client? _selectedClient;
    [ObservableProperty] private Master? _selectedMaster;
    [ObservableProperty] private Service? _selectedService;
    [ObservableProperty] private DateTimeOffset _selectedDate = DateTimeOffset.Now;
    [ObservableProperty] private int _selectedHour = 10;
    [ObservableProperty] private int _selectedMinute = 0;
    [ObservableProperty] private Appointment? _selectedAppointment;

    // === Selection for delete ===
    [ObservableProperty] private Client? _selectedClientInList;
    [ObservableProperty] private Master? _selectedMasterInList;
    [ObservableProperty] private Service? _selectedServiceInList;

    // === Status message ===
    [ObservableProperty] private string _statusMessage = string.Empty;

    // === Logout ===
    public event Action? LogoutRequested;

    public int[] Hours { get; } = Enumerable.Range(9, 12).ToArray();
    public int[] Minutes { get; } = [0, 15, 30, 45];

    public MainWindowViewModel()
    {
        InitializeDatabase();
        LoadData();
    }

    private void InitializeDatabase()
    {
        using var db = new SalonDbContext();
        db.Database.EnsureCreated();

        if (!db.Masters.Any())
        {
            db.Masters.AddRange(
                new Master { Name = "Анна Иванова", Specialization = "Стрижки, укладки", ExperienceYears = 7 },
                new Master { Name = "Елена Петрова", Specialization = "Окрашивание", ExperienceYears = 7 },
                new Master { Name = "Мария Сидорова", Specialization = "Маникюр", ExperienceYears = 4 },
                new Master { Name = "Дарья Козлова", Specialization = "Укладки, причёски", ExperienceYears = 4 },
                new Master { Name = "Наталья Волкова", Specialization = "Педикюр", ExperienceYears = 4 }
            );
            db.SaveChanges();
        }

        if (!db.Services.Any())
        {
            db.Services.AddRange(
                new Service { Name = "Стрижка женская", Price = 5500, DurationMinutes = 60 },
                new Service { Name = "Стрижка мужская", Price = 3000, DurationMinutes = 30 },
                new Service { Name = "Окрашивание", Price = 12000, DurationMinutes = 120 },
                new Service { Name = "Укладка", Price = 4000, DurationMinutes = 45 },
                new Service { Name = "Маникюр", Price = 4500, DurationMinutes = 60 }
            );
            db.SaveChanges();
        }

        if (!db.Clients.Any())
        {
            db.Clients.AddRange(
                new Client { Name = "Ольга Кузнецова", Phone = "+7 (700) 123-45-67" },
                new Client { Name = "Татьяна Смирнова", Phone = "+7 (701) 765-43-21" }
            );
            db.SaveChanges();
        }

        if (!db.Appointments.Any())
        {
            var client1 = db.Clients.First();
            var client2 = db.Clients.Skip(1).First();
            var master1 = db.Masters.First();
            var master2 = db.Masters.Skip(1).First();
            var service1 = db.Services.First();
            var service3 = db.Services.Skip(2).First();

            db.Appointments.AddRange(
                new Appointment
                {
                    Client = client1, Master = master1, Service = service1,
                    DateTime = DateTime.Today.AddHours(10), Status = "Запланирована"
                },
                new Appointment
                {
                    Client = client2, Master = master2, Service = service3,
                    DateTime = DateTime.Today.AddHours(14), Status = "Запланирована"
                }
            );
            db.SaveChanges();
        }
    }

    private void LoadData()
    {
        using var db = new SalonDbContext();

        Clients.Clear();
        foreach (var c in db.Clients.ToList()) Clients.Add(c);

        Masters.Clear();
        foreach (var m in db.Masters.ToList()) Masters.Add(m);

        Services.Clear();
        foreach (var s in db.Services.ToList()) Services.Add(s);

        Appointments.Clear();
        foreach (var a in db.Appointments.Include(a => a.Client).Include(a => a.Master).Include(a => a.Service).ToList())
            Appointments.Add(a);
    }

    // === Commands ===

    [RelayCommand]
    private void Logout()
    {
        LogoutRequested?.Invoke();
    }

    [RelayCommand]
    private void AddClient()
    {
        if (string.IsNullOrWhiteSpace(NewClientName) || string.IsNullOrWhiteSpace(NewClientPhone))
        {
            StatusMessage = "Заполните имя и телефон клиента!";
            return;
        }

        var phone = NewClientPhone.Trim();
        if (!Regex.IsMatch(phone, @"^\+?[\d]+$"))
        {
            StatusMessage = "Телефон может содержать только цифры и знак +!";
            return;
        }

        var digitCount = phone.Count(char.IsDigit);
        if (digitCount < 11 || digitCount > 12)
        {
            StatusMessage = "Номер телефона должен содержать 11–12 цифр!";
            return;
        }

        var client = new Client { Name = NewClientName.Trim(), Phone = phone };
        using var db = new SalonDbContext();
        db.Clients.Add(client);
        db.SaveChanges();

        Clients.Add(client);
        StatusMessage = $"Клиент \"{client.Name}\" добавлен.";
        NewClientName = string.Empty;
        NewClientPhone = string.Empty;
    }

    [RelayCommand]
    private void DeleteClient()
    {
        if (SelectedClientInList is null)
        {
            StatusMessage = "Выберите клиента для удаления!";
            return;
        }

        using var db = new SalonDbContext();
        var hasAppointments = db.Appointments.Any(a => EF.Property<int>(a, "ClientId") == SelectedClientInList.Id);
        if (hasAppointments)
        {
            StatusMessage = "Нельзя удалить клиента — у него есть записи!";
            return;
        }

        var entity = db.Clients.Find(SelectedClientInList.Id);
        if (entity != null)
        {
            db.Clients.Remove(entity);
            db.SaveChanges();
        }

        var name = SelectedClientInList.Name;
        Clients.Remove(SelectedClientInList);
        StatusMessage = $"Клиент \"{name}\" удалён.";
    }

    [RelayCommand]
    private void AddMaster()
    {
        if (string.IsNullOrWhiteSpace(NewMasterName) || string.IsNullOrWhiteSpace(NewMasterSpecialization))
        {
            StatusMessage = "Заполните имя и специализацию мастера!";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewMasterExperience) || !int.TryParse(NewMasterExperience, out var exp))
        {
            StatusMessage = "Стаж должен быть числом!";
            return;
        }

        if (exp < 0 || exp > 40)
        {
            StatusMessage = "Стаж должен быть от 0 до 40 лет!";
            return;
        }

        var master = new Master
        {
            Name = NewMasterName.Trim(),
            Specialization = NewMasterSpecialization.Trim(),
            ExperienceYears = exp
        };

        using var db = new SalonDbContext();
        db.Masters.Add(master);
        db.SaveChanges();

        Masters.Add(master);
        StatusMessage = $"Мастер \"{master.Name}\" добавлен.";
        NewMasterName = string.Empty;
        NewMasterSpecialization = string.Empty;
        NewMasterExperience = string.Empty;
    }

    [RelayCommand]
    private void DeleteMaster()
    {
        if (SelectedMasterInList is null)
        {
            StatusMessage = "Выберите мастера для удаления!";
            return;
        }

        using var db = new SalonDbContext();
        var hasAppointments = db.Appointments.Any(a => EF.Property<int>(a, "MasterId") == SelectedMasterInList.Id);
        if (hasAppointments)
        {
            StatusMessage = "Нельзя удалить мастера — у него есть записи!";
            return;
        }

        var entity = db.Masters.Find(SelectedMasterInList.Id);
        if (entity != null)
        {
            db.Masters.Remove(entity);
            db.SaveChanges();
        }

        var name = SelectedMasterInList.Name;
        Masters.Remove(SelectedMasterInList);
        StatusMessage = $"Мастер \"{name}\" удалён.";
    }

    [RelayCommand]
    private void AddService()
    {
        if (string.IsNullOrWhiteSpace(NewServiceName) ||
            !decimal.TryParse(NewServicePrice, out var price) ||
            !int.TryParse(NewServiceDuration, out var duration))
        {
            StatusMessage = "Заполните все поля услуги корректно!";
            return;
        }

        var service = new Service
        {
            Name = NewServiceName.Trim(),
            Price = price,
            DurationMinutes = duration
        };

        using var db = new SalonDbContext();
        db.Services.Add(service);
        db.SaveChanges();

        Services.Add(service);
        StatusMessage = $"Услуга \"{service.Name}\" добавлена.";
        NewServiceName = string.Empty;
        NewServicePrice = string.Empty;
        NewServiceDuration = string.Empty;
    }

    [RelayCommand]
    private void DeleteService()
    {
        if (SelectedServiceInList is null)
        {
            StatusMessage = "Выберите услугу для удаления!";
            return;
        }

        using var db = new SalonDbContext();
        var hasAppointments = db.Appointments.Any(a => EF.Property<int>(a, "ServiceId") == SelectedServiceInList.Id);
        if (hasAppointments)
        {
            StatusMessage = "Нельзя удалить услугу — она используется в записях!";
            return;
        }

        var entity = db.Services.Find(SelectedServiceInList.Id);
        if (entity != null)
        {
            db.Services.Remove(entity);
            db.SaveChanges();
        }

        var name = SelectedServiceInList.Name;
        Services.Remove(SelectedServiceInList);
        StatusMessage = $"Услуга \"{name}\" удалена.";
    }

    [RelayCommand]
    private void AddAppointment()
    {
        if (SelectedClient is null || SelectedMaster is null || SelectedService is null)
        {
            StatusMessage = "Выберите клиента, мастера и услугу!";
            return;
        }

        var appointmentDate = SelectedDate.Date
            .AddHours(SelectedHour)
            .AddMinutes(SelectedMinute);

        var appointment = new Appointment
        {
            Client = SelectedClient,
            Master = SelectedMaster,
            Service = SelectedService,
            DateTime = appointmentDate,
            Status = "Запланирована"
        };

        using var db = new SalonDbContext();
        db.Attach(appointment.Client);
        db.Attach(appointment.Master);
        db.Attach(appointment.Service);
        db.Appointments.Add(appointment);
        db.SaveChanges();

        Appointments.Add(appointment);
        StatusMessage = $"Запись для \"{SelectedClient.Name}\" на {appointmentDate:dd.MM.yyyy HH:mm} создана.";
    }

    [RelayCommand]
    private void CompleteAppointment()
    {
        if (SelectedAppointment is null)
        {
            StatusMessage = "Выберите запись!";
            return;
        }

        using var db = new SalonDbContext();
        var entity = db.Appointments.Find(SelectedAppointment.Id);
        if (entity != null)
        {
            entity.Status = "Выполнена";
            db.SaveChanges();
        }

        SelectedAppointment.Status = "Выполнена";
        StatusMessage = $"Запись #{SelectedAppointment.Id} отмечена как выполненная.";
        RefreshAppointments();
    }

    [RelayCommand]
    private void CancelAppointment()
    {
        if (SelectedAppointment is null)
        {
            StatusMessage = "Выберите запись!";
            return;
        }

        using var db = new SalonDbContext();
        var entity = db.Appointments.Find(SelectedAppointment.Id);
        if (entity != null)
        {
            entity.Status = "Отменена";
            db.SaveChanges();
        }

        SelectedAppointment.Status = "Отменена";
        StatusMessage = $"Запись #{SelectedAppointment.Id} отменена.";
        RefreshAppointments();
    }

    [RelayCommand]
    private void DeleteAppointment()
    {
        if (SelectedAppointment is null)
        {
            StatusMessage = "Выберите запись для удаления!";
            return;
        }

        using var db = new SalonDbContext();
        var entity = db.Appointments.Find(SelectedAppointment.Id);
        if (entity != null)
        {
            db.Appointments.Remove(entity);
            db.SaveChanges();
        }

        var id = SelectedAppointment.Id;
        Appointments.Remove(SelectedAppointment);
        StatusMessage = $"Запись #{id} удалена.";
    }

    private void RefreshAppointments()
    {
        var items = Appointments.ToList();
        Appointments.Clear();
        foreach (var item in items)
            Appointments.Add(item);
    }
}
