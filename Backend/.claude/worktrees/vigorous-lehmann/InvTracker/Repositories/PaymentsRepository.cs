using InvTracker.DbContexts;
using InvTracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace InvTracker.Repositories
{
    public interface IPaymentsRepository
    {
        Task PostPayment(PaymentToAdd payment);
        Task<Payment> DeletePayment(int paymentId);

        Task<List<Payment>> GetAllPayments();

        Task<Payment?> GetPaymentById(int paymentId);

        Task<List<Payment>> GetPaymentsByIdAndYear(int accountId, int year);

        Task<Dictionary<int, List<Payment>>> GetPaymentsLookupByYearAsync();
    }

    public class PaymentsRepository : IPaymentsRepository
    {

        private readonly InvTrackerContext _context;
        private readonly IMemoryCache _cache;

        public PaymentsRepository(InvTrackerContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task PostPayment(PaymentToAdd payment)
        {
            var paymentToAdd = new Payment()
            {
                AccountId = payment.AccountId,
                Date = payment.Date,
                Amount = payment.Amount
            };

            await _context.Payments.AddAsync(paymentToAdd);
            await _context.SaveChangesAsync();

        }

        public async Task<Payment> DeletePayment(int paymentId)
        {
            var payment = await GetPaymentById(paymentId);
            _cache.Remove($"accountyearlyreturns_{payment.AccountId}_{payment.Date.Year}");

            //if (payment is null)
            //{
            //    return;
            //}
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<List<Payment>> GetPaymentsByAccountId(int accountId)
        {
            var payments = await _context.Payments.ToListAsync();

            return payments.Where(x => x.AccountId == accountId).ToList();
        }

        public async Task<Payment?> GetPaymentById(int paymentId)
        {
            var payments = await _context.Payments.ToListAsync();

            return payments.FirstOrDefault(x => x.Id == paymentId);
        }

        public async Task<List<Payment>> GetPaymentsByIdAndYear(int accountId, int year)
        {
            var payments = await _context.Payments.ToListAsync();

            return payments.Where(x => x.AccountId == accountId)
                .Where(x => x.Date.Year == year)
                .ToList();
        }

        //public async Task<Dictionary<int, Dictionary<int, IEnumerable<Payment>>>> GetAllPaymentsLookupByAccountIdAndYear(int[] accountIdArray)
        //{
        //    var dict = new Dictionary<int, Dictionary<int, IEnumerable<Payment>>>();

        //    foreach (var accountId in accountIdArray)
        //    {
        //        var xxx = await GetPaymentsLookupByAccountId(accountId);
        //        dict.Add(accountId, xxx);
        //    }

        //    return dict;
        //}

        //public async Task<Dictionary<int, IEnumerable<Payment>>> GetPaymentsLookupByAccountId(int accountId)
        //{
            
        //    var accountDictByYear = new Dictionary<int, IEnumerable<Payment>>();

        //    var paymentsList = await GetPaymentsByAccountId(accountId);

        //    // list of payments for that accountid
        //    // so need to group them by year and turn into a dictionary

        //    var paymentsGroupedByYear = paymentsList.GroupBy(x => x.Date.Year);

        //    foreach (var year in paymentsGroupedByYear)
        //    {
        //        accountDictByYear.Add(year.Key, year);
        //    }

        //    return accountDictByYear;
        //}

        // Dictionary indexed by accountId and then year
        public async Task<Dictionary<int, List<Payment>>> GetPaymentsLookupByYearAsync()
        {
            var allPayments = await _context.Payments.ToListAsync();

            var lookup = allPayments.GroupBy(x => x.Date.Year)
                 .ToDictionary(g => g.Key, x => x.ToList());

            return lookup;
        }

        

        public async Task<List<Payment>> GetAllPayments()
        {
            return await _context.Payments.ToListAsync();
        }
    }
}
