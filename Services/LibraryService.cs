using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services;

/// <summary>
/// Service class handling all library business logic operations
/// </summary>
public class LibraryService
{
    private readonly LibraryDbContext _context;

    // Fine rate per day for late returns
    public const decimal FINE_PER_DAY = 1.00m;

    public LibraryService(LibraryDbContext context)
    {
        _context = context;
    }

    #region Book Operations

    public async Task<List<Book>> GetAllBooksAsync()
    {
        return await _context.Books
            .Where(b => b.IsActive)
            .OrderBy(b => b.Title)
            .ThenBy(b => b.CopyNumber)
            .ToListAsync();
    }

    public async Task<List<Book>> GetAvailableBooksAsync()
    {
        return await _context.Books
            .Where(b => b.IsActive && b.IsAvailable)
            .OrderBy(b => b.Title)
            .ToListAsync();
    }

    public async Task<Book?> GetBookByIdAsync(int bookId)
    {
        return await _context.Books.FindAsync(bookId);
    }

    public async Task<Book?> GetBookByCodeAsync(string bookCode)
    {
        return await _context.Books
            .FirstOrDefaultAsync(b => b.BookCode == bookCode);
    }

    public async Task<List<Book>> SearchBooksAsync(string searchTerm)
    {
        return await _context.Books
            .Where(b => b.IsActive &&
                (b.Title.Contains(searchTerm) ||
                 b.Author.Contains(searchTerm) ||
                 b.ISBN.Contains(searchTerm) ||
                 b.BookCode.Contains(searchTerm)))
            .OrderBy(b => b.Title)
            .ToListAsync();
    }

