namespace InvTracker.Models;

public class AccountYearlyDetail
{
    public required Balance Balance { get; set; }

    public required List<Payment> Payments { get; set; }

    public double NetDeposits => Payments.Select(x => x.Amount).Sum();

    public double TotalDeposits => Payments.Where(x => x.Amount > 0).Sum(x => x.Amount);

    public double TotalWithdrawals => Payments.Where(x => x.Amount < 0).Sum(x => x.Amount);

    public double XIRR { get; set; }


}
