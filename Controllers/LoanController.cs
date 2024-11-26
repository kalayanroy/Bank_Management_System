using BankMSWeb.Data;
using BankMSWeb.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankMSWeb.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class LoanController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        public LoanController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var loanApplyList=dbContext.tbl_Loans
                .Include(i => i.Account)
                .OrderByDescending(o => o.AppliedAt)
                .ToList();
            return View(loanApplyList);
        }
        [HttpGet]
        public IActionResult LoanApply()
        {
            ViewBag.CustomerList = dbContext.tbl_Customers.ToList();
            return View();
        }
        [HttpPost]
        public IActionResult LoanApply(Loan loan)
        {
            try
            {
                loan.AppliedAt = DateTime.Now;
                loan.LoanStatus = "Pending";

                dbContext.tbl_Loans.Add(loan);
                dbContext.SaveChanges();
                ViewBag.CustomerList = dbContext.tbl_Customers.ToList();
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
        [HttpPost]
        public IActionResult UpdateStatus(int masterId, string status)
        {
            try
            {
                var loanInfo=dbContext.tbl_Loans.Where(x => x.LoanId == masterId).FirstOrDefault();
                if (loanInfo != null)
                {
                    loanInfo.LoanStatus = status;
                    loanInfo.ApprovedBy = User.Identity.Name;
                    loanInfo.ApprovedAt = DateTime.Now;
                    dbContext.tbl_Loans.Update(loanInfo);
                    dbContext.SaveChanges();

                    if(status== "Approved")
                    {
                        var accountInfo = dbContext.tbl_Accounts.Where(x => x.AccountId == loanInfo.AccountId).FirstOrDefault();

                        dbContext.tbl_Transactions.Add(new Models.Transaction
                        {
                            Amount = loanInfo.LoanAmount??0,
                            Currency = "BDT",
                            TransactionType = "Loan",
                            Remarks = loanInfo.LoanAmount + " Amount Loan in Account No:" + accountInfo.AccountNo,
                            TransactionDate = DateTime.Now,
                            AccountId = loanInfo.AccountId??0,

                        });
                        dbContext.SaveChanges();

                        dbContext.tbl_TransactionHistory.Add(new TransactionHistory
                        {
                            AccountId = loanInfo.AccountId ?? 0,
                            AccountType = "Loan",
                            Amount = loanInfo.LoanAmount??0,
                            Date = DateTime.Now,
                            Fee = 0,
                            Remarks = "Loan To " + loanInfo.LoanAmount
                        });
                        dbContext.SaveChanges();

                        decimal monthlyInstallment = (loanInfo.LoanAmount *(1 + loanInfo.InterestRate/100))/Convert.ToDecimal(loanInfo.Term)??0;

                        for (int i = 1; i <= Convert.ToDecimal(loanInfo.Term); i++)
                        {
                            dbContext.tbl_Repayments.Add(new Repayment
                            {
                                LoanID=loanInfo.LoanId,
                                DueDate=Convert.ToDateTime(loanInfo.ApprovedAt).AddMonths(i),
                                AmountDue=monthlyInstallment,
                                IsPaid=false
                            });
                            dbContext.SaveChanges();
                        }
                        return Json(new { status = "success", loanAmount = loanInfo.LoanAmount });
                    }
                    else if(status == "Rejected")
                    {
                        dbContext.tbl_TransactionHistory.Add(new TransactionHistory
                        {
                            AccountId = loanInfo.AccountId ?? 0,
                            AccountType = "Loan",
                            Amount = loanInfo.LoanAmount ?? 0,
                            Date = DateTime.Now,
                            Fee = 0,
                            Remarks = "Loan is Rejected! Amount is " + loanInfo.LoanAmount
                        });
                        dbContext.SaveChanges();
                        return Json(new { status = "warning", loanAmount = loanInfo.LoanAmount });
                    }
                }
                return Json(new { status = "error", loanAmount = loanInfo.LoanAmount });
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public IActionResult LoanCollection()
        {

            ViewBag.CustomerList = dbContext.tbl_Customers.ToList();
            return View();
        }
        [HttpGet]
        public IActionResult GetAccountLoanInfo(string accountNo)
        {
            var accountInfo = dbContext.AccountInfoViews.FromSqlRaw("exec sp_AccountInfoByAccountNo '" + accountNo + "'").ToList();


            if (accountInfo.Count() == 0)
            {
                return Json(new { success = false, message = "Account No not found" });
            }
           
            var loanList=dbContext.tbl_Loans.Where(x => x.AccountId == accountInfo.FirstOrDefault().AccountId).ToList();

            List<RepaymentViewModel> loanRepayment = new List<RepaymentViewModel>();
            foreach (var loan in loanList)
            {
                var installmentNo=dbContext.tbl_Repayments.Where(x => x.LoanID == loan.LoanId && x.IsPaid == true).Count()+1;
                var loanInfo=dbContext.tbl_Repayments.Where(x => x.LoanID == loan.LoanId).FirstOrDefault();
                loanRepayment.Add(new RepaymentViewModel
                {
                    LoanID=loan.LoanId,
                    AccountId=loan.AccountId,
                    AmountDue=loanInfo.AmountDue,
                    InstallmentNo=installmentNo.ToString(),
                    InterestRate=loan.InterestRate,
                    LoanAmount=loan.LoanAmount,
                    LoanStatus=loan.LoanStatus,
                    LoanType=loan.LoanType,
                    MonthlyInstallmentAmount=loanInfo.AmountDue,
                    Term=loan.Term,
                    RepaymentID=loanInfo.RepaymentID,
                    DueDate= dbContext.tbl_Repayments.Where(x => x.LoanID == loan.LoanId && x.IsPaid == false).FirstOrDefault().DueDate,
                     
                });
            }
            
            return Json(new
            {
                success = true,
                firstName = accountInfo.FirstOrDefault().firstName,
                lastName = accountInfo.FirstOrDefault().lastName,
                email = accountInfo.FirstOrDefault().Email,
                phone = accountInfo.FirstOrDefault().Phone,
                currentBalance = accountInfo.FirstOrDefault().CurrentBalance,
                customerId = accountInfo.FirstOrDefault().CustomerId,
                accountType = accountInfo.FirstOrDefault().AccountType,
                accountId = accountInfo.FirstOrDefault().AccountId,
                loanInfoDetails= loanRepayment
            });
        }
        [HttpPost]
        public IActionResult LoanCollection(Repayment repayment)
        {
            ViewBag.CustomerList = dbContext.tbl_Customers.ToList();
            var repaymentInfo=dbContext.tbl_Repayments.Where(x => x.RepaymentID == repayment.RepaymentID).FirstOrDefault();
            if (repaymentInfo != null)
            {
                repaymentInfo.IsPaid = true;
                repaymentInfo.Penalty = repayment.Penalty;
                repaymentInfo.AmountPaid = repayment.AmountPaid;
                dbContext.tbl_Repayments.Update(repaymentInfo);
                dbContext.SaveChanges();
                ModelState.AddModelError("success", "Collection Save Successfully");
                return View();
            }
            else
            {
                ModelState.AddModelError("error", "Loan Information not found");
                return View();
            }
        }

    }
}
