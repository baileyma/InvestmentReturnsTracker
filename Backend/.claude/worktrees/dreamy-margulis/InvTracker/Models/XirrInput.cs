namespace InvTracker.Models
{
    public record XirrInput(double StartingAmount, double EndAmount, IEnumerable<Payment> Payments, int EndMonth, int EndDay);
}
