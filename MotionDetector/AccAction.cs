using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MotionDetector
{
    public class AccAction
    {
        public double Dynamic;
        public double Static;
        public double Periodic;
        public double Nonuniform;
        public double Walking;
        public double Running;
        public double Sitting;
        public double Standing;

        public AccAction()
        {
            Dynamic = 0;
            Static = 0;
            Periodic = 0;
            Nonuniform = 0;
            Walking = 0;
            Running = 0;
            Sitting = 0;
            Standing = 0;
        }

        public double total(double x, double y)
        {
            return x + y;
        }

        public string GetFact()
        {
            if ((Dynamic / total(Dynamic, Static)) > (Static / total(Dynamic, Static)))
            {
                double periodicR = (Periodic / total(Periodic, Nonuniform));
                double nonperiodicR = (Nonuniform / total(Periodic, Nonuniform));
                if (periodicR > nonperiodicR && periodicR > 0.5)
                {
                    double walkingR = (Walking / total(Walking, Running));
                    double runningR = (Running / total(Walking, Running));


                    if (walkingR > runningR && walkingR > 0.5)
                    {
                        return "Walking";
                    }
                    else if (runningR > 0.5)
                    {
                        return "Running";
                    }
                }
                else if (nonperiodicR > 0.5)
                {
                    return "Nonuniform"; //should give a name
                }
            }
            else
            {
                if ((Standing / total(Standing, Sitting)) > (Sitting / (total(Standing, Sitting))))
                {
                    return "Standing";
                }
                else
                {
                    return "Sitting";
                }
            }

            return "Unknown";
        }
    }
}