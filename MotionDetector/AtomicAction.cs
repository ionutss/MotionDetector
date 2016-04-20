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
    struct atomic
    {
        public string name;
        public int value;

        public atomic(string name, int value)
        {
            this.name = name;
            this.value = value;
        }
    }
    class AtomicAction
    {
        private static AtomicAction instance;

        public atomic Walking;
        public atomic Running;
        public atomic Nonuniform;
        public atomic Standing;
        public atomic Sitting;
        public atomic Unknown;

        public int Counter;

        private AtomicAction()
        {
            Walking = new atomic("Walking", 0);
            Running = new atomic("Running", 0);
            Nonuniform = new atomic("Nonuniform", 0);
            Standing = new atomic("Standing", 0);
            Sitting = new atomic("Sitting", 0);
            Unknown = new atomic("Unknown", 0);

            Counter = 0;
        }

        public static AtomicAction Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new AtomicAction();
                }
                return instance;
            }
        }

        public string GetAtomicAction()
        {
            List<atomic> list = new List<atomic>();
            list.Add(Walking);
            list.Add(Running);
            list.Add(Nonuniform);
            list.Add(Standing);
            list.Add(Sitting);
            list.Add(Unknown);

            var ret = new atomic("", 0);
            foreach(var a in list)
            {
                if(a.value > ret.value)
                {
                    ret = a;
                }
            }


            return ret.name;
        }

        public void Reset()
        {
            Walking.value = 0;
            Running.value = 0;
            Nonuniform.value = 0;
            Standing.value = 0;
            Sitting.value = 0;
            Unknown.value = 0;

            Counter = 0;
        }

        public void Increment(string atomicAction)
        {
            Counter++;
            switch (atomicAction)
            {
                case "Walking":
                    Walking.value++;
                    break;
                case "Running":
                    Running.value++;
                    break;
                case "Nonuniform":
                    Nonuniform.value++;
                    break;
                case "Standing":
                    Standing.value++;
                    break;
                case "Sitting":
                    Sitting.value++;
                    break;
                case "Unknown":
                    Unknown.value++;
                    break;
            }
        }
    }
}