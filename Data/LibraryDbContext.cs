using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Data;


/// Entity Framework DbContext for the Library Management System

public class LibraryDbContext : DbContext
{
    public DbSet<Member> Members { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Faculty> Faculty { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<BookIssue> BookIssues { get; set; }
    public DbSet<Fine> Fines { get; set; }
    public DbSet<User> Users { get; set; }

    public LibraryDbContext() { }

    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Connection string - modify as needed for your SQL Server instance
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=LibraryManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true",
                b => b.MigrationsAssembly("LibraryManagementSystem"));
            
            // Enable lazy loading for navigation properties
            optionsBuilder.UseLazyLoadingProxies(false);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure TPH (Table Per Hierarchy) for Member inheritance
        modelBuilder.Entity<Member>()
            .HasDiscriminator<string>("MemberType")
            .HasValue<Student>("Student")
            .HasValue<Faculty>("Faculty");

        // Configure Member
        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.MemberId);
            entity.Property(e => e.MembershipId).IsRequired();
            entity.HasIndex(e => e.MembershipId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure Student
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasIndex(e => e.StudentId).IsUnique();
        });

        // Configure Faculty
        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.HasIndex(e => e.FacultyId).IsUnique();
        });

        // Configure Book
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId);
            entity.Property(e => e.BookCode).IsRequired();
            entity.HasIndex(e => e.BookCode).IsUnique();
            entity.HasIndex(e => new { e.ISBN, e.CopyNumber }).IsUnique();
        });

        // Configure BookIssue
        modelBuilder.Entity<BookIssue>(entity =>
        {
            entity.HasKey(e => e.IssueId);
            
            entity.HasOne(e => e.Book)
                .WithMany(b => b.BookIssues)
                .HasForeignKey(e => e.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Member)
                .WithMany(m => m.BookIssues)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.BookId, e.MemberId, e.IssueDate });
        });

        // Configure Fine
        modelBuilder.Entity<Fine>(entity =>
        {
            entity.HasKey(e => e.FineId);

            entity.HasOne(e => e.BookIssue)
                .WithOne(bi => bi.Fine)
                .HasForeignKey<Fine>(e => e.IssueId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Role).HasConversion<string>();
            entity.Property(e => e.UserMemberType).HasConversion<string>();
            entity.HasOne(e => e.Member)
                .WithMany()
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Date inițiale - cărți populare românești
        modelBuilder.Entity<Book>().HasData(
            new Book { BookId = 1, BookCode = "1", ISBN = "978-973-50-1234-1", Title = "Ion", Author = "Liviu Rebreanu", Publisher = "Editura Litera", PublicationYear = 1920, Category = "Literatură Română", CopyNumber = 1 },
            new Book { BookId = 2, BookCode = "2", ISBN = "978-973-50-1234-1", Title = "Ion", Author = "Liviu Rebreanu", Publisher = "Editura Litera", PublicationYear = 1920, Category = "Literatură Română", CopyNumber = 2 },
            new Book { BookId = 3, BookCode = "3", ISBN = "978-973-50-2345-2", Title = "Enigma Otiliei", Author = "George Călinescu", Publisher = "Editura Humanitas", PublicationYear = 1938, Category = "Literatură Română", CopyNumber = 1 },
            new Book { BookId = 4, BookCode = "4", ISBN = "978-973-50-3456-3", Title = "Moromeții", Author = "Marin Preda", Publisher = "Editura Cartea Românească", PublicationYear = 1955, Category = "Literatură Română", CopyNumber = 1 },
            new Book { BookId = 5, BookCode = "5", ISBN = "978-973-50-4567-4", Title = "Pădurea Spânzuraților", Author = "Liviu Rebreanu", Publisher = "Editura Litera", PublicationYear = 1922, Category = "Literatură Română", CopyNumber = 1 },
            new Book { BookId = 6, BookCode = "6", ISBN = "978-973-50-4567-4", Title = "Pădurea Spânzuraților", Author = "Liviu Rebreanu", Publisher = "Editura Litera", PublicationYear = 1922, Category = "Literatură Română", CopyNumber = 2 },
            new Book { BookId = 7, BookCode = "7", ISBN = "978-973-50-5678-5", Title = "Maitreyi", Author = "Mircea Eliade", Publisher = "Editura Humanitas", PublicationYear = 1933, Category = "Literatură Română", CopyNumber = 1 },
            new Book { BookId = 8, BookCode = "8", ISBN = "978-973-50-6789-6", Title = "Ultima Noapte de Dragoste, Întâia Noapte de Război", Author = "Camil Petrescu", Publisher = "Editura Polirom", PublicationYear = 1930, Category = "Literatură Română", CopyNumber = 1 },
            new Book { BookId = 9, BookCode = "9", ISBN = "978-973-50-7890-7", Title = "Baltagul", Author = "Mihail Sadoveanu", Publisher = "Editura Litera", PublicationYear = 1930, Category = "Literatură Română", CopyNumber = 1 },
            new Book { BookId = 10, BookCode = "10", ISBN = "978-973-50-8901-8", Title = "Poezii", Author = "Mihai Eminescu", Publisher = "Editura Cartea Românească", PublicationYear = 1883, Category = "Poezie", CopyNumber = 1 },
            new Book { BookId = 11, BookCode = "11", ISBN = "978-973-50-8901-8", Title = "Poezii", Author = "Mihai Eminescu", Publisher = "Editura Cartea Românească", PublicationYear = 1883, Category = "Poezie", CopyNumber = 2 },
            new Book { BookId = 12, BookCode = "12", ISBN = "978-973-50-9012-9", Title = "O Scrisoare Pierdută", Author = "Ion Luca Caragiale", Publisher = "Editura Humanitas", PublicationYear = 1884, Category = "Teatru", CopyNumber = 1 }
        );

        // Date inițiale - studenți (facultăți din București)
        modelBuilder.Entity<Student>().HasData(
            new Student { MemberId = 1, MembershipId = "STU-2024-001", FirstName = "Ana", LastName = "Popescu", Email = "ana.popescu@student.unibuc.ro", StudentId = "S001", Department = "Facultatea de Matematică și Informatică", YearOfStudy = 2, MemberType = "Student" },
            new Student { MemberId = 2, MembershipId = "STU-2024-002", FirstName = "Ștefan", LastName = "Panaite", Email = "stefan.panaite@student.unibuc.ro", StudentId = "S002", Department = "Facultatea de Automatică și Calculatoare", YearOfStudy = 3, MemberType = "Student" },
            new Student { MemberId = 3, MembershipId = "STU-2024-003", FirstName = "Ion", LastName = "Toma", Email = "ion.toma@student.unibuc.ro", StudentId = "S003", Department = "Facultatea de Matematică și Informatică", YearOfStudy = 1, MemberType = "Student" },
            new Student { MemberId = 6, MembershipId = "STU-2024-004", FirstName = "Elena", LastName = "Dumitru", Email = "elena.dumitru@student.unibuc.ro", StudentId = "S004", Department = "Facultatea de Cibernetică, Statistică și Informatică Economică", YearOfStudy = 2, MemberType = "Student" },
            new Student { MemberId = 7, MembershipId = "STU-2024-005", FirstName = "Andrei", LastName = "Munteanu", Email = "andrei.munteanu@student.unibuc.ro", StudentId = "S005", Department = "Facultatea de Automatică și Calculatoare", YearOfStudy = 4, MemberType = "Student" }
        );

        // Date inițiale - profesori (facultăți din București)
        modelBuilder.Entity<Faculty>().HasData(
            new Faculty { MemberId = 4, MembershipId = "PROF-2024-001", FirstName = "Maria", LastName = "Ionescu", Email = "maria.ionescu@unibuc.ro", FacultyId = "F001", Department = "Facultatea de Matematică și Informatică", Designation = "Profesor", MemberType = "Faculty" },
            new Faculty { MemberId = 5, MembershipId = "PROF-2024-002", FirstName = "Mihai", LastName = "Georgescu", Email = "mihai.georgescu@unibuc.ro", FacultyId = "F002", Department = "Facultatea de Automatică și Calculatoare", Designation = "Conferențiar", MemberType = "Faculty" },
            new Faculty { MemberId = 8, MembershipId = "PROF-2024-003", FirstName = "Carmen", LastName = "Radu", Email = "carmen.radu@unibuc.ro", FacultyId = "F003", Department = "Facultatea de Cibernetică, Statistică și Informatică Economică", Designation = "Lector", MemberType = "Faculty" }
        );

        // Utilizatorii sunt creați dinamic prin AuthenticationService
        // Admin-ul se creează automat la primul start prin EnsureAdminExistsAsync()
        // Utilizatorii normali se înregistrează prin interfața de login/signup
    }
}
