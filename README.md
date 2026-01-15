# Library Management System

## Proiect pentru materia "Administrarea și Dezvoltarea Aplicațiilor cu Baze de Date"

### Tehnologii utilizate:
- **C# / .NET 8.0**
- **Entity Framework Core 8.0** (Code-First cu SQL Server LocalDB)
- **WPF (Windows Presentation Foundation)**
- **Arhitectură MVVM (Model-View-ViewModel)**

---

## Cerințele implementate:

1. ✅ **Studenții și profesorii pot împrumuta cărți**
2. ✅ **Limite diferite pentru studenți și profesori:**
   - Studenți: maximum **3 cărți** pentru **14 zile**
   - Profesori: maximum **10 cărți** pentru **30 zile**
3. ✅ **Fiecare carte are un ID unic** (chiar și copii ale aceleiași cărți)
4. ✅ **Evidența completă a împrumuturilor:** cine, când, durată
5. ✅ **Sistem de amenzi** pentru întârzieri ($1.00/zi)

---

## Structura proiectului:

```
LibraryManagementSystem/
├── Models/                 # Entitățile bazei de date
│   ├── Member.cs          # Clasa abstractă de bază
│   ├── Student.cs         # Moștenește Member (3 cărți, 14 zile)
│   ├── Faculty.cs         # Moștenește Member (10 cărți, 30 zile)
│   ├── Book.cs            # Cartea cu ID unic per copie
│   ├── BookIssue.cs       # Tranzacția de împrumut
│   └── Fine.cs            # Amenda pentru întârziere
│
├── Data/
│   └── LibraryDbContext.cs # Context EF Core cu seed data
│
├── Services/
│   └── LibraryService.cs   # Logica de business
│
├── ViewModels/             # MVVM ViewModels
│   ├── ViewModelBase.cs
│   ├── MainViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── BooksViewModel.cs
│   ├── MembersViewModel.cs
│   ├── IssuesViewModel.cs
│   └── FinesViewModel.cs
│
├── Views/                  # Interfețele XAML
│   ├── DashboardView.xaml
│   ├── BooksView.xaml
│   ├── MembersView.xaml
│   ├── IssuesView.xaml
│   └── FinesView.xaml
│
├── Commands/
│   └── RelayCommand.cs     # ICommand implementations
│
├── Converters/
│   └── Converters.cs       # Value converters pentru binding
│
├── App.xaml                # Aplicația WPF
├── MainWindow.xaml         # Fereastra principală cu navigare
└── LibraryManagementSystem.csproj
```

---

## Schema bazei de date:

### Tabela Members (TPH - Table Per Hierarchy)
| Coloană | Tip | Descriere |
|---------|-----|-----------|
| MemberId | int | PK, auto-increment |
| MemberCode | string | Cod unic (STU-2024-001 / FAC-2024-001) |
| FirstName | string | Prenume |
| LastName | string | Nume |
| Email | string | Email |
| Phone | string | Telefon |
| MemberType | string | Discriminator (Student/Faculty) |
| EnrollmentDate | DateTime | Data înrolării (Student) |
| Department | string | Departament (Faculty) |

### Tabela Books
| Coloană | Tip | Descriere |
|---------|-----|-----------|
| BookId | int | PK, auto-increment |
| BookCode | string | Cod unic per copie (BK-123456-001) |
| ISBN | string | ISBN-ul cărții |
| Title | string | Titlul |
| Author | string | Autor |
| Publisher | string | Editura |
| PublicationYear | int | Anul publicării |
| Category | string | Categoria |
| IsAvailable | bool | Disponibilitate |

### Tabela BookIssues
| Coloană | Tip | Descriere |
|---------|-----|-----------|
| BookIssueId | int | PK |
| BookId | int | FK la Books |
| MemberId | int | FK la Members |
| IssueDate | DateTime | Data împrumutului |
| DueDate | DateTime | Data scadentă |
| ReturnDate | DateTime? | Data returnării (null = nereturnată) |
| IssuedBy | string | Cine a procesat |

### Tabela Fines
| Coloană | Tip | Descriere |
|---------|-----|-----------|
| FineId | int | PK |
| BookIssueId | int | FK la BookIssues |
| Amount | decimal | Suma totală |
| PaidAmount | decimal | Suma plătită |
| DaysOverdue | int | Zile întârziere |
| IsPaid | bool | Status plată |

---

## Cum să rulezi proiectul:

### Cerințe:
- .NET 8.0 SDK
- Visual Studio 2022 sau VS Code
- SQL Server LocalDB (inclus cu Visual Studio)

### Pași:
1. Dezarhivează proiectul
2. Deschide soluția în Visual Studio
3. Restaurează pachetele NuGet:
   ```bash
   dotnet restore
   ```
4. Rulează aplicația:
   ```bash
   dotnet run
   ```

Baza de date se va crea automat la prima rulare cu date de test.

---

## Funcționalități UI:

### 📊 Dashboard
- Statistici în timp real
- Cărți împrumutate recent
- Cărți cu întârziere

### 📚 Cărți
- Adăugare/editare/ștergere cărți
- Căutare după titlu, autor, ISBN
- Vizualizare disponibilitate

### 👥 Membri
- Gestionare studenți și profesori
- Filtrare după tip
- Vizualizare limite

### 📖 Împrumuturi
- Împrumutare cu validare automată a limitelor
- Returnare cu calcul automat al amenzii
- Istoric complet

### 💰 Amenzi
- Lista amenzilor neplătite
- Procesare plăți
- Posibilitate de anulare (waiver)

---

## Reguli de validare:

1. **Nu se poate împrumuta** dacă membrul a atins limita maximă
2. **Nu se poate împrumuta** dacă cartea nu este disponibilă
3. **Nu se poate împrumuta** dacă membrul are amenzi neplătite > $10
4. **Amenda se calculează automat** la returnare cu întârziere
5. **Data scadentă** se calculează automat în funcție de tipul membrului

---

## Date de test incluse:

### Cărți (7 exemplare):
- Clean Code (2 copii)
- Design Patterns
- The Pragmatic Programmer
- Introduction to Algorithms
- Database System Concepts (2 copii)

### Membri:
- 3 studenți
- 2 profesori

---

## Contact

Proiect realizat pentru materia **Administrarea și Dezvoltarea Aplicațiilor cu Baze de Date**.
