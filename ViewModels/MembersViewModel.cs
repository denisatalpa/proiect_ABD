using System.Collections.ObjectModel;
using System.Windows.Input;
using LibraryManagementSystem.Commands;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.ViewModels;


/// ViewModel for managing library members (Students and Faculty)

public class MembersViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;

    public MembersViewModel(LibraryService libraryService)
    {
        _libraryService = libraryService;
        Members = new ObservableCollection<Member>();

        // Initialize commands
        LoadDataCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
        AddStudentCommand = new AsyncRelayCommand(async _ => await AddStudentAsync());
        AddFacultyCommand = new AsyncRelayCommand(async _ => await AddFacultyAsync());
        UpdateMemberCommand = new AsyncRelayCommand(async _ => await UpdateMemberAsync(), _ => SelectedMember != null);
        NewStudentCommand = new RelayCommand(_ => PrepareNewStudent());
        NewFacultyCommand = new RelayCommand(_ => PrepareNewFaculty());
        FilterStudentsCommand = new AsyncRelayCommand(async _ => await FilterStudentsAsync());
        FilterFacultyCommand = new AsyncRelayCommand(async _ => await FilterFacultyAsync());
        ShowAllCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
    }

    #region Properties

    public ObservableCollection<Member> Members { get; }

    private Member? _selectedMember;
    public Member? SelectedMember
    {
        get => _selectedMember;
        set
        {
            if (SetProperty(ref _selectedMember, value) && value != null)
            {
                // Populate edit fields
                EditFirstName = value.FirstName;
                EditLastName = value.LastName;
                EditEmail = value.Email;
                EditPhone = value.PhoneNumber;
                IsStudent = value is Student;
                IsFaculty = value is Faculty;
                IsNewMember = false;

                if (value is Student student)
                {
                    EditStudentId = student.StudentId;
                    EditDepartment = student.Department;
                    EditYearOfStudy = student.YearOfStudy;
                }
                else if (value is Faculty faculty)
                {
                    EditFacultyId = faculty.FacultyId;
                    EditDepartment = faculty.Department;
                    EditDesignation = faculty.Designation;
                }
            }
        }
    }

    // Common edit fields
    private string _editFirstName = string.Empty;
    public string EditFirstName
    {
        get => _editFirstName;
        set => SetProperty(ref _editFirstName, value);
    }

    private string _editLastName = string.Empty;
    public string EditLastName
    {
        get => _editLastName;
        set => SetProperty(ref _editLastName, value);
    }

    private string _editEmail = string.Empty;
    public string EditEmail
    {
        get => _editEmail;
        set => SetProperty(ref _editEmail, value);
    }

    private string? _editPhone;
    public string? EditPhone
    {
        get => _editPhone;
        set => SetProperty(ref _editPhone, value);
    }

    private string _editDepartment = string.Empty;
    public string EditDepartment
    {
        get => _editDepartment;
        set => SetProperty(ref _editDepartment, value);
    }

    // Student-specific fields
    private string _editStudentId = string.Empty;
    public string EditStudentId
    {
        get => _editStudentId;
        set => SetProperty(ref _editStudentId, value);
    }

    private int _editYearOfStudy = 1;
    public int EditYearOfStudy
    {
        get => _editYearOfStudy;
        set => SetProperty(ref _editYearOfStudy, value);
    }

    // Faculty-specific fields
    private string _editFacultyId = string.Empty;
    public string EditFacultyId
    {
        get => _editFacultyId;
        set => SetProperty(ref _editFacultyId, value);
    }

    private string? _editDesignation;
    public string? EditDesignation
    {
        get => _editDesignation;
        set => SetProperty(ref _editDesignation, value);
    }

    // State flags
    private bool _isStudent;
    public bool IsStudent
    {
        get => _isStudent;
        set => SetProperty(ref _isStudent, value);
    }

    private bool _isFaculty;
    public bool IsFaculty
    {
        get => _isFaculty;
        set => SetProperty(ref _isFaculty, value);
    }

    private bool _isNewMember;
    public bool IsNewMember
    {
        get => _isNewMember;
        set => SetProperty(ref _isNewMember, value);
    }

    #endregion

    #region Commands

    public ICommand LoadDataCommand { get; }
    public ICommand AddStudentCommand { get; }
    public ICommand AddFacultyCommand { get; }
    public ICommand UpdateMemberCommand { get; }
    public ICommand NewStudentCommand { get; }
    public ICommand NewFacultyCommand { get; }
    public ICommand FilterStudentsCommand { get; }
    public ICommand FilterFacultyCommand { get; }
    public ICommand ShowAllCommand { get; }

    #endregion

    #region Methods

    public async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;
            var members = await _libraryService.GetAllMembersAsync();
            
            Members.Clear();
            foreach (var member in members)
            {
                Members.Add(member);
            }

            SetStatus($"S-au încărcat {members.Count} membri");
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la încărcarea membrilor: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task FilterStudentsAsync()
    {
        try
        {
            IsBusy = true;
            var students = await _libraryService.GetAllStudentsAsync();
            
            Members.Clear();
            foreach (var student in students)
            {
                Members.Add(student);
            }

            SetStatus($"Se afișează {students.Count} studenți");
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la filtrarea studenților: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task FilterFacultyAsync()
    {
        try
        {
            IsBusy = true;
            var faculty = await _libraryService.GetAllFacultyAsync();
            
            Members.Clear();
            foreach (var f in faculty)
            {
                Members.Add(f);
            }

            SetStatus($"Se afișează {faculty.Count} profesori");
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la filtrarea profesorilor: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearEditFields()
    {
        EditFirstName = string.Empty;
        EditLastName = string.Empty;
        EditEmail = string.Empty;
        EditPhone = string.Empty;
        EditDepartment = string.Empty;
        EditStudentId = string.Empty;
        EditYearOfStudy = 1;
        EditFacultyId = string.Empty;
        EditDesignation = string.Empty;
        SelectedMember = null;
    }

    private void PrepareNewStudent()
    {
        ClearEditFields();
        IsStudent = true;
        IsFaculty = false;
        IsNewMember = true;
        SetStatus("Pregătit pentru adăugarea unui student nou");
    }

    private void PrepareNewFaculty()
    {
        ClearEditFields();
        IsStudent = false;
        IsFaculty = true;
        IsNewMember = true;
        SetStatus("Pregătit pentru adăugarea unui profesor nou");
    }

    private async Task AddStudentAsync()
    {
        if (!ValidateCommonFields()) return;

        if (string.IsNullOrWhiteSpace(EditStudentId))
        {
            SetStatus("ID-ul studentului este obligatoriu", true);
            return;
        }

        try
        {
            IsBusy = true;

            var student = new Student
            {
                FirstName = EditFirstName,
                LastName = EditLastName,
                Email = EditEmail,
                PhoneNumber = EditPhone,
                StudentId = EditStudentId,
                Department = EditDepartment,
                YearOfStudy = EditYearOfStudy
            };

            await _libraryService.AddStudentAsync(student);
            await LoadDataAsync();
            
            ClearEditFields();
            SetStatus($"Studentul '{student.FullName}' a fost adăugat cu succes. Cărți maxime: {student.MaxBooksAllowed}, Zile maxime: {student.MaxIssueDays}");
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la adăugarea studentului: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddFacultyAsync()
    {
        if (!ValidateCommonFields()) return;

        if (string.IsNullOrWhiteSpace(EditFacultyId))
        {
            SetStatus("ID-ul profesorului este obligatoriu", true);
            return;
        }

        try
        {
            IsBusy = true;

            var faculty = new Faculty
            {
                FirstName = EditFirstName,
                LastName = EditLastName,
                Email = EditEmail,
                PhoneNumber = EditPhone,
                FacultyId = EditFacultyId,
                Department = EditDepartment,
                Designation = EditDesignation
            };

            await _libraryService.AddFacultyAsync(faculty);
            await LoadDataAsync();
            
            ClearEditFields();
            SetStatus($"Profesorul '{faculty.FullName}' a fost adăugat cu succes. Cărți maxime: {faculty.MaxBooksAllowed}, Zile maxime: {faculty.MaxIssueDays}");
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la adăugarea profesorului: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateMemberAsync()
    {
        if (SelectedMember == null) return;
        if (!ValidateCommonFields()) return;

        try
        {
            IsBusy = true;

            SelectedMember.FirstName = EditFirstName;
            SelectedMember.LastName = EditLastName;
            SelectedMember.Email = EditEmail;
            SelectedMember.PhoneNumber = EditPhone;

            if (SelectedMember is Student student)
            {
                student.Department = EditDepartment;
                student.YearOfStudy = EditYearOfStudy;
            }
            else if (SelectedMember is Faculty faculty)
            {
                faculty.Department = EditDepartment;
                faculty.Designation = EditDesignation;
            }

            await _libraryService.UpdateMemberAsync(SelectedMember);
            await LoadDataAsync();

            SetStatus($"Membrul '{EditFirstName} {EditLastName}' a fost actualizat cu succes");
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la actualizarea membrului: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidateCommonFields()
    {
        if (string.IsNullOrWhiteSpace(EditFirstName))
        {
            SetStatus("Prenumele este obligatoriu", true);
            return false;
        }
        if (string.IsNullOrWhiteSpace(EditLastName))
        {
            SetStatus("Numele este obligatoriu", true);
            return false;
        }
        if (string.IsNullOrWhiteSpace(EditEmail))
        {
            SetStatus("Emailul este obligatoriu", true);
            return false;
        }
        if (string.IsNullOrWhiteSpace(EditDepartment))
        {
            SetStatus("Departamentul este obligatoriu", true);
            return false;
        }
        return true;
    }

    #endregion
}
