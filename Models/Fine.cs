using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models;

/// <summary>
/// Tracks fines for late book returns
/// </summary>
public class Fine
{
    [Key]
    public int FineId { get; set; }

    [Required]
    public int IssueId { get; set; }

    /// <summary>
    /// Number of days the book was overdue
    /// </summary>
    public int DaysOverdue { get; set; }

    /// <summary>
    /// Fine rate per day (in currency units)
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal FinePerDay { get; set; } = 1.00m;

    /// <summary>
    /// Total fine amount = DaysOverdue * FinePerDay
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Amount paid so far
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal AmountPaid { get; set; } = 0;

    /// <summary>
    /// Date when fine was generated
    /// </summary>
    public DateTime FineDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Date when fine was paid (null if not paid)
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Fine status: Pending, Partial, Paid, Waived
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Any remarks about the fine or waiver reason
    /// </summary>
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

    /// <summary>
    /// Calculates fine based on overdue days
    /// </summary>
    public void CalculateFine(int daysOverdue, decimal finePerDay = 1.00m)
    {
        DaysOverdue = daysOverdue;
        FinePerDay = finePerDay;
        TotalAmount = DaysOverdue * FinePerDay;
    }
}
