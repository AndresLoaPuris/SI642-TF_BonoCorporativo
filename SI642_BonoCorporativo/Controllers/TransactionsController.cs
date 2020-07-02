using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FINANZAS;
using Rotativa;
using SI642_BonoCorporativo.Models;

namespace SI642_BonoCorporativo.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private SI642Entities db = new SI642Entities();

        private static double static_cok;
        private static double static_prima;
        private static double static_cavali;
        private static double static_estructuracion;
        private static double static_flotacion;
        private static double static_colocacion;


        public ActionResult Registered_Operations()
        {
            User user = db.User.Where(s => s.DNI == AccountController.Static_DNI).FirstOrDefault<User>();
            var transaction = db.Transaction.Include(t => t.CoinType).Include(t => t.Method).Include(t => t.PaymentFrequency).Include(t => t.RateType).Include(t => t.User).Where(s => s.User_Id == user.Id);

            return View(transaction.ToList());
        }


        private bool CheckDateRange(DateTime date)
        {
            DateTime dateNow = DateTime.Now.AddDays(-2);
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
            Datos datos = new Datos(static_cavali,static_flotacion,static_colocacion,static_estructuracion,static_prima,static_cok);

            ViewBag.Error = "TIR NEGATIVO";
            
            if (transaction.CoinType.Name == "$ USD")
            {
                datos.VN = Convert.ToDouble(transaction.FaceValue) * 0.28;
                datos.VC = Convert.ToDouble(transaction.CommercialValue) * 0.28;
            }
            else if (transaction.CoinType.Name == "€ EUR")
            {
                datos.VN = Convert.ToDouble(transaction.FaceValue) * 0.25;
                datos.VC = Convert.ToDouble(transaction.CommercialValue) * 0.25;
            }
            else {
                datos.VN = Convert.ToDouble(transaction.FaceValue);
                datos.VC = Convert.ToDouble(transaction.CommercialValue);
            }
                datos.NroAños = transaction.Years;
            datos.frecuenciaPago = transaction.PaymentFrequency.Name;
            datos.tipoTasa = transaction.RateType.Name;
            datos.TasaInteres = Convert.ToDouble(transaction.InterestRate);

            int frecCupon = datos.CalculoFrecuenciaCupon(transaction.PaymentFrequency.Name);
            int perPorAño = datos.PeriodosXaño();
            int totalPerXaño = datos.nTotPerXaño();
            double TEA = datos.TEA(); //Lo pongo en porcentaje
            double TasaEfecPerio = datos.TEPeriodo() * 100;//Lo pongo en porcentaje
            double COKdelPeriodo = datos.COKperiodo() * 100;
            double CostosInicialesDelEmisor = datos.CostosInicialesEmisor();
            double CostosInicBon = datos.CostInicBon();

            if (transaction.Method.Name == "Americano")  //tbxMetodos.Text == "Americano"
            {
                int aux = datos.nTotPerXaño() + 1;
                double[] Flujo_bonista = new double[aux];
                double[] Flujo_emisor = new double[aux];
                double[] Flujo_emisorEscudo = new double[aux];

                for (int i = 0; i < aux; i++)
                {
                    if (i == 0)
                    {
                        Flujo_bonista[i] = -datos.VC - datos.CostInicBon();
                        Flujo_emisor[i] = -datos.VC + datos.CostosInicialesEmisor();
                        Flujo_emisorEscudo[i] = -datos.VC + datos.CostosInicialesEmisor();
                    }


                    else if (i == aux - 1)
                    {
                        Flujo_bonista[i] = datos.VN + Flujo_bonista[i - 1] + datos.VN * datos.Prima;
                        Flujo_emisor[i] = datos.VN + Flujo_emisor[i - 1] + datos.VN * datos.Prima;
                        Flujo_emisorEscudo[i] = Flujo_emisor[i] - (datos.impRenta * Flujo_emisor[i - 1]);
                    }
                    else
                    {
                        Flujo_bonista[i] = datos.VN * datos.TEPeriodo();
                        Flujo_emisor[i] = datos.VN * datos.TEPeriodo();
                        Flujo_emisorEscudo[i] = datos.VN * datos.TEPeriodo() - datos.impRenta * Flujo_emisor[i];

                    }
                }
                double tir, tir2, tir3, Price, Utility;

                tir = datos.TIR(Flujo_bonista, aux);
                tir2 = datos.TIR(Flujo_emisor, aux);
                tir3 = datos.TIR(Flujo_emisorEscudo, aux);

                Price = datos.VAC(Flujo_bonista, aux, datos.COKperiodo(), 1);
                Utility = datos.VAC(Flujo_bonista, aux, datos.COKperiodo(), 0);


                double Trea_Bonista;
                double Tcea_Emisor;
                double Tcea_EmisorEscudo;

                if (tir != 0.001)
                {
                    Trea_Bonista = 100 * (Math.Pow((tir + 1), 360 / datos.FrecuenciaCupon) - 1);
                }
                else
                {
                    Trea_Bonista = 0;
                }
                if (tir2 != 0.001)
                {
                    Tcea_Emisor = 100 * (Math.Pow((tir2 + 1), 360 / datos.FrecuenciaCupon) - 1);
                }
                else
                {
                    Tcea_Emisor = 0;
                }
                if (tir3 != 0.001)
                {
                    Tcea_EmisorEscudo = 100 * (Math.Pow((tir3 + 1), 360 / datos.FrecuenciaCupon) - 1);
                }
                else
                {
                    Tcea_EmisorEscudo = 0;
                }



                //lblescud.Text = "El TCEA/c Escudo de emisor es: " + Tcea_EmisorEscudo.ToString();
                //lblPrecioActual.Text = "El Precio Actual es: " + Price.ToString();
                //lblUtPer.Text = "El Precio Actual es: " + Utility.ToString();
                ViewBag.TceaEscudo = Convert.ToDecimal(Tcea_EmisorEscudo.ToString());
                ViewBag.precio = Convert.ToDecimal(Price.ToString());
                ViewBag.utilidad = Convert.ToDecimal(Utility.ToString());
                transaction.TCEAIssuer = Convert.ToDecimal(Tcea_Emisor.ToString());
                 transaction.TREAInvestor = Convert.ToDecimal(Trea_Bonista.ToString());
            }

            if (transaction.Method.Name == "Aleman")
            {
                int aux = datos.nTotPerXaño() + 1;

                double[] Flujo_bonista = new double[aux];
                double[] Flujo_emisor = new double[aux];
                double[] Interes = new double[aux];
                double[] Amort = new double[aux];
                double[] Bono = new double[aux];
                double[] Flujo_emisorEscudo = new double[aux];

                double P = 0;
                for (int i = 0; i < aux; i++)
                {
                    if (i == 0)
                    {
                        Flujo_bonista[i] = -datos.VC - datos.CostInicBon();
                        Flujo_emisor[i] = -datos.VC + datos.CostosInicialesEmisor();
                        Flujo_emisorEscudo[i] = -datos.VC + datos.CostosInicialesEmisor();


                        Interes[i] = 0.00;
                        Amort[i] = 0.00;
                        Bono[i] = datos.VN;

                    }

                    else if (i == aux - 1)
                    {
                        P = datos.VN * datos.Prima;

                        Amort[i] = datos.VN / datos.nTotPerXaño();
                        Interes[i] = Bono[i - 1] * datos.TEPeriodo();
                        Bono[i] = Bono[i - 1] - Amort[i];

                        Flujo_bonista[i] = Interes[i] + Amort[i] + P;
                        Flujo_emisor[i] = Interes[i] + Amort[i] + P;
                        Flujo_emisorEscudo[i] = Flujo_emisor[i] - (datos.impRenta * Interes[i]);

                    }

                    else
                    {
                        Amort[i] = datos.VN / datos.nTotPerXaño();
                        Interes[i] = Bono[i - 1] * datos.TEPeriodo();
                        Bono[i] = Bono[i - 1] - Amort[i];

                        Flujo_bonista[i] = Interes[i] + Amort[i];
                        Flujo_emisor[i] = Interes[i] + Amort[i];

                        Flujo_emisorEscudo[i] = Flujo_emisor[i] - (datos.impRenta * Interes[i]);

                    }




                }

                double tir, tir2, tir3, Price, Utility;
                tir = datos.TIR(Flujo_bonista, aux);
                tir2 = datos.TIR(Flujo_emisor, aux);
                tir3 = datos.TIR(Flujo_emisorEscudo, aux);



                double Trea_Bonista;
                double Tcea_Emisor;
                double Tcea_EmisorEscudo;

                if (tir != 0.001)
                {
                    Trea_Bonista = 100 * (Math.Pow((tir + 1), 360 / datos.FrecuenciaCupon) - 1);
                }
                else
                {
                    Trea_Bonista = 0;
                }
                if (tir2 != 0.001)
                {
                    Tcea_Emisor = 100 * (Math.Pow((tir2 + 1), 360 / datos.FrecuenciaCupon) - 1);
                }
                else
                {
                    Tcea_Emisor = 0;
                }
                if (tir3 != 0.001)
                {
                    Tcea_EmisorEscudo = 100 * (Math.Pow((tir3 + 1), 360 / datos.FrecuenciaCupon) - 1);
                }
                else
                {
                    Tcea_EmisorEscudo = 0;
                }

                Price = datos.VAC(Flujo_bonista, aux, datos.COKperiodo(), 1);
                Utility = datos.VAC(Flujo_bonista, aux, datos.COKperiodo(), 0);



               
                //lbltcea.Text = "El TCEA/c Escudo de emisor es: " + Tcea_EmisorEscudo.ToString();
                //lblPrecioActual.Text = "El Precio Actual es: " + Price.ToString();
                //lblUtPer.Text = "El Precio Actual es: " + Utility.ToString();
                ViewBag.TceaEscudo = Convert.ToDecimal(Tcea_EmisorEscudo.ToString());
                ViewBag.precio = Convert.ToDecimal(Price.ToString());
                ViewBag.utilidad = Convert.ToDecimal(Utility.ToString());
                transaction.TCEAIssuer = Convert.ToDecimal(Tcea_Emisor.ToString());
                transaction.TREAInvestor = Convert.ToDecimal(Trea_Bonista.ToString());
            }

            if (transaction.Method.Name == "Frances")
            {

                int aux = datos.nTotPerXaño() + 1;

                double[] Flujo_bonista = new double[aux];
                double[] Flujo_emisor = new double[aux];
                double[] Flujo_emisorEscudo = new double[aux];


                double[] Interes = new double[aux];
                double[] Amort = new double[aux];
                double[] Bono = new double[aux];
                double P = 0;

                for (int i = 0; i < aux; i++)
                {
                    if (i == 0)
                    {
                        Flujo_bonista[i] = -datos.VC - datos.CostInicBon();
                        Flujo_emisor[i] = -datos.VC + datos.CostosInicialesEmisor();
                        Flujo_emisorEscudo[i] = -datos.VC + datos.CostosInicialesEmisor();


                        Interes[i] = 0;
                        Amort[i] = 0;
                        Bono[i] = datos.VN;

                    }

                    else if (i == aux - 1)
                    {
                        P = datos.VN * datos.Prima;
                        Flujo_bonista[i] = ((datos.VN * datos.TEPeriodo()) / (1 - Math.Pow(1 + datos.TEPeriodo(), -datos.nTotPerXaño()))) + P;


                        Interes[i] = Bono[i - 1] * datos.TEPeriodo();
                        Amort[i] = Flujo_bonista[i] - Interes[i];
                        Bono[i] = Bono[i - 1] - Amort[i];


                        Flujo_emisor[i] = Flujo_bonista[i];
                        Flujo_emisorEscudo[i] = Flujo_emisor[i] - (datos.impRenta * Interes[i]);

                    }

                    else
                    {
                        Flujo_bonista[i] = (datos.VN * datos.TEPeriodo()) / (1 - Math.Pow(1 + datos.TEPeriodo(), -datos.nTotPerXaño()));
                        Flujo_emisor[i] = (datos.VN * datos.TEPeriodo()) / (1 - Math.Pow(1 + datos.TEPeriodo(), -datos.nTotPerXaño()));

                        Interes[i] = Bono[i - 1] * datos.TEPeriodo();
                        Amort[i] = Flujo_bonista[i] - Interes[i];
                        Bono[i] = Bono[i - 1] - Amort[i];

                        Flujo_emisorEscudo[i] = ((datos.VN * datos.TEPeriodo()) / (1 - Math.Pow(1 + datos.TEPeriodo(), -datos.nTotPerXaño()))) - (datos.impRenta * Interes[i]);


                    }




                }

                double tir, tir2, tir3, Price, Utility;
                tir = datos.TIR(Flujo_bonista, aux);
                tir2 = datos.TIR(Flujo_emisor, aux);
                tir3 = datos.TIR(Flujo_emisorEscudo, aux);

                double Trea_Bonista;
                double Tcea_Emisor;
                double Tcea_EmisorEscudo;

                if (tir != 0.001)
                {
                    Trea_Bonista = 100 * (Math.Pow((tir + 1), 360 / datos.FrecuenciaCupon) - 1);
                }
                else {
                    Trea_Bonista = 0;
                }
                if (tir2 != 0.001) {
                    Tcea_Emisor = 100 * (Math.Pow((tir2 + 1), 360 / datos.FrecuenciaCupon) - 1);
                }
                else {
                    Tcea_Emisor = 0;
                }
                if (tir3 != 0.001)
                {
                    Tcea_EmisorEscudo = 100 * (Math.Pow((tir3 + 1), 360 / datos.FrecuenciaCupon) - 1);
                }
                else {
                    Tcea_EmisorEscudo = 0;
                }
                
                Price = datos.VAC(Flujo_bonista, aux, datos.COKperiodo(), 1);
                Utility = datos.VAC(Flujo_bonista, aux, datos.COKperiodo(), 0);

                ViewBag.precio = Convert.ToDecimal(Price.ToString());
                ViewBag.utilidad = Convert.ToDecimal(Utility.ToString());

                ViewBag.TceaEscudo = Convert.ToDecimal(Tcea_EmisorEscudo.ToString());
                transaction.TCEAIssuer = Convert.ToDecimal(Tcea_Emisor.ToString());
                transaction.TREAInvestor = Convert.ToDecimal(Trea_Bonista.ToString());
            }

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
            TempData["prima"] = "Test data";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Bono_Corporative([Bind(Include = "Id,FaceValue,CommercialValue,CoinType_Id,DateIssue,Years,PaymentFrequency_Id,Method_Id,RateType_Id,InterestRate,TCEAIssuer,TREAInvestor,User_Id,cavali,prima,colocacion,flotacion,estructuracion,cok")] Transaction transaction)
        {

            static_prima = transaction.prima;
            static_flotacion = transaction.flotacion;
            static_colocacion = transaction.colocacion;
            static_estructuracion = transaction.estructuracion;
            static_cavali = transaction.cavali;
            static_cok = transaction.cok;

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
