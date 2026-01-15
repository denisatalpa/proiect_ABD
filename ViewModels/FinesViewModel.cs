using System.Collections.ObjectModel;
using System.Windows.Input;
using LibraryManagementSystem.Commands;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.ViewModels;

/// <summary>
/// ViewModel for managing fines
/// </summary>
public class FinesViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;

    public FinesViewModel(LibraryService libraryService)
    {
        _libraryService = libraryService;
        Fines = new ObservableCollection<Fine>();

        // Initialize commands
        LoadDataCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
        PayFineCommand = new AsyncRelayCommand(async _ => await PayFineAsync(), _ => CanPayFine());
        WaiveFineCommand = new AsyncRelayCommand(async _ => await WaiveFineAsync(), _ => CanWaiveFine());
        ShowPendingFinesCommand = new AsyncRelayCommand(async _ => await ShowPendingFinesAsync());
        ShowAllFinesCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
    }

    #region Properties

    public ObservableCollection<Fine> Fines { get; }

    private Fine? _selectedFine;
    public Fine? SelectedFine
    {
        get => _selectedFine;
        set
        {
            if (SetProperty(ref _selectedFine, value) && value != null)
            {
                PaymentAmount = value.RemainingAmount;
            }
        }
    }

    private decimal _paymentAmount;
    public decimal PaymentAmount
    {
        get => _paymentAmount;
        set => SetProperty(ref _paymentAmount, value);
    }

    private string _waiveReason = string.Empty;
    public string WaiveReason
    {
        get => _waiveReason;
        set => SetProperty(ref _waiveReason, value);
    }

    private decimal _totalPendingFines;
    public decimal TotalPendingFines
    {
        get => _totalPendingFines;
        set => SetProperty(ref _totalPendingFines, value);
    }

    #endregion

    #region Commands

    public ICommand LoadDataCommand { get; }
    public ICommand PayFineCommand { get; }
    public ICommand WaiveFineCommand { get; }
    public ICommand ShowPendingFinesCommand { get; }
    public ICommand ShowAllFinesCommand { get; }

    #endregion

    #region Methods

    public async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;
            var fines = await _libraryService.GetAllFinesAsync();
            
            Fines.Clear();
            foreach (var fine in fines)
            {
                Fines.Add(fine);
            }

            // Calculate total pending
            TotalPendingFines = fines
                .Where(f => f.Status == "Pending" || f.Status == "Partial")
                .Sum(f => f.RemainingAmount);

            SetStatus($"S-au încărcat {fines.Count} amenzi. Total restant: {TotalPendingFines:C}");
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la încărcarea amenzilor: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ShowPendingFinesAsync()
    {
        try
        {
            IsBusy = true;
            var fines = await _libraryService.GetPendingFinesAsync();
            
            Fines.Clear();
            foreach (var fine in fines)
            {
                Fines.Add(fine);
            }

            TotalPendingFines = fines.Sum(f => f.RemainingAmount);
            SetStatus($"Se afișează {fines.Count} amenzi restante. Total: {TotalPendingFines:C}");
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanPayFine()
    {
        return SelectedFine != null && !SelectedFine.IsPaid && PaymentAmount > 0;
    }

    private async Task PayFineAsync()
    {
        if (SelectedFine == null) return;

        try
        {
            IsBusy = true;

            var result = await _libraryService.PayFineAsync(SelectedFine.FineId, PaymentAmount);

            if (result.Success)
            {
                await LoadDataAsync();
                SetStatus(result.Message);
            }
            else
            {
                SetStatus(result.Message, true);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la procesarea plății: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanWaiveFine()
    {
        return SelectedFine != null && !SelectedFine.IsPaid && !string.IsNullOrWhiteSpace(WaiveReason);
    }

    private async Task WaiveFineAsync()
    {
        if (SelectedFine == null) return;

        try
        {
            IsBusy = true;

            var result = await _libraryService.WaiveFineAsync(SelectedFine.FineId, WaiveReason);

            if (result.Success)
            {
                await LoadDataAsync();
                WaiveReason = string.Empty;
                SetStatus(result.Message);
            }
            else
            {
                SetStatus(result.Message, true);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Eroare la anularea amenzii: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion
}
