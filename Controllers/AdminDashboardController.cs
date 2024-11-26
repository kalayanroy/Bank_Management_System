using BankMSWeb.Areas.Identity.Pages.Account;
using BankMSWeb.Data;
using BankMSWeb.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankMSWeb.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public AdminDashboardController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            this.dbContext = dbContext;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            var totalAmountDashboards=dbContext.TotalAmountDashboards.FromSqlRaw("select TransactionType,sum(Amount)TotalAmount from tbl_Transactions\r\ngroup by TransactionType").ToList();

            var transactionDashboards=dbContext.TransactionDashboards.FromSqlRaw("select top 5 t.TransactionId,a.AccountNo,c.Username,t.Amount,t.TransactionDate from tbl_Transactions t\r\ninner join tbl_Accounts a on t.AccountId=a.AccountId\r\ninner join tbl_Customers c on a.CustomerId=c.CustomerId\r\norder by CONVERT(date,TransactionDate) desc").ToList();

            var transactionCharts=dbContext.TransactionCharts.FromSqlRaw("select Convert(decimal(18,0),ROW_NUMBER()Over(order by TransactionType))SL,TransactionType,CONVERT(date,TransactionDate)TransactionDate,sum(Amount)amount from tbl_Transactions\r\ngroup by TransactionType,CONVERT(date,TransactionDate)\r\norder by CONVERT(date,TransactionDate)").ToList();

            var transactionHistories=dbContext.TransactionHistoryDashboards.FromSqlRaw("select top 10 t.TransactionId,a.AccountNo,c.Username,t.TransactionType,t.Amount,t.TransactionDate from tbl_Transactions t\r\ninner join tbl_Accounts a on t.AccountId=a.AccountId\r\ninner join tbl_Customers c on a.CustomerId=c.CustomerId\r\norder by CONVERT(date,TransactionDate) desc").ToList();

            Dashboard dashboard = new Dashboard();
            dashboard.totalAmountDashboard = totalAmountDashboards;
            dashboard.transactionDashboard = transactionDashboards;
            dashboard.transactionChart=transactionCharts;
            dashboard.transactionHistory=transactionHistories;

            return View(dashboard);
        }
        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
        [HttpGet]
        public IActionResult GetTransactionChartData()
        {
            //var chartData = dbContext.tbl_Transactions
            //    .GroupBy(t => new { t.TransactionType, t.TransactionDate })
            //    .Select(g => new
            //    {
            //        TransactionDate = g.Key.TransactionDate.ToString("yyyy-MM-dd"),
            //        TransactionType = g.Key.TransactionType,
            //        Amount = g.Sum(t => t.Amount)
            //    })
            //    .AsEnumerable()
            //    .OrderBy(data => data.TransactionDate)
            //    .ToList();

            var transactionCharts = dbContext.TransactionCharts.FromSqlRaw("select Convert(decimal(18,0),ROW_NUMBER()Over(order by TransactionType))SL,TransactionType,CONVERT(date,TransactionDate)TransactionDate,sum(Amount)amount from tbl_Transactions\r\ngroup by TransactionType,CONVERT(date,TransactionDate)\r\norder by CONVERT(date,TransactionDate)").ToList();


            return Json(transactionCharts);
        }
    }
}
