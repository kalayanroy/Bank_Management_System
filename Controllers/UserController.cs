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
    public class UserController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public UserController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager,
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
            try
            {
                var accounts = dbContext.tbl_Accounts
                .Include(a => a.Customer) // Load associated Customer data
                .OrderByDescending(x => x.AccountNo)
                .ToList();

                return View(accounts);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public IActionResult ChangeUserInfo(int accountId)
        {
            try
            {
                var accounts = dbContext.tbl_Accounts
                .Include(a => a.Customer) // Load associated Customer data
                .Where(x=>x.AccountId==accountId)
                .OrderByDescending(x => x.AccountNo)
                .ToList();

                return View(accounts.FirstOrDefault());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        [HttpPost]
        public async Task<IActionResult> ChangeUserInfo(Customer customer,string OldPassword,string NewPassword,Account account)
        {
            try
            {
                var user=await _userManager.FindByIdAsync(customer.UserGuidId);

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, OldPassword, NewPassword);
                if (!changePasswordResult.Succeeded)
                {

                }


                    var accounts = dbContext.tbl_Accounts
                .Include(a => a.Customer) // Load associated Customer data
                .Where(x => x.AccountId == account.AccountId)
                .OrderByDescending(x => x.AccountNo)
                .ToList();

                return View(accounts.FirstOrDefault());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
