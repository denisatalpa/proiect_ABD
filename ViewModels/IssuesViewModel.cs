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
        Members = new ObservableCollection<Member>();

        // Initialize commands
        LoadDataCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
        IssueBookCommand = new AsyncRelayCommand(async _ => await IssueBookAsync(), _ => CanIssueBook());
        ReturnBookCommand = new AsyncRelayCommand(async _ => await ReturnBookAsync(), _ => CanReturnBook());
        ShowActiveIssuesCommand = new AsyncRelayCommand(async _ => await ShowActiveIssuesAsync());
        ShowOverdueIssuesCommand = new AsyncRelayCommand(async _ => await ShowOverdueIssuesAsync());
        ShowAllIssuesCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
        RefreshBooksCommand = new AsyncRelayCommand(async _ => await LoadAvailableBooksAsync());
    }

    #region Properties

    public ObservableCollection<BookIssue> BookIssues { get; }
    public ObservableCollection<Book> AvailableBooks { get; }
    public ObservableCollection<Member> Members { get; }

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

    private Member? _selectedMember;
    public Member? SelectedMember
    {
        get => _selectedMember;
        set
        {
            if (SetProperty(ref _selectedMember, value) && value != null)
            {
                MemberInfo = $"Type: {value.MemberType} | Max Books: {value.MaxBooksAllowed} | Max Days: {value.MaxIssueDays}";
            }
            else
            {
                MemberInfo = string.Empty;
            }
        }
    }

    private string _memberInfo = string.Empty;
    public string MemberInfo
    {
        get => _memberInfo;
        set => SetProperty(ref _memberInfo, value);
    }

    private string _issuedBy = "Librarian";
    public string IssuedBy
    {
        get => _issuedBy;
        set => SetProperty(ref _issuedBy, value);
    }

    #endregion

    #region Commands

    public ICommand LoadDataCommand { get; }
    public ICommand IssueBookCommand { get; }
    public ICommand ReturnBookCommand { get; }
    public ICommand ShowActiveIssuesCommand { get; }
    public ICommand ShowOverdueIssuesCommand { get; }
    public ICommand ShowAllIssuesCommand { get; }
    public ICommand RefreshBooksCommand { get; }

    #endregion

    #region Methods

    public async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;
            
            // Load all issues
            var issues = await _libraryService.GetAllIssuesAsync();
            BookIssues.Clear();
            foreach (var issue in issues)
            {
                BookIssues.Add(issue);
            }

            // Load available books
            await LoadAvailableBooksAsync();

            // Load members
            var members = await _libraryService.GetAllMembersAsync();
            Members.Clear();
            foreach (var member in members)
            {
                Members.Add(member);
            }

            SetStatus($"S-au încărcat {issues.Count} împrumuturi, {AvailableBooks.Count} cărți disponibile");
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la încărcarea datelor: {ex.Message}", true);
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
        return SelectedBookToIssue != null && SelectedMember != null;
    }

    private async Task IssueBookAsync()
    {
        if (SelectedBookToIssue == null || SelectedMember == null) return;

        try
        {
            IsBusy = true;

            var result = await _libraryService.IssueBookAsync(
                SelectedBookToIssue.BookId, 
                SelectedMember.MemberId, 
                IssuedBy);

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
            SetStatus($"Eroare la împrumutarea cărții: {ex.Message}", true);
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

            var result = await _libraryService.ReturnBookAsync(SelectedIssue.IssueId, IssuedBy);

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
            SetStatus($"Eroare la returnarea cărții: {ex.Message}", true);
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
            var issues = await _libraryService.GetActiveIssuesAsync();
            
            BookIssues.Clear();
            foreach (var issue in issues)
            {
                BookIssues.Add(issue);
            }

            SetStatus($"Se afișează {issues.Count} împrumuturi active");
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
            var issues = await _libraryService.GetOverdueIssuesAsync();
            
            BookIssues.Clear();
            foreach (var issue in issues)
            {
                BookIssues.Add(issue);
            }

            SetStatus($"Se afișează {issues.Count} împrumuturi restante");
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
