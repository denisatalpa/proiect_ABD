using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models;

/// <summary>
/// Tipul de utilizator în sistem
/// </summary>
public enum UserRole
{
    User,   // Utilizator normal - poate împrumuta/returna cărți, vizualiza
    Admin   // Administrator - poate adăuga/șterge cărți, gestiona membri, etc.
}

/// <summary>
/// Tipul de membru (student sau profesor)
/// </summary>
public enum MemberType
{
    Student,
    Professor
}

/// <summary>
/// Modelul pentru utilizatorii sistemului (autentificare)
/// </summary>
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

    /// <summary>
    /// Hash-ul parolei (stocare securizată)
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Rolul utilizatorului (User sau Admin)
    /// </summary>
    [Required]
    public UserRole Role { get; set; } = UserRole.User;

    /// <summary>
    /// Tipul de membru (Student sau Profesor) - pentru utilizatori non-admin
    /// </summary>
    public MemberType? UserMemberType { get; set; }

    /// <summary>
    /// ID-ul membrului asociat (Student sau Faculty)
    /// </summary>
    public int? MemberId { get; set; }

    /// <summary>
    /// Membrul asociat acestui utilizator
    /// </summary>
    [ForeignKey("MemberId")]
    public virtual Member? Member { get; set; }

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Numele complet al utilizatorului
    /// </summary>
    public string FullName => string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName)
        ? Username
        : $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Verifică dacă utilizatorul este administrator
    /// </summary>
    public bool IsAdmin => Role == UserRole.Admin;

    /// <summary>
    /// Numărul maxim de cărți pe care le poate împrumuta
    /// </summary>
    [NotMapped]
    public int MaxBooksAllowed => Member?.MaxBooksAllowed ?? (UserMemberType == MemberType.Professor ? 10 : 3);

    /// <summary>
    /// Numărul maxim de zile pentru împrumut
    /// </summary>
    [NotMapped]
    public int MaxIssueDays => Member?.MaxIssueDays ?? (UserMemberType == MemberType.Professor ? 30 : 14);
}
