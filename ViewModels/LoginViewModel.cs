using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HairSalon.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;

    public event Action? LoginSucceeded;

    [RelayCommand]
    private void Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Введите логин и пароль!";
            return;
        }

        if (Username.Trim() == "admin" && Password == "admin")
        {
            ErrorMessage = string.Empty;
            LoginSucceeded?.Invoke();
        }
        else
        {
            ErrorMessage = "Неверный логин или пароль!";
        }
    }
}
