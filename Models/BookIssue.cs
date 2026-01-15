using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models;


/// Tracks book issue transactions - who issued which book, when, and duration

public class BookIssue
{
    [Key]
    public int IssueId { get; set; }

    [Required]
    public int BookId { get; set; }

    [Required]
    public int MemberId { get; set; }

    
    /// Date when the book was issued
    
    [Required]
    public DateTime IssueDate { get; set; } = DateTime.Now;

    
    /// Expected return date based on member type limits
    
    [Required]
    public DateTime DueDate { get; set; }

    
    /// Actual return date (null if not yet returned)
    
    public DateTime? ReturnDate { get; set; }

    
    /// Number of days the book can be kept (varies by member type)
    
    public int IssueDuration { get; set; }

    
    /// Current status of the issue
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Issued";

    
    /// Any remarks or notes about this issue
    
    [MaxLength(500)]
    public string? Remarks { get; set; }

    
    /// Staff member who processed the issue
    
    [MaxLength(100)]
    public string? IssuedBy { get; set; }

    
    /// Staff member who processed the return
    
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
