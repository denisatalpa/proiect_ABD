using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models;


/// Tracks fines for late book returns

public class Fine
{
    [Key]
    public int FineId { get; set; }

    [Required]
    public int IssueId { get; set; }

    
    /// Number of days the book was overdue
    
    public int DaysOverdue { get; set; }

    
    /// Fine rate per day (in currency units)
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal FinePerDay { get; set; } = 1.00m;

    
    /// Total fine amount = DaysOverdue * FinePerDay
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    
    /// Amount paid so far
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal AmountPaid { get; set; } = 0;

    
    /// Date when fine was generated
    
    public DateTime FineDate { get; set; } = DateTime.Now;

    
    /// Date when fine was paid (null if not paid)
    
    public DateTime? PaymentDate { get; set; }

    
    /// Fine status: Pending, Partial, Paid, Waived
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    
    /// Any remarks about the fine or waiver reason
    
    [MaxLength(500)]
    public string? Remarks { get; set; }

    // Navigation property
    [ForeignKey(nameof(IssueId))]
    public virtual BookIssue BookIssue { get; set; } = null!;

    // Computed properties
    [NotMapped]
    public decimal RemainingAmount => TotalAmount - AmountPaid;

    [NotMapped]
    public bool IsPaid => AmountPaid >= TotalAmount || Status == "Paid" || Status == "Waived";

    
    /// Calculates fine based on overdue days
    
    public void CalculateFine(int daysOverdue, decimal finePerDay = 1.00m)
    {
        DaysOverdue = daysOverdue;
        FinePerDay = finePerDay;
        TotalAmount = DaysOverdue * FinePerDay;
    }
}
