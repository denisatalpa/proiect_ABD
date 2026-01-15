using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models;

/// <summary>
/// Base class for library members (Students and Faculty)
/// </summary>
public abstract class Member
{
    [Key]
    public int MemberId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    [MaxLength(50)]
    public string MembershipId { get; set; } = string.Empty;

    public DateTime RegistrationDate { get; set; } = DateTime.Now;

    public bool IsActive { get; set; } = true;

    [Required]
    public string MemberType { get; set; } = string.Empty;

    // Navigation property
    public virtual ICollection<BookIssue> BookIssues { get; set; } = new List<BookIssue>();

    // Abstract properties for different limits
    [NotMapped]
    public abstract int MaxBooksAllowed { get; }

    [NotMapped]
    public abstract int MaxIssueDays { get; }

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}
