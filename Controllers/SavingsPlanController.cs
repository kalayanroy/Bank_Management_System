using BankMSWeb.Data;
using BankMSWeb.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

namespace BankMSWeb.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class SavingsPlanController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        public SavingsPlanController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var savingPlans=dbContext.tbl_SavingsPlan
                .Include(x => x.Account)
                .OrderByDescending(x => x.PlanID)
                .ToList();
            return View(savingPlans);
        }
        public IActionResult create()
        {
            ViewBag.CustomerList = dbContext.tbl_Customers.ToList();
            return View();
        }
        [HttpPost]
        public IActionResult create(SavingPlan savingPlan)
        {
            try
            {
                var maturityAmount = Convert.ToDecimal(savingPlan.PrincipalAmount) + Convert.ToDecimal(savingPlan.PrincipalAmount) * ((1 + Convert.ToDecimal(savingPlan.InterestRate) / 100) / Convert.ToDecimal(savingPlan.DurationMonths) / 12);
                savingPlan.MaturityAmount= maturityAmount;

                savingPlan.MaturityDate = savingPlan.StartDate.AddMonths(Convert.ToInt32(savingPlan.DurationMonths));
                dbContext.tbl_SavingsPlan.Add(savingPlan);
                dbContext.SaveChanges();
                ViewBag.CustomerList = dbContext.tbl_Customers.ToList();
                ModelState.AddModelError("success", "Saving Plan Save Successfully");
                return View();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
        public IActionResult Credit()
        {
            var savingPlans = dbContext.tbl_SavingsPlan
                .Include(x => x.Account)
                .OrderByDescending(x => x.PlanID)
                .ToList();
            return View(savingPlans);
        }
    }
}