    public async Task<Book> AddBookAsync(Book book)
    {
        // Generate unique book code
        var existingCopies = await _context.Books
            .Where(b => b.ISBN == book.ISBN)
            .CountAsync();
        
        book.CopyNumber = existingCopies + 1;
        book.BookCode = GenerateBookCode(book.ISBN, book.CopyNumber);

        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task UpdateBookAsync(Book book)
    {
        _context.Books.Update(book);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteBookAsync(int bookId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book != null)
        {
            book.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    private string GenerateBookCode(string isbn, int copyNumber)
    {
        var shortIsbn = isbn.Replace("-", "").Substring(Math.Max(0, isbn.Length - 6));
        return $"BK-{shortIsbn}-{copyNumber:D3}";
    }

    #endregion

    #region Member Operations

    public async Task<List<Member>> GetAllMembersAsync()
    {
        return await _context.Members
            .Where(m => m.IsActive)
            .OrderBy(m => m.LastName)
            .ThenBy(m => m.FirstName)
            .ToListAsync();
    }

    public async Task<List<Student>> GetAllStudentsAsync()
    {
        return await _context.Students
            .Where(s => s.IsActive)
            .OrderBy(s => s.LastName)
            .ToListAsync();
    }

    public async Task<List<Faculty>> GetAllFacultyAsync()
    {
        return await _context.Faculty
            .Where(f => f.IsActive)
            .OrderBy(f => f.LastName)
            .ToListAsync();
    }

    public async Task<Member?> GetMemberByIdAsync(int memberId)
    {
        return await _context.Members.FindAsync(memberId);
    }

    public async Task<Member?> GetMemberByMembershipIdAsync(string membershipId)
    {
        return await _context.Members
            .FirstOrDefaultAsync(m => m.MembershipId == membershipId);
    }

    public async Task<Student> AddStudentAsync(Student student)
    {
        student.MembershipId = await GenerateMembershipIdAsync("STU");
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task<Faculty> AddFacultyAsync(Faculty faculty)
    {
        faculty.MembershipId = await GenerateMembershipIdAsync("FAC");
        _context.Faculty.Add(faculty);
        await _context.SaveChangesAsync();
        return faculty;
    }

    public async Task UpdateMemberAsync(Member member)
    {
        _context.Members.Update(member);
        await _context.SaveChangesAsync();
    }

    private async Task<string> GenerateMembershipIdAsync(string prefix)
    {
        var year = DateTime.Now.Year;
        var count = await _context.Members
            .Where(m => m.MembershipId.StartsWith($"{prefix}-{year}"))
            .CountAsync();
        return $"{prefix}-{year}-{(count + 1):D3}";
    }

    #endregion

    #region Book Issue Operations

    public async Task<(bool Success, string Message, BookIssue? Issue)> IssueBookAsync(int bookId, int memberId, string? issuedBy = null)
    {
        var book = await _context.Books.FindAsync(bookId);
        var member = await _context.Members.FindAsync(memberId);

        if (book == null)
            return (false, "Cartea nu a fost găsită.", null);

        if (member == null)
            return (false, "Membrul nu a fost găsit.", null);

        if (!book.IsAvailable)
            return (false, "Cartea nu este disponibilă pentru împrumut.", null);

        if (!member.IsActive)
            return (false, "Contul membrului nu este activ.", null);

        // Check member's current issued books count
        var currentIssuedCount = await _context.BookIssues
            .Where(bi => bi.MemberId == memberId && bi.ReturnDate == null)
            .CountAsync();

        if (currentIssuedCount >= member.MaxBooksAllowed)
            return (false, $"Membrul a atins limita maximă de {member.MaxBooksAllowed} cărți.", null);

        // Check if member has unpaid fines
        var unpaidFines = await _context.Fines
            .Where(f => f.BookIssue.MemberId == memberId && f.Status == "Pending")
            .SumAsync(f => (decimal?)(f.TotalAmount - f.AmountPaid)) ?? 0;

        if (unpaidFines > 10) // Allow up to 10 units of unpaid fine
            return (false, $"Membrul are amenzi neachitate de {unpaidFines:C}. Vă rugăm să achitați amenzile mai întâi.", null);

        // Create book issue
        var issue = new BookIssue
        {
            BookId = bookId,
            MemberId = memberId,
            IssueDate = DateTime.Now,
            DueDate = DateTime.Now.AddDays(member.MaxIssueDays),
            IssueDuration = member.MaxIssueDays,
            Status = "Issued",
            IssuedBy = issuedBy
        };

        // Mark book as not available
        book.IsAvailable = false;

        _context.BookIssues.Add(issue);
        await _context.SaveChangesAsync();

        return (true, $"Cartea a fost împrumutată cu succes. Data scadență: {issue.DueDate:dd/MM/yyyy}", issue);
    }

    public async Task<(bool Success, string Message, Fine? Fine)> ReturnBookAsync(int issueId, string? returnedTo = null)
    {
        var issue = await _context.BookIssues
            .Include(bi => bi.Book)
            .Include(bi => bi.Member)
            .FirstOrDefaultAsync(bi => bi.IssueId == issueId);

        if (issue == null)
            return (false, "Înregistrarea împrumutului nu a fost găsită.", null);

        if (issue.ReturnDate != null)
            return (false, "Cartea a fost deja returnată.", null);

        issue.ReturnDate = DateTime.Now;
        issue.Status = "Returned";
        issue.ReturnedTo = returnedTo;

        // Mark book as available
        issue.Book.IsAvailable = true;

        Fine? fine = null;

        // Calculate fine if overdue
        if (issue.ReturnDate > issue.DueDate)
        {
            var daysOverdue = (int)(issue.ReturnDate.Value - issue.DueDate).TotalDays;
            fine = new Fine
            {
                IssueId = issueId,
                DaysOverdue = daysOverdue,
                FinePerDay = FINE_PER_DAY,
                TotalAmount = daysOverdue * FINE_PER_DAY,
                FineDate = DateTime.Now,
                Status = "Pending"
            };
            _context.Fines.Add(fine);
        }

        await _context.SaveChangesAsync();

        if (fine != null)
            return (true, $"Carte returnată. Amendă de {fine.TotalAmount:C} aplicată pentru {fine.DaysOverdue} zile întârziere.", fine);

        return (true, "Carte returnată cu succes. Nicio amendă aplicată.", null);
    }

    public async Task<List<BookIssue>> GetAllIssuesAsync()
    {
        return await _context.BookIssues
            .Include(bi => bi.Book)
            .Include(bi => bi.Member)
            .OrderByDescending(bi => bi.IssueDate)
            .ToListAsync();
    }

    public async Task<List<BookIssue>> GetActiveIssuesAsync()
    {
        return await _context.BookIssues
            .Include(bi => bi.Book)
            .Include(bi => bi.Member)
            .Where(bi => bi.ReturnDate == null)
            .OrderByDescending(bi => bi.IssueDate)
            .ToListAsync();
    }

    public async Task<List<BookIssue>> GetMemberIssuesAsync(int memberId)
    {
        return await _context.BookIssues
            .Include(bi => bi.Book)
            .Where(bi => bi.MemberId == memberId)
            .OrderByDescending(bi => bi.IssueDate)
            .ToListAsync();
    }

    public async Task<List<BookIssue>> GetOverdueIssuesAsync()
    {
        return await _context.BookIssues
            .Include(bi => bi.Book)
            .Include(bi => bi.Member)
            .Where(bi => bi.ReturnDate == null && bi.DueDate < DateTime.Now)
            .OrderBy(bi => bi.DueDate)
            .ToListAsync();
    }

    #endregion

    #region Fine Operations

    public async Task<List<Fine>> GetAllFinesAsync()
    {
        return await _context.Fines
            .Include(f => f.BookIssue)
                .ThenInclude(bi => bi.Book)
            .Include(f => f.BookIssue)
                .ThenInclude(bi => bi.Member)
            .OrderByDescending(f => f.FineDate)
            .ToListAsync();
    }

    public async Task<List<Fine>> GetPendingFinesAsync()
    {
        return await _context.Fines
            .Include(f => f.BookIssue)
                .ThenInclude(bi => bi.Book)
            .Include(f => f.BookIssue)
                .ThenInclude(bi => bi.Member)
            .Where(f => f.Status == "Pending" || f.Status == "Partial")
            .OrderByDescending(f => f.FineDate)
            .ToListAsync();
    }

    public async Task<List<Fine>> GetMemberFinesAsync(int memberId)
    {
        return await _context.Fines
            .Include(f => f.BookIssue)
                .ThenInclude(bi => bi.Book)
            .Where(f => f.BookIssue.MemberId == memberId)
            .OrderByDescending(f => f.FineDate)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> PayFineAsync(int fineId, decimal amount)
    {
        var fine = await _context.Fines.FindAsync(fineId);

        if (fine == null)
            return (false, "Înregistrarea amenzii nu a fost găsită.");

        if (fine.IsPaid)
            return (false, "Amenda a fost deja achitată.");

        if (amount <= 0)
            return (false, "Suma plății trebuie să fie pozitivă.");

        fine.AmountPaid += amount;

        if (fine.AmountPaid >= fine.TotalAmount)
        {
            fine.Status = "Paid";
            fine.PaymentDate = DateTime.Now;
        }
        else
        {
            fine.Status = "Partial";
        }

        await _context.SaveChangesAsync();
        return (true, $"Plată de {amount:C} înregistrată. Rămas: {fine.RemainingAmount:C}");
    }

    public async Task<(bool Success, string Message)> WaiveFineAsync(int fineId, string reason)
    {
        var fine = await _context.Fines.FindAsync(fineId);

        if (fine == null)
            return (false, "Înregistrarea amenzii nu a fost găsită.");

        fine.Status = "Waived";
        fine.PaymentDate = DateTime.Now;
        fine.Remarks = $"Anulată: {reason}";

        await _context.SaveChangesAsync();
        return (true, "Amenda a fost anulată.");
    }

    #endregion

    #region Statistics

    public async Task<LibraryStatistics> GetStatisticsAsync()
    {
        return new LibraryStatistics
        {
            TotalBooks = await _context.Books.Where(b => b.IsActive).CountAsync(),
            AvailableBooks = await _context.Books.Where(b => b.IsActive && b.IsAvailable).CountAsync(),
            TotalMembers = await _context.Members.Where(m => m.IsActive).CountAsync(),
            TotalStudents = await _context.Students.Where(s => s.IsActive).CountAsync(),
            TotalFaculty = await _context.Faculty.Where(f => f.IsActive).CountAsync(),
            ActiveIssues = await _context.BookIssues.Where(bi => bi.ReturnDate == null).CountAsync(),
            OverdueBooks = await _context.BookIssues.Where(bi => bi.ReturnDate == null && bi.DueDate < DateTime.Now).CountAsync(),
            PendingFines = await _context.Fines.Where(f => f.Status == "Pending" || f.Status == "Partial").SumAsync(f => f.TotalAmount - f.AmountPaid),
            TotalFinesCollected = await _context.Fines.Where(f => f.Status == "Paid").SumAsync(f => f.TotalAmount)
        };
    }

    #endregion
}

/// <summary>
/// Statistics data transfer object
/// </summary>
public class LibraryStatistics
{
    public int TotalBooks { get; set; }
    public int AvailableBooks { get; set; }
    public int TotalMembers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalFaculty { get; set; }
    public int ActiveIssues { get; set; }
    public int OverdueBooks { get; set; }
    public decimal PendingFines { get; set; }
    public decimal TotalFinesCollected { get; set; }
}
