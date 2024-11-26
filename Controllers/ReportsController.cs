using BankMSWeb.Data;
using BankMSWeb.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using OfficeOpenXml;
using Rotativa.AspNetCore;

namespace BankMSWeb.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        public ReportsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var accounts = dbContext.tbl_Accounts
                .Include(a => a.Customer) // Load associated Customer data
                .OrderByDescending(x => x.AccountNo)
                .ToList();
            ViewBag.AccountList = accounts;
            return View();
        }

        public JsonResult FindValues(int accountId, DateTime fromDate, DateTime toDate)
        {
            var transactions = dbContext.TransactionVws
                        .FromSqlInterpolated($"EXEC GetMonthlyStatement {accountId}, {fromDate}, {toDate}")
                        .ToList();
            
            return Json(new { success=true,Model= transactions });
        }
        public IActionResult Details()
        {
            var accounts = dbContext.tbl_Accounts
                .Include(a => a.Customer) // Load associated Customer data
                .OrderByDescending(x => x.AccountNo)
                .ToList();
            ViewBag.AccountList = accounts;
            return View();
        }

        public JsonResult FindDetailsValues(int accountId, DateTime fromDate, DateTime toDate)
        {
            // Account Summary
            var accountSummary = dbContext.AccountViews
                .FromSqlInterpolated($"EXEC GetDetailedReport {accountId}, {fromDate}, {toDate}")
                .AsEnumerable()
                .FirstOrDefault();


            var loanList = dbContext.LoanVws
                        .FromSqlInterpolated($"EXEC GetDetailedReportLoanList {accountId}, {fromDate}, {toDate}")
                        .AsEnumerable()
                        .ToList();
            var transactions = dbContext.TransactionDetailsVws
                        .FromSqlInterpolated($"EXEC GetTransationHistoryMonthlyStatement {accountId}, {fromDate}, {toDate}")
                        .AsEnumerable()
                        .ToList();

            DetailsStatements detailsStatements = new DetailsStatements(){
                AccountSummary=accountSummary,
                LoanList=loanList,
                TransactionDetailsVws = transactions
            };
            return Json(new { success = true, Model = detailsStatements });
        }
        public IActionResult ExportToPDF(int accountId, DateTime fromDate, DateTime toDate)
        {
            // Account Summary
            var accountSummary = dbContext.AccountViews
                .FromSqlInterpolated($"EXEC GetDetailedReport {accountId}, {fromDate}, {toDate}")
                .AsEnumerable()
                .FirstOrDefault();


            var loanList = dbContext.LoanVws
                        .FromSqlInterpolated($"EXEC GetDetailedReportLoanList {accountId}, {fromDate}, {toDate}")
                        .AsEnumerable()
                        .ToList();
            var transactions = dbContext.TransactionDetailsVws
                        .FromSqlInterpolated($"EXEC GetTransationHistoryMonthlyStatement {accountId}, {fromDate}, {toDate}")
                        .AsEnumerable()
                        .ToList();

            DetailsStatements detailsStatements = new DetailsStatements()
            {
                AccountSummary = accountSummary,
                LoanList = loanList,
                TransactionDetailsVws = transactions
            };
            //var transactions = GetTransactionsForAccount(accountId); // Get transactions
            return new ViewAsPdf("PrintPreview", detailsStatements);
        }
        public IActionResult PrintPreview(int accountId, DateTime fromDate, DateTime toDate)
        {
            // Account Summary
            var accountSummary = dbContext.AccountViews
                .FromSqlInterpolated($"EXEC GetDetailedReport {accountId}, {fromDate}, {toDate}")
                .AsEnumerable()
                .FirstOrDefault();


            var loanList = dbContext.LoanVws
                        .FromSqlInterpolated($"EXEC GetDetailedReportLoanList {accountId}, {fromDate}, {toDate}")
                        .AsEnumerable()
                        .ToList();
            var transactions = dbContext.TransactionDetailsVws
                        .FromSqlInterpolated($"EXEC GetTransationHistoryMonthlyStatement {accountId}, {fromDate}, {toDate}")
                        .AsEnumerable()
                        .ToList();

            DetailsStatements detailsStatements = new DetailsStatements()
            {
                AccountSummary = accountSummary,
                LoanList = loanList,
                TransactionDetailsVws = transactions
            };
            return View(detailsStatements);

        }
        public IActionResult ExportToExcel(int accountId, DateTime fromDate, DateTime toDate)
        {
            // Account Summary
            var accountSummary = dbContext.AccountViews
                .FromSqlInterpolated($"EXEC GetDetailedReport {accountId}, {fromDate}, {toDate}")
                .AsEnumerable()
                .FirstOrDefault();

            var transactions = dbContext.TransactionDetailsVws
                         .FromSqlInterpolated($"EXEC GetTransationHistoryMonthlyStatement {accountId}, {DateTime.Now.AddYears(-1)}, {DateTime.Now}")
                         .AsEnumerable()
                         .ToList();
            var loanList = dbContext.LoanVws
                        .FromSqlInterpolated($"EXEC GetDetailedReportLoanList {accountId}, {fromDate}, {toDate}")
                        .AsEnumerable()
                        .ToList();
            
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Monthly Statement");
                worksheet.Cells["A1:D1"].Merge = true;
                // Add a title to the merged cells
                worksheet.Cells["A1"].Value = "Account Summary";
                worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.Font.Size = 14;

                worksheet.Cells["A2:D2"].Style.Font.Bold = true;
                worksheet.Cells["A2"].Value = "Account No";
                worksheet.Cells["B2"].Value = "Account Type";
                worksheet.Cells["C2"].Value = "Customer Name";
                worksheet.Cells["D2"].Value = "Opening Balance";

                worksheet.Cells[3,1].Value = accountSummary.AccountNo;
                worksheet.Cells[3,2].Value = accountSummary.AccountType;
                worksheet.Cells[3,3].Value = accountSummary.CustomerName;
                worksheet.Cells[3,4].Value = accountSummary.OpeningBalance;


                worksheet.Cells["A5:E5"].Merge = true;
                // Add a title to the merged cells
                worksheet.Cells["A5"].Value = "Monthly Transaction History";
                worksheet.Cells["A5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["A5"].Style.Font.Bold = true;
                worksheet.Cells["A5"].Style.Font.Size = 14;

                worksheet.Cells["A6:E6"].Style.Font.Bold = true;
                worksheet.Cells["A6"].Value = "Transaction Date";
                worksheet.Cells["B6"].Value = "Account No";
                worksheet.Cells["C6"].Value = "Type";
                worksheet.Cells["D6"].Value = "Remarks";
                worksheet.Cells["E6"].Value = "Amount";

                int row = 7;
                foreach (var transaction in transactions)
                {
                    worksheet.Cells[row, 1].Value = transaction.TransactionDate;
                    worksheet.Cells[row, 2].Value = accountSummary.AccountNo;
                    worksheet.Cells[row, 3].Value = transaction.TransactionType;
                    worksheet.Cells[row, 4].Value = transaction.Remarks;
                    worksheet.Cells[row, 5].Value = transaction.Amount;
                    row++;
                }

                int mergeRowStart = row+2; // Dynamic merge start row (2 rows above data start)
                //int mergeRowEnd = row - transactions.Count - 1;

                worksheet.Cells[$"A{mergeRowStart}:F{mergeRowStart}"].Merge = true;
                // Add a title to the merged cells
                worksheet.Cells[$"A{mergeRowStart}"].Value = "Loan History";
                worksheet.Cells[$"A{mergeRowStart}"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[$"A{mergeRowStart}"].Style.Font.Bold = true;
                worksheet.Cells[$"A{mergeRowStart}"].Style.Font.Size = 14;

                worksheet.Cells[$"A{mergeRowStart + 1}:F{mergeRowStart + 1}"].Style.Font.Bold = true;
                worksheet.Cells[$"A{mergeRowStart + 1}"].Value = "Account No";
                worksheet.Cells[$"B{mergeRowStart + 1}"].Value = "Loan Type";
                worksheet.Cells[$"C{mergeRowStart + 1}"].Value = "Interest Rate";
                worksheet.Cells[$"D{mergeRowStart + 1}"].Value = "Term";
                worksheet.Cells[$"E{mergeRowStart + 1}"].Value = "Loan Amount";
                worksheet.Cells[$"F{mergeRowStart + 1}"].Value = "Status";
                int row1 = mergeRowStart + 2;
                foreach (var loan in loanList)
                {
                    worksheet.Cells[row1, 1].Value = accountSummary.AccountNo;
                    worksheet.Cells[row1, 2].Value = loan.LoanType;
                    worksheet.Cells[row1, 3].Value = loan.InterestRate;
                    worksheet.Cells[row1, 4].Value = loan.Term;
                    worksheet.Cells[row1, 5].Value = loan.LoanAmount;
                    worksheet.Cells[row1, 6].Value = loan.LoanStatus;
                    row++;
                }

                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MonthlyStatement.xlsx");
            }
        }
    }
}
