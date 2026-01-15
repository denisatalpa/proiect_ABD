using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Data;

/// <summary>
/// Entity Framework DbContext for the Library Management System
/// </summary>
public class LibraryDbContext : DbContext
{
    public DbSet<Member> Members { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Faculty> Faculty { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<BookIssue> BookIssues { get; set; }
    public DbSet<Fine> Fines { get; set; }

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

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Date inițiale - cărți (prețuri în Lei RON)
        modelBuilder.Entity<Book>().HasData(
            new Book { BookId = 1, BookCode = "CS101-001", ISBN = "978-0-13-468599-1", Title = "Clean Code", Author = "Robert C. Martin", Publisher = "Pearson", PublicationYear = 2008, Category = "Programare", CopyNumber = 1, Price = 180.00m },
            new Book { BookId = 2, BookCode = "CS101-002", ISBN = "978-0-13-468599-1", Title = "Clean Code", Author = "Robert C. Martin", Publisher = "Pearson", PublicationYear = 2008, Category = "Programare", CopyNumber = 2, Price = 180.00m },
            new Book { BookId = 3, BookCode = "CS102-001", ISBN = "978-0-596-51774-8", Title = "JavaScript: The Good Parts", Author = "Douglas Crockford", Publisher = "O'Reilly", PublicationYear = 2008, Category = "Programare", CopyNumber = 1, Price = 120.00m },
            new Book { BookId = 4, BookCode = "CS103-001", ISBN = "978-0-13-235088-4", Title = "The Pragmatic Programmer", Author = "David Thomas, Andrew Hunt", Publisher = "Addison-Wesley", PublicationYear = 2019, Category = "Programare", CopyNumber = 1, Price = 200.00m },
            new Book { BookId = 5, BookCode = "DB101-001", ISBN = "978-0-13-289632-1", Title = "Database System Concepts", Author = "Abraham Silberschatz", Publisher = "McGraw-Hill", PublicationYear = 2020, Category = "Baze de Date", CopyNumber = 1, Price = 320.00m },
            new Book { BookId = 6, BookCode = "DB101-002", ISBN = "978-0-13-289632-1", Title = "Database System Concepts", Author = "Abraham Silberschatz", Publisher = "McGraw-Hill", PublicationYear = 2020, Category = "Baze de Date", CopyNumber = 2, Price = 320.00m },
            new Book { BookId = 7, BookCode = "NET101-001", ISBN = "978-1-4842-8873-4", Title = "Pro C# 10 with .NET 6", Author = "Andrew Troelsen", Publisher = "Apress", PublicationYear = 2022, Category = "Programare", CopyNumber = 1, Price = 260.00m }
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
    }
}
