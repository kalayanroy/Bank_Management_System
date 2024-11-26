using BankMSWeb.Areas.Identity.Pages.Account;
using BankMSWeb.Data;
using BankMSWeb.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace BankMSWeb.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public AccountsController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager,
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
        [HttpGet]
        public IActionResult GetCustomerInfo(int customerId)
        {
            var customer = dbContext.tbl_Customers
                .Where(c => c.CustomerId == customerId)
                .Select(c => new
                {
                    c.FirstName,
                    c.LastName,
                    c.Email,
                    c.Phone
                })
                .FirstOrDefault();

            if (customer == null)
            {
                return Json(new { success = false, message = "Customer not found" });
            }

            return Json(new
            {
                success = true,
                firstName = customer.FirstName,
                lastName = customer.LastName,
                email = customer.Email,
                phone = customer.Phone
            });
        }
        [HttpGet]
        public IActionResult GetAccountInfo(string accountNo)
        {
            var accountInfo = dbContext.AccountInfoViews.FromSqlRaw("exec sp_AccountInfoByAccountNo '" + accountNo + "'").ToList();
                

            if (accountInfo.Count() == 0)
            {
                return Json(new { success = false, message = "Account No not found" });
            }

            return Json(new
            {
                success = true,
                firstName = accountInfo.FirstOrDefault().firstName,
                lastName = accountInfo.FirstOrDefault().lastName,
                email = accountInfo.FirstOrDefault().Email,
                phone = accountInfo.FirstOrDefault().Phone,
                currentBalance= accountInfo.FirstOrDefault().CurrentBalance,
                customerId= accountInfo.FirstOrDefault().CustomerId,
                accountType= accountInfo.FirstOrDefault().AccountType,
                accountId= accountInfo.FirstOrDefault().AccountId
            });
        }
        [HttpPost]
        public async Task<JsonResult> AddCustomer([FromBody]Customer customer)
        {
            try
            {
                var register = new RegisterModel(_userManager, _userStore, _signInManager, _logger, _emailSender,dbContext);
                var newUserId = await register.NewUser(customer.Username, customer.Password);
                if (newUserId != null)
                {
                    var addCustomer = new Customer
                    {
                        Address = customer.Address,
                        CareatedAt = DateTime.Now,
                        Email = customer.Email,
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        Password = customer.Password,
                        Phone = customer.Phone,
                        Role = customer.Role,
                        Username = customer.Username,
                        UserGuidId= newUserId
                    };
                    dbContext.tbl_Customers.Add(addCustomer);
                    dbContext.SaveChanges();
                    return Json(new { success = true, customerId = addCustomer.CustomerId, username = customer.Username });
                }
                return Json(new { success = false, customerId = 0, username = string.Empty });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpGet]
        public IActionResult Create()
        {
            //ViewBag.AccountNo=dbContext.GeneratedAccountNoModels.FromSqlRaw("EXEC sp_generateAccountNo").FirstOrDefault().GeneratedAccountNo;
            var result = dbContext.Set<GeneratedAccountNoModel>()
                               .FromSqlRaw("EXEC sp_generateAccountNo")
                               .AsEnumerable()  // Forces execution on the database side
                               .FirstOrDefault();  // Get the first result (or null if none)

            // Return the generated AccountNo
            ViewBag.AccountNo = result?.GeneratedAccountNo;
            ViewBag.CustomerList = dbContext.tbl_Customers.ToList();
            return View();
        }
        // Create Account - POST
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Create(Account account)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = dbContext.Set<GeneratedAccountNoModel>()
                               .FromSqlRaw("EXEC sp_generateAccountNo")
                               .AsEnumerable()  // Forces execution on the database side
                               .FirstOrDefault();  // Get the first result (or null if none)

                    bool accountExists = dbContext.tbl_Accounts
                            .Any(a => a.AccountNo == account.AccountNo);
                    if(accountExists)
                    {
                        // Add a custom validation error if AccountNo is not unique
                        ModelState.AddModelError("AccountNo", "An account with this AccountNo already exists.");
                        ViewBag.AccountNo = result?.GeneratedAccountNo;
                        ViewBag.CustomerList = dbContext.tbl_Customers.ToList();
                        return View(account); // Return the view with the error message
                    }
                    account.AccountNo = result?.GeneratedAccountNo;
                    // Add to in-memory storage
                    dbContext.tbl_Accounts.Add(account);
                    dbContext.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                return View(account);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var account=dbContext.tbl_Accounts.Where(x => x.AccountId == id).FirstOrDefault();
            if (account == null) {
                return NotFound();
                    
            }
            ViewBag.CustomerList = dbContext.tbl_Customers.ToList();
            return View(account);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Edit(Account account)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingAccount = dbContext.tbl_Accounts.Where(a => a.AccountId == account.AccountId).FirstOrDefault();
                    if (existingAccount != null)
                    {
                        existingAccount.AccountType = account.AccountType;
                        existingAccount.Balance = account.Balance;
                        existingAccount.Currency = account.Currency;

                        dbContext.tbl_Accounts.Update(existingAccount); 
                        dbContext.SaveChanges();
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View(account);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var account = dbContext.tbl_Accounts.Where(x => x.AccountId == id).FirstOrDefault();
                    if (account == null)
                    {
                        return NotFound();

                    }
                    else
                    {
                        dbContext.tbl_Accounts.Remove(account); 
                        dbContext.SaveChanges();
                    }
                    return Json(account);//RedirectToAction(nameof(Index));
                }
                return Json(string.Empty);
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
