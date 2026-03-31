namespace InvTracker.Models
{
    public class PaymentToAdd
    {
        public int AccountId { get; set; }

        public double Amount { get; set; }

        public int Month { get; set; }

        public int Day { get; set; }

        public int Year { get; set; }

        public DateOnly Date => new DateOnly(Year, Month, Day);
    }
}
