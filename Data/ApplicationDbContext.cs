using BankMSWeb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BankMSWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Customer> tbl_Customers { get; set; }
        public DbSet<Account> tbl_Accounts{ get; set; }
        public DbSet<AccountView> AccountViews { get; set; }
        public DbSet<LoanVw> LoanVws { get; set; }
        public DbSet<TransactionDetailsVw> TransactionDetailsVws { get; set; }
        public DbSet<Transaction> tbl_Transactions{ get; set; }
        public DbSet<TransactionVw> TransactionVws { get; set; }
        public DbSet<Loan> tbl_Loans{ get; set; }
        public DbSet<Repayment> tbl_Repayments { get; set; }
        public DbSet<GeneratedAccountNoModel> GeneratedAccountNoModels{ get; set; }
        public DbSet<AccountInfoView> AccountInfoViews { get; set; }
        public DbSet<TransactionLimit> tbl_TransactionLimit { get; set; }
        public DbSet<TransactionHistory> tbl_TransactionHistory { get; set; }
        public DbSet<SavingPlan> tbl_SavingsPlan { get; set; }
        public DbSet<TotalAmountDashboard> TotalAmountDashboards { get; set; }
        public DbSet<TransactionDashboard> TransactionDashboards { get; set; }
        public DbSet<TransactionChart> TransactionCharts { get; set; }
        public DbSet<TransactionHistoryDashboard> TransactionHistoryDashboards { get; set; }
    }
}
