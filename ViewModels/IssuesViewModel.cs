using System.Collections.ObjectModel;
using System.Windows.Input;
using LibraryManagementSystem.Commands;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.ViewModels;

/// <summary>
/// ViewModel for managing book issues and returns
/// </summary>
public class IssuesViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;

    public IssuesViewModel(LibraryService libraryService)
    {
        _libraryService = libraryService;
        BookIssues = new ObservableCollection<BookIssue>();
        AvailableBooks = new ObservableCollection<Book>();

        // Initialize commands
        LoadDataCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
        IssueBookCommand = new AsyncRelayCommand(async _ => await IssueBookAsync(), _ => CanIssueBook());
        ReturnBookCommand = new AsyncRelayCommand(async _ => await ReturnBookAsync(), _ => CanReturnBook());
        ShowActiveIssuesCommand = new AsyncRelayCommand(async _ => await ShowActiveIssuesAsync());
        ShowOverdueIssuesCommand = new AsyncRelayCommand(async _ => await ShowOverdueIssuesAsync());
        ShowAllIssuesCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
    }

    #region Properties

    public ObservableCollection<BookIssue> BookIssues { get; }
    public ObservableCollection<Book> AvailableBooks { get; }

    private BookIssue? _selectedIssue;
    public BookIssue? SelectedIssue
    {
        get => _selectedIssue;
        set => SetProperty(ref _selectedIssue, value);
    }

    private Book? _selectedBookToIssue;
    public Book? SelectedBookToIssue
    {
        get => _selectedBookToIssue;
        set => SetProperty(ref _selectedBookToIssue, value);
    }

    /// <summary>
    /// Informatii despre utilizatorul curent
    /// </summary>
    public string CurrentUserInfo
    {
        get
        {
            var user = AuthenticationService.CurrentUser;
            if (user == null) return "";

            var memberType = user.UserMemberType == MemberType.Professor ? "Profesor" : "Student";
            return $"Imprumut pentru: {user.FullName} ({memberType}) - Max {user.MaxBooksAllowed} carti, {user.MaxIssueDays} zile";
        }
    }

    /// <summary>
    /// ID-ul membrului utilizatorului curent
    /// </summary>
    private int? CurrentMemberId => AuthenticationService.CurrentUserMemberId;

    #endregion

    #region Commands

    public ICommand LoadDataCommand { get; }
    public ICommand IssueBookCommand { get; }
    public ICommand ReturnBookCommand { get; }
    public ICommand ShowActiveIssuesCommand { get; }
    public ICommand ShowOverdueIssuesCommand { get; }
    public ICommand ShowAllIssuesCommand { get; }

    #endregion

    #region Methods

    public async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;

            // Load available books
            await LoadAvailableBooksAsync();

            // Load issues for current user only
            if (CurrentMemberId.HasValue)
            {
                var issues = await _libraryService.GetMemberIssuesAsync(CurrentMemberId.Value);
                BookIssues.Clear();
                foreach (var issue in issues.OrderByDescending(i => i.IssueDate))
                {
                    BookIssues.Add(issue);
                }
                SetStatus($"S-au incarcat {issues.Count} imprumuturi");
            }
            else
            {
                BookIssues.Clear();
                SetStatus("Nu exista membru asociat contului", true);
            }

            OnPropertyChanged(nameof(CurrentUserInfo));
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la incarcarea datelor: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadAvailableBooksAsync()
    {
        var availableBooks = await _libraryService.GetAvailableBooksAsync();
        AvailableBooks.Clear();
        foreach (var book in availableBooks)
        {
            AvailableBooks.Add(book);
        }
    }

    private bool CanIssueBook()
    {
        return SelectedBookToIssue != null && CurrentMemberId.HasValue;
    }

    private async Task IssueBookAsync()
    {
        if (SelectedBookToIssue == null || !CurrentMemberId.HasValue) return;

        try
        {
            IsBusy = true;

            var result = await _libraryService.IssueBookAsync(
                SelectedBookToIssue.BookId,
                CurrentMemberId.Value,
                AuthenticationService.CurrentUser?.FullName ?? "Utilizator");

            if (result.Success)
            {
                await LoadDataAsync();
                SelectedBookToIssue = null;
                SetStatus(result.Message);
            }
            else
            {
                SetStatus(result.Message, true);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la imprumutarea cartii: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanReturnBook()
    {
        return SelectedIssue != null && !SelectedIssue.IsReturned;
    }

    private async Task ReturnBookAsync()
    {
        if (SelectedIssue == null) return;

        try
        {
            IsBusy = true;

            var result = await _libraryService.ReturnBookAsync(
                SelectedIssue.IssueId,
                AuthenticationService.CurrentUser?.FullName ?? "Utilizator");

            if (result.Success)
            {
                await LoadDataAsync();
                SelectedIssue = null;
                SetStatus(result.Message);
            }
            else
            {
                SetStatus(result.Message, true);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la returnarea cartii: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ShowActiveIssuesAsync()
    {
        try
        {
            IsBusy = true;

            if (CurrentMemberId.HasValue)
            {
                var allIssues = await _libraryService.GetMemberIssuesAsync(CurrentMemberId.Value);
                var activeIssues = allIssues.Where(i => !i.IsReturned).ToList();

                BookIssues.Clear();
                foreach (var issue in activeIssues.OrderByDescending(i => i.IssueDate))
                {
                    BookIssues.Add(issue);
                }

                SetStatus($"Se afiseaza {activeIssues.Count} imprumuturi active");
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ShowOverdueIssuesAsync()
    {
        try
        {
            IsBusy = true;

            if (CurrentMemberId.HasValue)
            {
                var allIssues = await _libraryService.GetMemberIssuesAsync(CurrentMemberId.Value);
                var overdueIssues = allIssues.Where(i => i.IsOverdue).ToList();

                BookIssues.Clear();
                foreach (var issue in overdueIssues.OrderByDescending(i => i.DaysOverdue))
                {
                    BookIssues.Add(issue);
                }

                SetStatus($"Se afiseaza {overdueIssues.Count} imprumuturi intarziate");
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion
}
