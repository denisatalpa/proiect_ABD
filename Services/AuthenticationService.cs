using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services;


/// Serviciu pentru autentificarea utilizatorilor

public class AuthenticationService
{
    private readonly LibraryDbContext _context;
    private static User? _currentUser;
    private static int? _currentUserMemberId;

    public AuthenticationService(LibraryDbContext context)
    {
        _context = context;
    }

    
    /// Utilizatorul curent autentificat
    
    public static User? CurrentUser
    {
        get => _currentUser;
        private set => _currentUser = value;
    }

    
    /// ID-ul membrului utilizatorului curent (pentru împrumuturi)
    
    public static int? CurrentUserMemberId
    {
        get => _currentUserMemberId;
        private set => _currentUserMemberId = value;
    }

    
    /// Verifică dacă există un utilizator autentificat
    
    public static bool IsAuthenticated => CurrentUser != null;

    
    /// Verifică dacă utilizatorul curent este administrator
    
    public static bool IsAdmin => CurrentUser?.IsAdmin ?? false;

    
    /// Asigură că există un cont de admin implicit
    
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

    
    /// Autentifică un utilizator
    
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

    
    /// Înregistrează un utilizator nou
    
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

    
    /// Deconectează utilizatorul curent
    
    public void Logout()
    {
        CurrentUser = null;
    }

    
    /// Obține toți utilizatorii (doar pentru admin)
    
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    
    /// Actualizează rolul unui utilizator (doar pentru admin)
    
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

    
    /// Hash pentru parolă folosind SHA256
    
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}
