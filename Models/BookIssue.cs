using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models;

/// <summary>
/// Tracks book issue transactions - who issued which book, when, and duration
/// </summary>
public class BookIssue
{
    [Key]
    public int IssueId { get; set; }

    [Required]
    public int BookId { get; set; }

    [Required]
    public int MemberId { get; set; }

    /// <summary>
    /// Date when the book was issued
    /// </summary>
    [Required]
    public DateTime IssueDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Expected return date based on member type limits
    /// </summary>
    [Required]
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Actual return date (null if not yet returned)
    /// </summary>
    public DateTime? ReturnDate { get; set; }

    /// <summary>
    /// Number of days the book can be kept (varies by member type)
    /// </summary>
    public int IssueDuration { get; set; }

    /// <summary>
    /// Current status of the issue
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Issued";

    /// <summary>
    /// Any remarks or notes about this issue
    /// </summary>
    [MaxLength(500)]
    public string? Remarks { get; set; }

    /// <summary>
    /// Staff member who processed the issue
    /// </summary>
    [MaxLength(100)]
    public string? IssuedBy { get; set; }

    /// <summary>
    /// Staff member who processed the return
    /// </summary>
    [MaxLength(100)]
    public string? ReturnedTo { get; set; }

    // Navigation properties
    [ForeignKey(nameof(BookId))]
    public virtual Book Book { get; set; } = null!;

    [ForeignKey(nameof(MemberId))]
    public virtual Member Member { get; set; } = null!;

    public virtual Fine? Fine { get; set; }

    // Computed properties
    [NotMapped]
    public bool IsOverdue => !ReturnDate.HasValue && DateTime.Now > DueDate;

    [NotMapped]
    public int DaysOverdue
    {
        get
        {
            if (!IsOverdue) return 0;
            return (int)(DateTime.Now - DueDate).TotalDays;
        }
    }

    [NotMapped]
    public bool IsReturned => ReturnDate.HasValue;

    [NotMapped]
    public string StatusDisplay
    {
        get
        {
            if (IsReturned) return "Returned";
            if (IsOverdue) return $"Overdue ({DaysOverdue} days)";
            return "Issued";
        }
    }
}
