using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services;

/// <summary>
/// Serviciu pentru autentificarea utilizatorilor
/// </summary>
public class AuthenticationService
{
    private readonly LibraryDbContext _context;
    private static User? _currentUser;
    private static int? _currentUserMemberId;

    public AuthenticationService(LibraryDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Utilizatorul curent autentificat
    /// </summary>
    public static User? CurrentUser
    {
        get => _currentUser;
        private set => _currentUser = value;
    }

    /// <summary>
    /// ID-ul membrului utilizatorului curent (pentru împrumuturi)
    /// </summary>
    public static int? CurrentUserMemberId
    {
        get => _currentUserMemberId;
        private set => _currentUserMemberId = value;
    }

    /// <summary>
    /// Verifică dacă există un utilizator autentificat
    /// </summary>
    public static bool IsAuthenticated => CurrentUser != null;

    /// <summary>
    /// Verifică dacă utilizatorul curent este administrator
    /// </summary>
    public static bool IsAdmin => CurrentUser?.IsAdmin ?? false;

    /// <summary>
    /// Asigură că există un cont de admin implicit
    /// </summary>
    public async Task EnsureAdminExistsAsync()
    {
        var adminExists = await _context.Users.AnyAsync(u => u.Role == UserRole.Admin);
        if (!adminExists)
        {
            var admin = new User
            {
                Username = "admin",
                Email = "admin@biblioteca.ro",
                PasswordHash = HashPassword("admin123"),
                Role = UserRole.Admin,
                FirstName = "Administrator",
                LastName = "Sistem",
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            _context.Users.Add(admin);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Autentifică un utilizator
    /// </summary>
    public async Task<(bool Success, string Message, User? User)> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Numele de utilizator și parola sunt obligatorii.", null);
        }

        var passwordHash = HashPassword(password);
        var user = await _context.Users
            .Include(u => u.Member)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.PasswordHash == passwordHash);

        if (user == null)
        {
            return (false, "Nume de utilizator sau parolă incorectă.", null);
        }

        if (!user.IsActive)
        {
            return (false, "Contul este dezactivat. Contactați administratorul.", null);
        }

        // Actualizare data ultimei autentificări
        user.LastLoginAt = DateTime.Now;
        await _context.SaveChangesAsync();

        CurrentUser = user;
        CurrentUserMemberId = user.MemberId;
        return (true, $"Bine ați venit, {user.FullName}!", user);
    }

    /// <summary>
    /// Înregistrează un utilizator nou
    /// </summary>
    public async Task<(bool Success, string Message, User? User)> RegisterAsync(
        string username,
        string email,
        string password,
        string confirmPassword,
        MemberType memberType,
        string? firstName = null,
        string? lastName = null,
        string? department = null)
    {
        // Validări
        if (string.IsNullOrWhiteSpace(username))
        {
            return (false, "Numele de utilizator este obligatoriu.", null);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return (false, "Adresa de email este obligatorie.", null);
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return (false, "Parola este obligatorie.", null);
        }

        if (password != confirmPassword)
        {
            return (false, "Parolele nu coincid.", null);
        }

        if (password.Length < 6)
        {
            return (false, "Parola trebuie să aibă cel puțin 6 caractere.", null);
        }

        // Verificare dacă username-ul există deja
        var existingUsername = await _context.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower());
        if (existingUsername)
        {
            return (false, "Numele de utilizator este deja folosit.", null);
        }

        // Verificare dacă email-ul există deja în Users
        var existingEmail = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        if (existingEmail)
        {
            return (false, "Adresa de email este deja folosită.", null);
        }

        // Verificare dacă email-ul există deja în Members
        var existingMemberEmail = await _context.Members
            .AnyAsync(m => m.Email.ToLower() == email.ToLower());
        if (existingMemberEmail)
        {
            return (false, "Adresa de email este deja asociată unui membru existent.", null);
        }

        // Folosim o tranzacție pentru a asigura consistența datelor
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Creare membru în funcție de tipul selectat
            Member member;
            if (memberType == MemberType.Student)
            {
                var studentId = $"S{DateTime.Now:yyyyMMddHHmmss}";
                var membershipId = $"STU-{DateTime.Now.Year}-{await GetNextMemberNumberAsync("STU"):D3}";
                member = new Student
                {
                    MembershipId = membershipId,
                    FirstName = firstName ?? username,
                    LastName = lastName ?? "",
                    Email = email,
                    StudentId = studentId,
                    Department = department ?? "Nedefinit",
                    YearOfStudy = 1,
                    MemberType = "Student",
                    IsActive = true
                };
                _context.Students.Add((Student)member);
            }
            else
            {
                var facultyId = $"F{DateTime.Now:yyyyMMddHHmmss}";
                var membershipId = $"PROF-{DateTime.Now.Year}-{await GetNextMemberNumberAsync("PROF"):D3}";
                member = new Faculty
                {
                    MembershipId = membershipId,
                    FirstName = firstName ?? username,
                    LastName = lastName ?? "",
                    Email = email,
                    FacultyId = facultyId,
                    Department = department ?? "Nedefinit",
                    Designation = "Profesor",
                    MemberType = "Faculty",
                    IsActive = true
                };
                _context.Faculty.Add((Faculty)member);
            }

            await _context.SaveChangesAsync();

            // Creare utilizator nou
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                Role = UserRole.User,
                UserMemberType = memberType,
                MemberId = member.MemberId,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return (true, "Cont creat cu succes! Vă puteți autentifica acum.", user);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            // Obținem mesajul din inner exception dacă există
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return (false, $"Eroare la crearea contului: {innerMessage}", null);
        }
    }

    private async Task<int> GetNextMemberNumberAsync(string prefix)
    {
        var year = DateTime.Now.Year;
        var count = await _context.Members
            .Where(m => m.MembershipId.StartsWith($"{prefix}-{year}"))
            .CountAsync();
        return count + 1;
    }

    /// <summary>
    /// Deconectează utilizatorul curent
    /// </summary>
    public void Logout()
    {
        CurrentUser = null;
    }

    /// <summary>
    /// Obține toți utilizatorii (doar pentru admin)
    /// </summary>
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    /// <summary>
    /// Actualizează rolul unui utilizator (doar pentru admin)
    /// </summary>
    public async Task<(bool Success, string Message)> UpdateUserRoleAsync(int userId, UserRole newRole)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return (false, "Utilizatorul nu a fost găsit.");
        }

        user.Role = newRole;
        await _context.SaveChangesAsync();
        return (true, $"Rolul utilizatorului {user.Username} a fost actualizat la {newRole}.");
    }

    /// <summary>
    /// Hash pentru parolă folosind SHA256
    /// </summary>
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}
