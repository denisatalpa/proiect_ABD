using System.Collections.ObjectModel;
using System.Windows.Input;
using LibraryManagementSystem.Commands;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.ViewModels;


/// ViewModel for the dashboard showing user's personal statistics

public class DashboardViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;

    public DashboardViewModel(LibraryService libraryService)
    {
        _libraryService = libraryService;
        MyRecentIssues = new ObservableCollection<BookIssue>();
        MyOverdueBooks = new ObservableCollection<BookIssue>();
        AllActiveMembers = new ObservableCollection<Member>();

        LoadDataCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
    }

    #region Properties

    public ObservableCollection<BookIssue> MyRecentIssues { get; }
    public ObservableCollection<BookIssue> MyOverdueBooks { get; }
    public ObservableCollection<Member> AllActiveMembers { get; }

    
    /// ID-ul membrului utilizatorului curent
    
    private int? CurrentMemberId => AuthenticationService.CurrentUserMemberId;

    
    /// Verifică dacă utilizatorul curent este administrator
    
    public bool IsAdmin => AuthenticationService.IsAdmin;

    
    /// Verifică dacă utilizatorul curent NU este administrator
    
    public bool IsNotAdmin => !IsAdmin;

    
    /// Titlul dashboard-ului
    
    public string DashboardTitle => IsAdmin ? "Panou Admin" : "Panoul Meu";

    
    /// Titlul secțiunii de împrumuturi active
    
    public string ActiveLoansTitle => IsAdmin ? "Toate Imprumuturile Active" : "Imprumuturile Mele Active";

    
    /// Titlul secțiunii de avertizare întârzieri
    
    public string OverdueWarningTitle => IsAdmin ? "Carti Intarziate in Biblioteca" : "Atentie: Carti Intarziate!";

    
    /// Textul afișat când nu există împrumuturi active
    
    public string NoActiveIssuesText => IsAdmin ? "Nu exista imprumuturi active in biblioteca" : "Nu ai imprumuturi active";

    
    /// Mesaj de bun venit personalizat
    
    public string WelcomeMessage
    {
        get
        {
            var user = AuthenticationService.CurrentUser;
            if (user == null) return "";
            if (IsAdmin) return $"Bine ai venit, Administrator {user.FullName}!";
            return $"Bine ai venit, {user.FullName}!";
        }
    }

    
    /// Tipul de membru al utilizatorului
    
    public string MemberTypeDisplay
    {
        get
        {
            var user = AuthenticationService.CurrentUser;
            if (user == null) return "";
            if (IsAdmin) return "Administrator";
            return user.UserMemberType == MemberType.Professor ? "Profesor" : "Student";
        }
    }

    
    /// Info despre limita de carti
    
    public string MaxBooksInfo
    {
        get
        {
            var user = AuthenticationService.CurrentUser;
            if (user == null) return "";
            return $"Maxim {user.MaxBooksAllowed} carti";
        }
    }

    
    /// Info despre durata maxima
    
    public string MaxDaysInfo
    {
        get
        {
            var user = AuthenticationService.CurrentUser;
            if (user == null) return "";
            return $"Durata maxima: {user.MaxIssueDays} zile";
        }
    }

    
    /// Text pentru limita de carti
    
    public string MyBooksLimit
    {
        get
        {
            var user = AuthenticationService.CurrentUser;
            if (user == null) return "";
            return $"din maxim {user.MaxBooksAllowed}";
        }
    }

    private int _myActiveIssues;
    public int MyActiveIssues
    {
        get => _myActiveIssues;
        set => SetProperty(ref _myActiveIssues, value);
    }

    private int _myOverdueCount;
    public int MyOverdueCount
    {
        get => _myOverdueCount;
        set
        {
            if (SetProperty(ref _myOverdueCount, value))
            {
                OnPropertyChanged(nameof(HasOverdueBooks));
            }
        }
    }

    private decimal _myPendingFines;
    public decimal MyPendingFines
    {
        get => _myPendingFines;
        set => SetProperty(ref _myPendingFines, value);
    }

    public bool HasOverdueBooks => MyOverdueCount > 0;
    public bool HasNoActiveIssues => MyActiveIssues == 0;

    #endregion

    #region Commands

    public ICommand LoadDataCommand { get; }

    #endregion

    #region Methods

    public async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;

            // Refresh user info
            OnPropertyChanged(nameof(WelcomeMessage));
            OnPropertyChanged(nameof(MemberTypeDisplay));
            OnPropertyChanged(nameof(MaxBooksInfo));
            OnPropertyChanged(nameof(MaxDaysInfo));
            OnPropertyChanged(nameof(MyBooksLimit));
            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(IsNotAdmin));
            OnPropertyChanged(nameof(DashboardTitle));
            OnPropertyChanged(nameof(ActiveLoansTitle));
            OnPropertyChanged(nameof(OverdueWarningTitle));
            OnPropertyChanged(nameof(NoActiveIssuesText));

            if (IsAdmin)
            {
                // Admin vede toate datele din bibliotecă
                await LoadAdminDataAsync();
            }
            else if (CurrentMemberId.HasValue)
            {
                // Get user's issues
                var myIssues = await _libraryService.GetMemberIssuesAsync(CurrentMemberId.Value);
                var activeIssues = myIssues.Where(i => !i.IsReturned).ToList();
                var overdueIssues = myIssues.Where(i => i.IsOverdue).ToList();

                MyActiveIssues = activeIssues.Count;
                MyOverdueCount = overdueIssues.Count;

                // Get user's fines
                var myFines = await _libraryService.GetMemberFinesAsync(CurrentMemberId.Value);
                MyPendingFines = myFines
                    .Where(f => f.Status == "Pending" || f.Status == "Partial")
                    .Sum(f => f.RemainingAmount);

                // Load recent active issues
                MyRecentIssues.Clear();
                foreach (var issue in activeIssues.OrderByDescending(i => i.IssueDate).Take(5))
                {
                    MyRecentIssues.Add(issue);
                }

                // Load overdue books
                MyOverdueBooks.Clear();
                foreach (var issue in overdueIssues.OrderByDescending(i => i.DaysOverdue))
                {
                    MyOverdueBooks.Add(issue);
                }

                AllActiveMembers.Clear();
                OnPropertyChanged(nameof(HasNoActiveIssues));
            }
            else
            {
                MyActiveIssues = 0;
                MyOverdueCount = 0;
                MyPendingFines = 0;
                MyRecentIssues.Clear();
                MyOverdueBooks.Clear();
                AllActiveMembers.Clear();
            }

            SetStatus("Panoul actualizat");
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la incarcarea panoului: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadAdminDataAsync()
    {
        // Get all active issues from the library
        var allActiveIssues = await _libraryService.GetActiveIssuesAsync();
        var allOverdueIssues = await _libraryService.GetOverdueIssuesAsync();

        MyActiveIssues = allActiveIssues.Count;
        MyOverdueCount = allOverdueIssues.Count;

        // Get all pending fines from the library
        var allFines = await _libraryService.GetPendingFinesAsync();
        MyPendingFines = allFines.Sum(f => f.RemainingAmount);

        // Load all active issues
        MyRecentIssues.Clear();
        foreach (var issue in allActiveIssues.OrderByDescending(i => i.IssueDate))
        {
            MyRecentIssues.Add(issue);
        }

        // Load all overdue books
        MyOverdueBooks.Clear();
        foreach (var issue in allOverdueIssues.OrderByDescending(i => i.DaysOverdue))
        {
            MyOverdueBooks.Add(issue);
        }

        // Load all active members
        var allMembers = await _libraryService.GetAllMembersAsync();
        AllActiveMembers.Clear();
        foreach (var member in allMembers)
        {
            AllActiveMembers.Add(member);
        }

        OnPropertyChanged(nameof(HasNoActiveIssues));
    }

    #endregion
}
