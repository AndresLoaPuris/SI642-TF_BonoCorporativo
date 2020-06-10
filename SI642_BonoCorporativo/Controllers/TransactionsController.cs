using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SI642_BonoCorporativo.Models;

namespace SI642_BonoCorporativo.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private SI642Entities db = new SI642Entities();

        
        public ActionResult Registered_Operations()
        {
            User user = db.User.Where(s => s.DNI == AccountController.Static_DNI).FirstOrDefault<User>();
            var transaction = db.Transaction.Include(t => t.CoinType).Include(t => t.Method).Include(t => t.PaymentFrequency).Include(t => t.RateType).Include(t => t.User).Where(s => s.User_Id == user.Id);

            return View(transaction.ToList());
        }

        private bool CheckDateRange(DateTime date)
        {
            DateTime dateNow = DateTime.Now.AddDays(-1);
            DateTime dateFuture = DateTime.Now.AddYears(3);


            if (dateNow <= date && date <= dateFuture)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public ActionResult Bono_Algorithm(int? id)
        {
            Transaction transaction = db.Transaction.Find(id);

            // TODO: Algoritmo Calcular TREA , TCEA

            // ...
            // ...

            // transaction.TCEAIssuer =
            // transaction.TREAInvestor =

            if (ModelState.IsValid)
            {
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();
            }

            return View(transaction);
        }

        
        public ActionResult Bono_Corporative()
        {
            ViewBag.CoinType_Id = new SelectList(db.CoinType, "Id", "Name");
            ViewBag.Method_Id = new SelectList(db.Method, "Id", "Name");
            ViewBag.PaymentFrequency_Id = new SelectList(db.PaymentFrequency, "Id", "Name");
            ViewBag.RateType_Id = new SelectList(db.RateType, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Bono_Corporative([Bind(Include = "Id,FaceValue,CommercialValue,CoinType_Id,DateIssue,Years,PaymentFrequency_Id,Method_Id,RateType_Id,InterestRate,TCEAIssuer,TREAInvestor,User_Id")] Transaction transaction)
        {
            User user = db.User.Where(s => s.DNI == AccountController.Static_DNI).FirstOrDefault<User>();

            transaction.User_Id = user.Id;
            transaction.TCEAIssuer = 0;
            transaction.TREAInvestor = 0;

            if (ModelState.IsValid && CheckDateRange(transaction.DateIssue))
            {
                db.Transaction.Add(transaction);
                db.SaveChanges();
                int last_Transaction = db.Transaction.Max(p => p.Id);
                return RedirectToAction("Bono_Algorithm", "Transactions", new { id = last_Transaction });
            }

            ViewBag.CoinType_Id = new SelectList(db.CoinType, "Id", "Name", transaction.CoinType_Id);
            ViewBag.Method_Id = new SelectList(db.Method, "Id", "Name", transaction.Method_Id);
            ViewBag.PaymentFrequency_Id = new SelectList(db.PaymentFrequency, "Id", "Name", transaction.PaymentFrequency_Id);
            ViewBag.RateType_Id = new SelectList(db.RateType, "Id", "Name", transaction.RateType_Id);
            return View(transaction);
        }

        public ActionResult Details_Operation(int? id)
        {
            return View();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
