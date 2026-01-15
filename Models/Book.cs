using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models;

/// <summary>
/// Represents a single book copy in the library.
/// Each physical copy has a unique BookId, even if it's the same title/author.
/// </summary>
public class Book
{
    [Key]
    public int BookId { get; set; }

    /// <summary>
    /// Unique identifier for this specific book copy (e.g., "ISBN-001-COPY-1")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string BookCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string ISBN { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Author { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Publisher { get; set; }

    public int? PublicationYear { get; set; }

    [MaxLength(50)]
    public string? Edition { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(50)]
    public string? Genre { get; set; }

    /// <summary>
    /// Copy number for books with the same ISBN
    /// </summary>
    public int CopyNumber { get; set; } = 1;

    /// <summary>
    /// Physical location in the library (e.g., "Shelf A-3")
    /// </summary>
    [MaxLength(50)]
    public string? ShelfLocation { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.Now;

    /// <summary>
    /// Indicates if the book is available for issue
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Indicates if the book is active in the system (not lost/damaged)
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation property
    public virtual ICollection<BookIssue> BookIssues { get; set; } = new List<BookIssue>();

    [NotMapped]
    public string DisplayInfo => $"{Title} - {Author} (Exemplar #{CopyNumber})";
}
