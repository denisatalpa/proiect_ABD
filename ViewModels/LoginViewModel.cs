using System.Windows.Input;
using LibraryManagementSystem.Commands;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.ViewModels;


/// ViewModel pentru ecranul de autentificare/înregistrare

public class LoginViewModel : ViewModelBase
{
    private readonly AuthenticationService _authService;

    public LoginViewModel()
    {
        var context = new LibraryDbContext();
        _authService = new AuthenticationService(context);

        LoginCommand = new AsyncRelayCommand(async _ => await LoginAsync());
        RegisterCommand = new AsyncRelayCommand(async _ => await RegisterAsync());
        SwitchToRegisterCommand = new RelayCommand(_ => SwitchToRegister());
        SwitchToLoginCommand = new RelayCommand(_ => SwitchToLogin());

        // Asigură că există admin-ul implicit
        Task.Run(async () => await _authService.EnsureAdminExistsAsync());
    }

    #region Login Properties

    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    #endregion

    #region Register Properties

    private string _registerUsername = string.Empty;
    public string RegisterUsername
    {
        get => _registerUsername;
        set => SetProperty(ref _registerUsername, value);
    }

    private string _registerEmail = string.Empty;
    public string RegisterEmail
    {
        get => _registerEmail;
        set => SetProperty(ref _registerEmail, value);
    }

    private string _registerPassword = string.Empty;
    public string RegisterPassword
    {
        get => _registerPassword;
        set => SetProperty(ref _registerPassword, value);
    }

    private string _registerConfirmPassword = string.Empty;
    public string RegisterConfirmPassword
    {
        get => _registerConfirmPassword;
        set => SetProperty(ref _registerConfirmPassword, value);
    }

    private string _registerFirstName = string.Empty;
    public string RegisterFirstName
    {
        get => _registerFirstName;
        set => SetProperty(ref _registerFirstName, value);
    }

    private string _registerLastName = string.Empty;
    public string RegisterLastName
    {
        get => _registerLastName;
        set => SetProperty(ref _registerLastName, value);
    }

    private bool _isStudent = true;
    public bool IsStudent
    {
        get => _isStudent;
        set
        {
            if (SetProperty(ref _isStudent, value))
            {
                OnPropertyChanged(nameof(IsProfessor));
            }
        }
    }

    public bool IsProfessor
    {
        get => !_isStudent;
        set
        {
            IsStudent = !value;
        }
    }

    #endregion

    #region View State

    private bool _isRegisterMode;
    public bool IsRegisterMode
    {
        get => _isRegisterMode;
        set => SetProperty(ref _isRegisterMode, value);
    }

    public bool IsLoginMode => !IsRegisterMode;

    #endregion

    #region Commands

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand SwitchToRegisterCommand { get; }
    public ICommand SwitchToLoginCommand { get; }

    #endregion

    #region Events

    
    /// Eveniment declanșat când autentificarea reușește
    
    public event EventHandler? LoginSuccessful;

    #endregion

    #region Methods

    private async Task LoginAsync()
    {
        try
        {
            IsBusy = true;
            ClearStatus();

            var (success, message, user) = await _authService.LoginAsync(Username, Password);

            if (success)
            {
                SetStatus(message, false);
                LoginSuccessful?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                SetStatus(message, true);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la autentificare: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RegisterAsync()
    {
        try
        {
            IsBusy = true;
            ClearStatus();

            var memberType = IsStudent ? MemberType.Student : MemberType.Professor;
            var (success, message, user) = await _authService.RegisterAsync(
                RegisterUsername,
                RegisterEmail,
                RegisterPassword,
                RegisterConfirmPassword,
                memberType,
                RegisterFirstName,
                RegisterLastName);

            if (success)
            {
                SetStatus(message, false);
                // Comută la modul de login după înregistrare reușită
                Username = RegisterUsername;
                Password = string.Empty;
                ClearRegisterFields();
                IsRegisterMode = false;
                OnPropertyChanged(nameof(IsLoginMode));
            }
            else
            {
                SetStatus(message, true);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la înregistrare: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SwitchToRegister()
    {
        IsRegisterMode = true;
        OnPropertyChanged(nameof(IsLoginMode));
        ClearStatus();
    }

    private void SwitchToLogin()
    {
        IsRegisterMode = false;
        OnPropertyChanged(nameof(IsLoginMode));
        ClearStatus();
    }

    private void ClearRegisterFields()
    {
        RegisterUsername = string.Empty;
        RegisterEmail = string.Empty;
        RegisterPassword = string.Empty;
        RegisterConfirmPassword = string.Empty;
        RegisterFirstName = string.Empty;
        RegisterLastName = string.Empty;
    }

    private void ClearStatus()
    {
        StatusMessage = string.Empty;
        IsError = false;
    }

    #endregion
}
