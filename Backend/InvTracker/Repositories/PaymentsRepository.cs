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

        Task<List<Payment>> GetPaymentsByAccountId(int accountId);
        Task<Payment?> GetPaymentById(int paymentId);


        Task<List<Payment>> GetPaymentsByIdAndYearAsync(string userId, int accountId, int year);

        Task<Dictionary<int, List<Payment>>> GetPaymentsLookupByYearAsync(string userId);

        Task<List<Payment>> GetAllPaymentsAsync(string userId);
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

        public async Task<List<Payment>> GetPaymentsByIdAndYearAsync(string userId, int accountId, int year)
        {
            var allPaymentsByUser = await GetAllPaymentsAsync(userId);

            return allPaymentsByUser.Where(x => x.AccountId == accountId)
                .Where(x => x.Date.Year == year)
                .ToList();
        }

        // Dictionary indexed by accountId and then year
        public async Task<Dictionary<int, List<Payment>>> GetPaymentsLookupByYearAsync(string userId)
        {
            var allPaymentsByUser = await GetAllPaymentsAsync(userId);

            var lookup = allPaymentsByUser.GroupBy(x => x.Date.Year)
                 .ToDictionary(g => g.Key, x => x.ToList());

            return lookup;
        }

        

        public async Task<List<Payment>> GetAllPaymentsAsync(string userId)
        {
            var userAccountIds = _context.Accounts.Where(x => x.UserId == userId).Select(x => x.Id).ToHashSet();

            return await _context.Payments.Where(x => userAccountIds.Contains(x.AccountId)).ToListAsync();
        }
    }
}
