using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;






namespace FINANZAS
{
    public class Datos
    {
        

        double LOW_RATE = 0.001;
        double HIGH_RATE = 0.5;
        double MAX_ITERATION = 1000;
        double PRECISION_REQ = 0.00000001;

        public double VN { get; set; }
        public double VC { get; set; }
        public int NroAños { get; set; }
        public int FrecuenciaCupon { get; set; }
        //public int impRenta { get; set; }

        public string frecuenciaPago { get; set; }
        public string tipoTasa { get; set; }
        public string capitalizacion { get; set; }

        public double TasaInteres { get; set; }
        public double TasaAnualDcto { get; set; }


        public int diasDeCapitalizacion = 30;// pondremos capitalozacion mensual 30 dias
                                             //Definimos la tasa anual de descuento COK = 4.5%

        public Datos(double _cavali, double _flotacion, double _colocacion, double _estructuracion, double _prima, double _cok)
        {
            //Datos datos = new Datos();
            //datos.FrecuenciaCupon
            this.cavali = _cavali/100.0;
            this.Flotacion = _flotacion/100.0;
            this.Colocacion = _colocacion/100.0;
            this.Estructuracion = _estructuracion/100.0;
            this.Prima = _prima/100.0;
            this.COK = _cok/100.0;

        }

        public double COK;
        //public double Prima = 0.01;
        //public double Estructuracion = 0.005;
        //public double Colocacion = 0.005;
        //public double Flotacion = 0.0045;
        //public double cavali = 0.005;

        public double Prima;
        public double Estructuracion;
        public double Colocacion;
        public double Flotacion;
        public double cavali;

        public double impRenta = 0.3;


        public double c { get; set; }

        public double b { get; set; }


        


        public int CalculoFrecuenciaCupon(string frecuencia)
        {

            if (frecuencia == "Mensual") { FrecuenciaCupon = 30; }
            if (frecuencia == "Bimestral") { FrecuenciaCupon = 60; }
            if (frecuencia == "Trimestral") { FrecuenciaCupon = 90; }
            if (frecuencia == "Cuatrimestral") { FrecuenciaCupon = 120; }
            if (frecuencia == "Semestral") { FrecuenciaCupon = 180; }
            if (frecuencia == "Anual") { FrecuenciaCupon = 360; }

            return FrecuenciaCupon;
        }

        public int PeriodosXaño()
        {
            return 360 / FrecuenciaCupon;
        }

        public int nTotPerXaño()
        {
            return NroAños * PeriodosXaño();
        }

        public double TEA()
        {
            double ValorTEA;

            if (tipoTasa == "Nominal")
            {
                ValorTEA = (Math.Pow((1 + (TasaInteres / (12 * 100))), 12) - 1) * 100;
            }
            else ValorTEA = TasaInteres;

            return ValorTEA;
            //return 0.5;
        }

        public double VAC(double[] cf, int numOfFlows, double cok, int x)
        {
            double denom = 0.0;
            double npv = 0.0;

            for (int j = x; j < numOfFlows; j++)
            {
                denom = Math.Pow((1 + cok), j);
                npv = npv + (cf[j] / denom);
            }

            return npv;
        }
        public double TEPeriodo()
        {


            if (FrecuenciaCupon == 30) { c = 0.08333333333; b = Math.Pow(1 + (TEA() / 100), c) - 1; }
            if (FrecuenciaCupon == 60) { c = 0.1666666667; b = Math.Pow(1 + (TEA() / 100), c) - 1; }
            if (FrecuenciaCupon == 90) { c = 0.25; b = Math.Pow(1 + (TEA() / 100), c) - 1; }
            if (FrecuenciaCupon == 120) { c = 0.3333333333; b = Math.Pow(1 + (TEA() / 100), c) - 1; }
            if (FrecuenciaCupon == 180) { c = 0.5; b = Math.Pow(1 + (TEA() / 100), c) - 1; }
            if (FrecuenciaCupon == 360) { c = 1; b = Math.Pow(1 + (TEA() / 100), c) - 1; }

            return b;

        }

        public double COKperiodo()
        {
            return Math.Pow(1 + COK, c) - 1;
        }

        public double CostosInicialesEmisor()
        {
            return VC * (Estructuracion + Colocacion + Flotacion + cavali);
        }

        public double CostInicBon()
        {
            return VC * (Flotacion + cavali);
        }


        public double TIR(double[] cf, int numOfFlows)
        {
            int i = 0, j = 0;
            double m = 0.0;
            double old = 0.00;
            double new1 = 0.00;
            double oldguessRate = LOW_RATE;
            double newguessRate = LOW_RATE;
            double guessRate = LOW_RATE;
            double lowGuessRate = LOW_RATE;
            double highGuessRate = HIGH_RATE;
            double npv = 0.0;
            double denom = 0.0;
            for (i = 0; i < MAX_ITERATION; i++)
            {
                npv = 0.00;
                for (j = 0; j < numOfFlows; j++)
                {
                    denom = Math.Pow((1 + guessRate), j);
                    npv = npv + (cf[j] / denom);
                }
                /* Stop checking once the required precision is achieved */
                if ((npv > 0) && (npv < PRECISION_REQ))
                    break;
                if (old == 0)
                    old = npv;
                else
                    old = new1;
                new1 = npv;
                if (i > 0)
                {
                    if (old < new1)
                    {
                        if (old < 0 && new1 < 0)
                            highGuessRate = newguessRate;
                        else
                            lowGuessRate = newguessRate;
                    }
                    else
                    {
                        if (old > 0 && new1 > 0)
                            lowGuessRate = newguessRate;
                        else
                            highGuessRate = newguessRate;
                    }
                }
                oldguessRate = guessRate;
                guessRate = (lowGuessRate + highGuessRate) / 2;
                newguessRate = guessRate;
            }
            return guessRate;
        }





    }




}

