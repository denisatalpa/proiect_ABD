using System.Collections.ObjectModel;
using System.Windows.Input;
using LibraryManagementSystem.Commands;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.ViewModels;

/// <summary>
/// ViewModel for managing books
/// </summary>
public class BooksViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;

    public BooksViewModel(LibraryService libraryService)
    {
        _libraryService = libraryService;
        Books = new ObservableCollection<Book>();

        // Initialize commands
        LoadDataCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
        AddBookCommand = new AsyncRelayCommand(async _ => await AddBookAsync());
        UpdateBookCommand = new AsyncRelayCommand(async _ => await UpdateBookAsync(), _ => SelectedBook != null);
        DeleteBookCommand = new AsyncRelayCommand(async _ => await DeleteBookAsync(), _ => SelectedBook != null);
        SearchCommand = new AsyncRelayCommand(async _ => await SearchBooksAsync());
        ClearSearchCommand = new RelayCommand(_ => ClearSearch());
        NewBookCommand = new RelayCommand(_ => PrepareNewBook());
    }

    #region Properties

    public ObservableCollection<Book> Books { get; }

    private Book? _selectedBook;
    public Book? SelectedBook
    {
        get => _selectedBook;
        set
        {
            if (SetProperty(ref _selectedBook, value) && value != null)
            {
                // Copy selected book to edit fields
                EditTitle = value.Title;
                EditAuthor = value.Author;
                EditISBN = value.ISBN;
                EditPublisher = value.Publisher;
                EditCategory = value.Category;
                EditPrice = value.Price;
                EditPublicationYear = value.PublicationYear;
                IsNewBook = false;
            }
        }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    // Edit form properties
    private string _editTitle = string.Empty;
    public string EditTitle
    {
        get => _editTitle;
        set => SetProperty(ref _editTitle, value);
    }

    private string _editAuthor = string.Empty;
    public string EditAuthor
    {
        get => _editAuthor;
        set => SetProperty(ref _editAuthor, value);
    }

    private string _editISBN = string.Empty;
    public string EditISBN
    {
        get => _editISBN;
        set => SetProperty(ref _editISBN, value);
    }

    private string? _editPublisher;
    public string? EditPublisher
    {
        get => _editPublisher;
        set => SetProperty(ref _editPublisher, value);
    }

    private string? _editCategory;
    public string? EditCategory
    {
        get => _editCategory;
        set => SetProperty(ref _editCategory, value);
    }

    private decimal _editPrice;
    public decimal EditPrice
    {
        get => _editPrice;
        set => SetProperty(ref _editPrice, value);
    }

    private int? _editPublicationYear;
    public int? EditPublicationYear
    {
        get => _editPublicationYear;
        set => SetProperty(ref _editPublicationYear, value);
    }

    private bool _isNewBook;
    public bool IsNewBook
    {
        get => _isNewBook;
        set => SetProperty(ref _isNewBook, value);
    }

    #endregion

    #region Commands

    public ICommand LoadDataCommand { get; }
    public ICommand AddBookCommand { get; }
    public ICommand UpdateBookCommand { get; }
    public ICommand DeleteBookCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand NewBookCommand { get; }

    #endregion

    #region Methods

    public async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;
            var books = await _libraryService.GetAllBooksAsync();
            
            Books.Clear();
            foreach (var book in books)
            {
                Books.Add(book);
            }

            SetStatus($"S-au încărcat {books.Count} cărți");
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la încărcarea cărților: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void PrepareNewBook()
    {
        SelectedBook = null;
        EditTitle = string.Empty;
        EditAuthor = string.Empty;
        EditISBN = string.Empty;
        EditPublisher = string.Empty;
        EditCategory = string.Empty;
        EditPrice = 0;
        EditPublicationYear = DateTime.Now.Year;
        IsNewBook = true;
        SetStatus("Pregătit pentru adăugarea unei cărți noi");
    }

    private async Task AddBookAsync()
    {
        if (string.IsNullOrWhiteSpace(EditTitle) || string.IsNullOrWhiteSpace(EditAuthor) || string.IsNullOrWhiteSpace(EditISBN))
        {
            SetStatus("Titlul, autorul și ISBN-ul sunt obligatorii", true);
            return;
        }

        try
        {
            IsBusy = true;

            var book = new Book
            {
                Title = EditTitle,
                Author = EditAuthor,
                ISBN = EditISBN,
                Publisher = EditPublisher,
                Category = EditCategory,
                Price = EditPrice,
                PublicationYear = EditPublicationYear
            };

            await _libraryService.AddBookAsync(book);
            await LoadDataAsync();
            
            PrepareNewBook();
            SetStatus($"Cartea '{book.Title}' a fost adăugată cu succes cu codul {book.BookCode}");
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la adăugarea cărții: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateBookAsync()
    {
        if (SelectedBook == null) return;

        if (string.IsNullOrWhiteSpace(EditTitle) || string.IsNullOrWhiteSpace(EditAuthor))
        {
            SetStatus("Titlul și autorul sunt obligatorii", true);
            return;
        }

        try
        {
            IsBusy = true;

            SelectedBook.Title = EditTitle;
            SelectedBook.Author = EditAuthor;
            SelectedBook.Publisher = EditPublisher;
            SelectedBook.Category = EditCategory;
            SelectedBook.Price = EditPrice;
            SelectedBook.PublicationYear = EditPublicationYear;

            await _libraryService.UpdateBookAsync(SelectedBook);
            await LoadDataAsync();

            SetStatus($"Cartea '{EditTitle}' a fost actualizată cu succes");
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la actualizarea cărții: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteBookAsync()
    {
        if (SelectedBook == null) return;

        try
        {
            IsBusy = true;
            var title = SelectedBook.Title;
            
            await _libraryService.DeleteBookAsync(SelectedBook.BookId);
            await LoadDataAsync();

            PrepareNewBook();
            SetStatus($"Cartea '{title}' a fost ștearsă cu succes");
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la ștergerea cărții: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SearchBooksAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadDataAsync();
            return;
        }

        try
        {
            IsBusy = true;
            var books = await _libraryService.SearchBooksAsync(SearchText);
            
            Books.Clear();
            foreach (var book in books)
            {
                Books.Add(book);
            }

            SetStatus($"S-au găsit {books.Count} cărți care corespund cu '{SearchText}'");
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la căutarea cărților: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
        _ = LoadDataAsync();
    }

    #endregion
}
