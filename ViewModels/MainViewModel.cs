using System.Collections.ObjectModel;
using System.Windows.Input;
using LibraryManagementSystem.Commands;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.ViewModels;

/// <summary>
/// Main ViewModel that manages navigation and overall application state
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;
    private readonly AuthenticationService _authService;
    private readonly LibraryDbContext _context;

    public MainViewModel()
    {
        _context = new LibraryDbContext();
        _libraryService = new LibraryService(_context);
        _authService = new AuthenticationService(_context);

        // Initialize sub-ViewModels
        LoginViewModel = new LoginViewModel();
        LoginViewModel.LoginSuccessful += OnLoginSuccessful;

        BooksViewModel = new BooksViewModel(_libraryService);
        MembersViewModel = new MembersViewModel(_libraryService);
        IssuesViewModel = new IssuesViewModel(_libraryService);
        FinesViewModel = new FinesViewModel(_libraryService);
        DashboardViewModel = new DashboardViewModel(_libraryService);

        // Set initial view to Login
        CurrentViewModel = LoginViewModel;

        // Initialize commands
        NavigateToDashboardCommand = new RelayCommand(_ => NavigateTo(DashboardViewModel));
        NavigateToBooksCommand = new RelayCommand(_ => NavigateTo(BooksViewModel));
        NavigateToMembersCommand = new RelayCommand(_ => NavigateTo(MembersViewModel));
        NavigateToIssuesCommand = new RelayCommand(_ => NavigateTo(IssuesViewModel));
        NavigateToFinesCommand = new RelayCommand(_ => NavigateTo(FinesViewModel));
        LogoutCommand = new RelayCommand(_ => Logout());

        // Ensure database is created
        InitializeDatabaseAsync();
    }

    private void OnLoginSuccessful(object? sender, EventArgs e)
    {
        IsAuthenticated = true;
        OnPropertyChanged(nameof(IsAdmin));
        OnPropertyChanged(nameof(CurrentUserName));
        OnPropertyChanged(nameof(CurrentUserRole));
        NavigateTo(DashboardViewModel);
    }

    private void Logout()
    {
        _authService.Logout();
        IsAuthenticated = false;
        OnPropertyChanged(nameof(IsAdmin));
        OnPropertyChanged(nameof(CurrentUserName));
        OnPropertyChanged(nameof(CurrentUserRole));
        CurrentViewModel = LoginViewModel;
    }

    private async void InitializeDatabaseAsync()
    {
        try
        {
            IsBusy = true;
            await _context.Database.EnsureCreatedAsync();
            await DashboardViewModel.LoadDataAsync();
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la inițializarea bazei de date: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    #region ViewModels

    public LoginViewModel LoginViewModel { get; }
    public BooksViewModel BooksViewModel { get; }
    public MembersViewModel MembersViewModel { get; }
    public IssuesViewModel IssuesViewModel { get; }
    public FinesViewModel FinesViewModel { get; }
    public DashboardViewModel DashboardViewModel { get; }

    private ViewModelBase _currentViewModel = null!;
    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    #endregion

    #region Authentication Properties

    private bool _isAuthenticated;
    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        set => SetProperty(ref _isAuthenticated, value);
    }

    /// <summary>
    /// Verifică dacă utilizatorul curent este administrator
    /// </summary>
    public bool IsAdmin => AuthenticationService.IsAdmin;

    /// <summary>
    /// Numele utilizatorului curent
    /// </summary>
    public string CurrentUserName => AuthenticationService.CurrentUser?.FullName ?? "";

    /// <summary>
    /// Rolul utilizatorului curent
    /// </summary>
    public string CurrentUserRole => AuthenticationService.CurrentUser?.Role.ToString() ?? "";

    #endregion

    #region Navigation Commands

    public ICommand NavigateToDashboardCommand { get; }
    public ICommand NavigateToBooksCommand { get; }
    public ICommand NavigateToMembersCommand { get; }
    public ICommand NavigateToIssuesCommand { get; }
    public ICommand NavigateToFinesCommand { get; }
    public ICommand LogoutCommand { get; }

    private void NavigateTo(ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;

        // Load data when navigating
        if (viewModel is BooksViewModel booksVm)
            _ = booksVm.LoadDataAsync();
        else if (viewModel is MembersViewModel membersVm)
            _ = membersVm.LoadDataAsync();
        else if (viewModel is IssuesViewModel issuesVm)
            _ = issuesVm.LoadDataAsync();
        else if (viewModel is FinesViewModel finesVm)
            _ = finesVm.LoadDataAsync();
        else if (viewModel is DashboardViewModel dashboardVm)
            _ = dashboardVm.LoadDataAsync();
    }

    #endregion
}
