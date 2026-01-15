using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models;

/// <summary>
/// Student member - can issue up to 3 books for 14 days
/// </summary>
public class Student : Member
{
    [Required]
    [MaxLength(50)]
    public string StudentId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required]
    public int YearOfStudy { get; set; }

    [MaxLength(100)]
    public string? Program { get; set; }

    public DateTime? ExpectedGraduationDate { get; set; }

    // Students can issue maximum 3 books
    [NotMapped]
    public override int MaxBooksAllowed => 3;

    // Students can keep books for 14 days
    [NotMapped]
    public override int MaxIssueDays => 14;

    public Student()
    {
        MemberType = "Student";
    }
}
