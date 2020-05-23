using SI642_BonoCorporativo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SI642_BonoCorporativo.Controllers
{
    public class AccountController : Controller
    {
        private SI642_Entities db = new SI642_Entities();
        public static string Static_DNI { get; set; }
        // GET: Account
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult ProfileUser()
        {
            User user = db.User.Where(s => s.DNI == Static_DNI).FirstOrDefault<User>();
            return RedirectToAction("Details", "Users", new { id = user.Id });
        }

        [HttpPost]
        public ActionResult Login(UserLogin userLogin)
        {

            bool isValid = db.User.Any(x => x.DNI == userLogin.DNI && x.Password == userLogin.Password);

            if (isValid)
            {
                User user = db.User.Where(x => x.DNI == userLogin.DNI).FirstOrDefault<User>();
                FormsAuthentication.SetAuthCookie(user.Name +" "+ user.FatherLastName + " " + user.MotherLastName, false);
                Static_DNI = userLogin.DNI;
                return RedirectToAction("Index","Home");
            }
            ModelState.AddModelError("", "Usuario y/o Contraseña Invalido");
            return View();
        }


        public ActionResult SignUp()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp([Bind(Include = "Id,Name,FatherLastName,MotherLastName,DNI,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                db.User.Add(user);
                db.SaveChanges();
                return RedirectToAction("Login");
            }

            return View(user);
        }


        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

    }
}