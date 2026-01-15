using System.Collections.ObjectModel;
using System.Windows.Input;
using LibraryManagementSystem.Commands;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.ViewModels;

/// <summary>
/// ViewModel for the dashboard showing user's personal statistics
/// </summary>
public class DashboardViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;

    public DashboardViewModel(LibraryService libraryService)
    {
        _libraryService = libraryService;
        MyRecentIssues = new ObservableCollection<BookIssue>();
        MyOverdueBooks = new ObservableCollection<BookIssue>();

        LoadDataCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
    }

    #region Properties

    public ObservableCollection<BookIssue> MyRecentIssues { get; }
    public ObservableCollection<BookIssue> MyOverdueBooks { get; }

    /// <summary>
    /// ID-ul membrului utilizatorului curent
    /// </summary>
    private int? CurrentMemberId => AuthenticationService.CurrentUserMemberId;

    /// <summary>
    /// Mesaj de bun venit personalizat
    /// </summary>
    public string WelcomeMessage
    {
        get
        {
            var user = AuthenticationService.CurrentUser;
            if (user == null) return "";
            return $"Bine ai venit, {user.FullName}!";
        }
    }

    /// <summary>
    /// Tipul de membru al utilizatorului
    /// </summary>
    public string MemberTypeDisplay
    {
        get
        {
            var user = AuthenticationService.CurrentUser;
            if (user == null) return "";
            return user.UserMemberType == MemberType.Professor ? "Profesor" : "Student";
        }
    }

    /// <summary>
    /// Info despre limita de carti
    /// </summary>
    public string MaxBooksInfo
    {
        get
        {
            var user = AuthenticationService.CurrentUser;
            if (user == null) return "";
            return $"Maxim {user.MaxBooksAllowed} carti";
        }
    }

    /// <summary>
    /// Info despre durata maxima
    /// </summary>
    public string MaxDaysInfo
    {
        get
        {
            var user = AuthenticationService.CurrentUser;
            if (user == null) return "";
            return $"Durata maxima: {user.MaxIssueDays} zile";
        }
    }

    /// <summary>
    /// Text pentru limita de carti
    /// </summary>
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

            if (CurrentMemberId.HasValue)
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

                OnPropertyChanged(nameof(HasNoActiveIssues));
            }
            else
            {
                MyActiveIssues = 0;
                MyOverdueCount = 0;
                MyPendingFines = 0;
                MyRecentIssues.Clear();
                MyOverdueBooks.Clear();
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

    #endregion
}
