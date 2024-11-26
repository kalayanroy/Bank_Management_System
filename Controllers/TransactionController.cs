using BankMSWeb.Data;
using BankMSWeb.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Transactions;

namespace BankMSWeb.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        public TransactionController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var transactionHistories = dbContext.tbl_TransactionHistory
                .Include(a => a.Account) // Load associated Customer data
                .OrderByDescending(x => x.Date)
                .ToList();

            return View(transactionHistories);
        }
        [HttpGet]
        public IActionResult Deposit()
        {
            ViewBag.CustomerList = dbContext.tbl_Customers.ToList();
            return View();
        }
        [HttpPost]
        public IActionResult Deposit(Models.Transaction transaction)
        {
            try
            {
                if (transaction.AccountId != null && transaction.AccountId!=0)
                {
                    ViewBag.CustomerList = dbContext.tbl_Customers.ToList();

                    var accountInfo = dbContext.tbl_Accounts.Where(x => x.AccountId == transaction.AccountId).FirstOrDefault();
                    var dailyLimit = dbContext.tbl_TransactionLimit.Where(x => x.AccountType == transaction.TransactionType).FirstOrDefault();
                    decimal totalToday = dbContext.tbl_Transactions
    .Where(x => x.AccountId == transaction.AccountId &&
                x.TransactionDate.Date == transaction.TransactionDate.Date)
    .Sum(x => x.Amount); 
                    if (totalToday + transaction.Amount > dailyLimit.TransactionDailyLimit)
                    {
                        ModelState.AddModelError("error", "Daily limit exceeded");
                        return View();//Json(new { status = "error", message = "Daily limit exceeded" });
                    }
                    dbContext.tbl_Transactions.Add(new Models.Transaction
                    {
                        Amount = (transaction.Amount - dailyLimit.TransactionFee),
                        Currency = "BDT",
                        TransactionType = "Deposit",
                        Remarks = (transaction.Amount - dailyLimit.TransactionFee) + " Amount Deposit in Account No:" + accountInfo.AccountNo,
                        TransactionDate = transaction.TransactionDate,
                        AccountId = transaction.AccountId,

                    });
                    dbContext.SaveChanges();

                    dbContext.tbl_TransactionHistory.Add(new TransactionHistory
                    {
                        AccountId = transaction.AccountId,
                        AccountType = "Deposit",
                        Amount = transaction.Amount,
                        Date = transaction.TransactionDate,
                        Fee = dailyLimit.TransactionFee,
                        Remarks= "Deposit To "+ transaction.Amount
                    });
                    dbContext.SaveChanges();
                    ViewBag.CustomerList = dbContext.tbl_Customers.ToList();
                    ModelState.AddModelError("success", "Deposit Save Successfully");
                    

                    return View();
                    //return Json(new { status = "success", newBalance = transaction.Amount});
                }
                
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            ModelState.AddModelError("error", "Account not found");
            return View();
            //return Json(new { status = "error", message = "Account not found" });
        }

        [HttpGet]
        public IActionResult Withdrawal()
        {
            ViewBag.CustomerList = dbContext.tbl_Customers.ToList();
            return View();
        }
        [HttpPost]
        public IActionResult Withdrawal(Models.Transaction transaction)
        {
            try
            {
                if (transaction.AccountId != null && transaction.AccountId != 0)
                {
                    ViewBag.CustomerList = dbContext.tbl_Customers.ToList();

                    var accountInfo = dbContext.tbl_Accounts.Where(x => x.AccountId == transaction.AccountId).FirstOrDefault();
                    var dailyLimit = dbContext.tbl_TransactionLimit.Where(x => x.AccountType == transaction.TransactionType).FirstOrDefault();
                    decimal totalToday = dbContext.tbl_Transactions
                    .Where(x => x.AccountId == transaction.AccountId &&
                    x.TransactionDate.Date == transaction.TransactionDate.Date)
                    .Sum(x => x.Amount);
                    if (totalToday + transaction.Amount > dailyLimit.TransactionDailyLimit)
                    {
                        ModelState.AddModelError("error", "Daily limit exceeded");
                        return View();//Json(new { status = "error", message = "Daily limit exceeded" });
                    }
                    var accountInfoVW = dbContext.AccountInfoViews.FromSqlRaw("exec sp_AccountInfoByAccountNo '" + accountInfo.AccountNo + "'").ToList();
                    if (accountInfoVW.Count() > 0)
                    {
                        if (accountInfoVW.FirstOrDefault().CurrentBalance >= (transaction.Amount + dailyLimit.TransactionFee))
                        {
                            dbContext.tbl_Transactions.Add(new Models.Transaction
                            {
                                Amount = (transaction.Amount + dailyLimit.TransactionFee),
                                Currency = "BDT",
                                TransactionType = "Withdrawal",
                                Remarks = (transaction.Amount + dailyLimit.TransactionFee) + " Amount Withdrawal in Account No:" + accountInfo.AccountNo,
                                TransactionDate = transaction.TransactionDate,
                                AccountId = transaction.AccountId,

                            });
                            dbContext.SaveChanges();

                            dbContext.tbl_TransactionHistory.Add(new TransactionHistory
                            {
                                AccountId = transaction.AccountId,
                                AccountType = "Withdrawal",
                                Amount = transaction.Amount,
                                Date = transaction.TransactionDate,
                                Fee = dailyLimit.TransactionFee,
                                Remarks = "Withdrawal To " + transaction.Amount
                            });
                            dbContext.SaveChanges();
                            ModelState.AddModelError("success", "Withdrawal Save Successfully");
                        }
                        else
                        {
                            ModelState.AddModelError("error", "Insufficient balance");
                        }
                    }
                    
                    ViewBag.CustomerList = dbContext.tbl_Customers.ToList();
                    


                    return View();
                    //return Json(new { status = "success", newBalance = transaction.Amount});
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            ModelState.AddModelError("error", "Account not found");
            return View();
            //return Json(new { status = "error", message = "Account not found" });
        }

        [HttpGet]
        public IActionResult FundTransfer()
        {
            ViewBag.CustomerList = dbContext.tbl_Customers.ToList();
            return View();
        }
        [HttpPost]
        public IActionResult FundTransfer(Models.Transaction transaction,string transferToAccountId)
        {
            try
            {
                if (transaction.AccountId != null && transaction.AccountId != 0 && Convert.ToInt32(transferToAccountId)!=0)
                {
                    ViewBag.CustomerList = dbContext.tbl_Customers.ToList();

                    var accountInfo = dbContext.tbl_Accounts.Where(x => x.AccountId == transaction.AccountId).FirstOrDefault();
                    var transferToaccountInfo = dbContext.tbl_Accounts.Where(x => x.AccountId == Convert.ToInt32(transferToAccountId)).FirstOrDefault();
                    
                    
                    var accountInfoVW = dbContext.AccountInfoViews.FromSqlRaw("exec sp_AccountInfoByAccountNo '" + accountInfo.AccountNo + "'").ToList();
                    if (accountInfoVW.Count() > 0)
                    {
                        if (accountInfoVW.FirstOrDefault().CurrentBalance >= (transaction.Amount))
                        {

                            ///Transfer From Part
                            dbContext.tbl_Transactions.Add(new Models.Transaction
                            {
                                Amount = (transaction.Amount),
                                Currency = "BDT",
                                TransactionType = "Fund Transfer",
                                Remarks = (transaction.Amount) + " Amount Fund Transfer From:" + accountInfo.AccountNo+" To "+ transferToaccountInfo.AccountNo,
                                TransactionDate = transaction.TransactionDate,
                                AccountId = transaction.AccountId,
                            });
                            dbContext.SaveChanges();

                            dbContext.tbl_TransactionHistory.Add(new TransactionHistory
                            {
                                AccountId = transaction.AccountId,
                                AccountType = "Fund Transfer",
                                Amount = transaction.Amount,
                                Date = transaction.TransactionDate,
                                Fee = decimal.Zero,
                                Remarks = "Fund Transfer From " + accountInfo.AccountNo + " To " + transferToaccountInfo.AccountNo + ". Amount is " + transaction.Amount
                            }) ;
                            dbContext.SaveChanges();

                            ///Transfer To Part
                            dbContext.tbl_Transactions.Add(new Models.Transaction
                            {
                                Amount = (transaction.Amount),
                                Currency = "BDT",
                                TransactionType = "Deposit",
                                Remarks = (transaction.Amount) + " Amount Fund Transfer To:" + accountInfo.AccountNo,
                                TransactionDate = transaction.TransactionDate,
                                AccountId = Convert.ToInt32(transferToAccountId),
                            });
                            dbContext.SaveChanges();

                            dbContext.tbl_TransactionHistory.Add(new TransactionHistory
                            {
                                AccountId = Convert.ToInt32(transferToAccountId),
                                AccountType = "Deposit",
                                Amount = transaction.Amount,
                                Date = transaction.TransactionDate,
                                Fee = decimal.Zero,
                                Remarks = "Fund Transfer To " + accountInfo.AccountNo + ". Amount is " + transaction.Amount
                            });
                            dbContext.SaveChanges();

                            ModelState.AddModelError("success", "Fund Transfer Save Successfully");
                        }
                        else
                        {
                            ModelState.AddModelError("error", "Insufficient balance");
                        }
                    }

                    ViewBag.CustomerList = dbContext.tbl_Customers.ToList();



                    return View();
                    //return Json(new { status = "success", newBalance = transaction.Amount});
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            ModelState.AddModelError("error", "Account not found");
            return View();
            //return Json(new { status = "error", message = "Account not found" });
        }

    }
}
