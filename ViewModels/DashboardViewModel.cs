using System.Collections.ObjectModel;
using System.Windows.Input;
using LibraryManagementSystem.Commands;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.ViewModels;

/// <summary>
/// ViewModel for the dashboard showing library statistics
/// </summary>
public class DashboardViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;

    public DashboardViewModel(LibraryService libraryService)
    {
        _libraryService = libraryService;
        RecentIssues = new ObservableCollection<BookIssue>();
        OverdueBooks = new ObservableCollection<BookIssue>();

        LoadDataCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
    }

    #region Properties

    public ObservableCollection<BookIssue> RecentIssues { get; }
    public ObservableCollection<BookIssue> OverdueBooks { get; }

    private int _totalBooks;
    public int TotalBooks
    {
        get => _totalBooks;
        set => SetProperty(ref _totalBooks, value);
    }

    private int _availableBooks;
    public int AvailableBooks
    {
        get => _availableBooks;
        set => SetProperty(ref _availableBooks, value);
    }

    private int _totalMembers;
    public int TotalMembers
    {
        get => _totalMembers;
        set => SetProperty(ref _totalMembers, value);
    }

    private int _totalStudents;
    public int TotalStudents
    {
        get => _totalStudents;
        set => SetProperty(ref _totalStudents, value);
    }

    private int _totalFaculty;
    public int TotalFaculty
    {
        get => _totalFaculty;
        set => SetProperty(ref _totalFaculty, value);
    }

    private int _activeIssues;
    public int ActiveIssues
    {
        get => _activeIssues;
        set => SetProperty(ref _activeIssues, value);
    }

    private int _overdueCount;
    public int OverdueCount
    {
        get => _overdueCount;
        set => SetProperty(ref _overdueCount, value);
    }

    private decimal _pendingFines;
    public decimal PendingFines
    {
        get => _pendingFines;
        set => SetProperty(ref _pendingFines, value);
    }

    private decimal _totalFinesCollected;
    public decimal TotalFinesCollected
    {
        get => _totalFinesCollected;
        set => SetProperty(ref _totalFinesCollected, value);
    }

    // Student vs Faculty comparison info
    private string _studentLimits = "Students: Max 3 books for 14 days";
    public string StudentLimits
    {
        get => _studentLimits;
        set => SetProperty(ref _studentLimits, value);
    }

    private string _facultyLimits = "Faculty: Max 10 books for 30 days";
    public string FacultyLimits
    {
        get => _facultyLimits;
        set => SetProperty(ref _facultyLimits, value);
    }

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

            // Get statistics
            var stats = await _libraryService.GetStatisticsAsync();

            TotalBooks = stats.TotalBooks;
            AvailableBooks = stats.AvailableBooks;
            TotalMembers = stats.TotalMembers;
            TotalStudents = stats.TotalStudents;
            TotalFaculty = stats.TotalFaculty;
            ActiveIssues = stats.ActiveIssues;
            OverdueCount = stats.OverdueBooks;
            PendingFines = stats.PendingFines;
            TotalFinesCollected = stats.TotalFinesCollected;

            // Get recent issues
            var issues = await _libraryService.GetActiveIssuesAsync();
            RecentIssues.Clear();
            foreach (var issue in issues.Take(5))
            {
                RecentIssues.Add(issue);
            }

            // Get overdue books
            var overdue = await _libraryService.GetOverdueIssuesAsync();
            OverdueBooks.Clear();
            foreach (var issue in overdue.Take(5))
            {
                OverdueBooks.Add(issue);
            }

            SetStatus("Panoul principal actualizat");
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la încărcarea panoului principal: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion
}
