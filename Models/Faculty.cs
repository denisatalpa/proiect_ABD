using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models;


/// Faculty member - can issue up to 10 books for 30 days

public class Faculty : Member
{
    [Required]
    [MaxLength(50)]
    public string FacultyId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Designation { get; set; }

    [MaxLength(100)]
    public string? Specialization { get; set; }

    public DateTime? JoiningDate { get; set; }

    // Faculty can issue maximum 10 books
    [NotMapped]
    public override int MaxBooksAllowed => 10;

    // Faculty can keep books for 30 days
    [NotMapped]
    public override int MaxIssueDays => 30;

    public Faculty()
    {
        MemberType = "Faculty";
    }
}
