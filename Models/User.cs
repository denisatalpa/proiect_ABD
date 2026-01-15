using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models;


/// Tipul de utilizator în sistem

public enum UserRole
{
    User,   // Utilizator normal - poate împrumuta/returna cărți, vizualiza
    Admin   // Administrator - poate adăuga/șterge cărți, gestiona membri, etc.
}


/// Tipul de membru (student sau profesor)

public enum MemberType
{
    Student,
    Professor
}


/// Modelul pentru utilizatorii sistemului (autentificare)

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    
    /// Hash-ul parolei (stocare securizată)
    
    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    
    /// Rolul utilizatorului (User sau Admin)
    
    [Required]
    public UserRole Role { get; set; } = UserRole.User;

    
    /// Tipul de membru (Student sau Profesor) - pentru utilizatori non-admin
    
    public MemberType? UserMemberType { get; set; }

    
    /// ID-ul membrului asociat (Student sau Faculty)
    
    public int? MemberId { get; set; }

    
    /// Membrul asociat acestui utilizator
    
    [ForeignKey("MemberId")]
    public virtual Member? Member { get; set; }

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;

    
    /// Numele complet al utilizatorului
    
    public string FullName => string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName)
        ? Username
        : $"{FirstName} {LastName}".Trim();

    
    /// Verifică dacă utilizatorul este administrator
    
    public bool IsAdmin => Role == UserRole.Admin;

    
    /// Numărul maxim de cărți pe care le poate împrumuta
    
    [NotMapped]
    public int MaxBooksAllowed => Member?.MaxBooksAllowed ?? (UserMemberType == MemberType.Professor ? 10 : 3);

    
    /// Numărul maxim de zile pentru împrumut
    
    [NotMapped]
    public int MaxIssueDays => Member?.MaxIssueDays ?? (UserMemberType == MemberType.Professor ? 30 : 14);
}
