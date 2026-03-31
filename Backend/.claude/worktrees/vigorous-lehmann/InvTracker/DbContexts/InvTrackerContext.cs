using InvTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace InvTracker.DbContexts
{
    public class InvTrackerContext : DbContext
    {
        public InvTrackerContext(DbContextOptions<InvTrackerContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Balance> Balances { get; set; }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Balance>(entity =>
            {
                entity.ToTable("balances");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.StartingBalance).HasColumnName("start");
                entity.Property(e => e.EndBalance).HasColumnName("end");
                entity.Property(e => e.AccountId).HasColumnName("account_id");
                entity.Property(e => e.Year).HasColumnName("year");
                entity.Property(e => e.Month).HasColumnName("month");
                entity.Property(e => e.Day).HasColumnName("day");
            });

            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("accounts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.AccountId).HasColumnName("account_id");
                entity.Property(e => e.Date).HasColumnName("date");
                entity.Property(e => e.Amount).HasColumnName("payment");
            });
        }
    }
}