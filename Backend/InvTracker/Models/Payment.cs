namespace InvTracker.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public DateOnly Date { get; set; }

        public double Amount { get; set; }
    }
}
