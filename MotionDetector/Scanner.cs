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
using Newtonsoft.Json.Linq;

using Newtonsoft.Json;
using System.IO;

namespace MotionDetector
{
    struct Data
    {
        public double[] X;
        public double[] Y;
        public double[] Z;
    }

    public class Scanner
    {
        private static Scanner instance;
        CircularBuffer CircularBuffer = new CircularBuffer(2000);
        AtomicAction AtomicAction = AtomicAction.Instance;

        static string path = global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
        static string AppPath = System.IO.Path.Combine(path.ToString(), "results.txt");

        private Scanner() { }

        public static Scanner Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Scanner();
                }
                return instance;
            }
        }
        public void Execute()
        {
            int ProccessWindowSize = 200;
            Data ProcessWindow = new Data { X = new double[ProccessWindowSize], Y = new double[ProccessWindowSize], Z = new double[ProccessWindowSize] };
            string result;

            if (!CircularBuffer.IsReadOffsetBlocked())
            {
                if (CircularBuffer.IsReadyToProcess(ProccessWindowSize))
                {
                    var tempBuf = CircularBuffer.Read(ProccessWindowSize);

                    for (var j = 0; j < ProccessWindowSize; j++)
                    {
                        ProcessWindow.X[j] = tempBuf[j].X;
                        ProcessWindow.Y[j] = tempBuf[j].Y;
                        ProcessWindow.Z[j] = tempBuf[j].Z;
                    }

                    FilterSignal(ProcessWindow.X, ProcessWindow.X);
                    FilterSignal(ProcessWindow.Y, ProcessWindow.Y);
                    FilterSignal(ProcessWindow.Z, ProcessWindow.Z);


                    result = ProcessBuffer(ProcessWindow.Y, "X");

                    if(result == "Standing" && IsGravityOnAxis(ProcessWindow.Z))
                    {
                        result = "Sitting";
                    }

                    var atomicActionResult = "";
                    AtomicAction.Increment(result);
                    if(AtomicAction.Counter == 10)
                    {
                        atomicActionResult = AtomicAction.GetAtomicAction();
                        AtomicAction.Reset();

                        try
                        {
                            using (var streamWriter = new StreamWriter(AppPath, true))
                            {
                                streamWriter.WriteLine(atomicActionResult + " " + System.DateTime.Now.Second);
                            }
                        }
                        catch { }
                    }

                   

                    //ProcessBuffer(ProcessWindow.Y, "Y");
                    //Thread.Sleep(500);
                    CircularBuffer.IncrementReadOffset(ProccessWindowSize);
                }
            }
        }

        public void Write(double valueX, double valueY, double valueZ)
        {
            if (!CircularBuffer.IsWriteOffsetBlocked())
            {
                CircularBuffer.Write(valueX, valueY, valueZ);
                CircularBuffer.IncrementWriteOffset();
            }
        }

        public static double[] InitializeBuffer(double[] array, int size)
        {
            double[] buf = new double[size];

            for (var i = 0; i < size; i++)
            {
                buf[i] = array[i];
            }
            return buf;
        }

        public static void UpdateBuffer(double[] buffer, double nextValue)
        {
            for (var i = 0; i < buffer.Length - 1; i++)
            {
                buffer[i] = buffer[i + 1];
            }

            buffer[buffer.Length - 1] = nextValue;
        }

        public static double GetBufferThreshold(double[] buffer)
        {
            return (buffer.Max() + buffer.Min()) / 2;
        }

        public static bool ToLowTransition(double current, double next, double threshold)
        {
            if (current > threshold && next < threshold)
            {
                return true;
            }
            return false;
        }

        public static bool ToHighTransition(double current, double next, double threshold)
        {
            if (current < threshold && next > threshold)
            {
                return true;
            }
            return false;
        }

        public static string ProcessBuffer(double[] buffer, string direction)
        {
            var threshold = GetBufferThreshold(buffer);
            var tempArrayList = new List<double>();
            List<Step> BufferSteps = new List<Step>();
            AccAction fact = new AccAction();


            for (var i = 0; i < buffer.Length - 1; i++)
            {
                if (ToLowTransition(buffer[i], buffer[i + 1], threshold) && tempArrayList.Count > 1)
                {
                    tempArrayList.Add(buffer[i]);
                    var tempBuffer = tempArrayList.ToArray();

                    var tempThreshold = GetBufferThreshold(tempBuffer);
                    if (tempBuffer.Max() - threshold > 0.03)
                    {
                        BufferSteps.Add(new Step(1, tempBuffer));
                    }

                    tempArrayList.Clear();
                    tempArrayList.TrimExcess();

                }
                else if (ToHighTransition(buffer[i], buffer[i + 1], threshold) && tempArrayList.Count > 1)
                {
                    tempArrayList.Add(buffer[i]);
                    var tempBuffer = tempArrayList.ToArray();
                    var tempThreshold = GetBufferThreshold(tempBuffer);
                    if (threshold - tempBuffer.Min() > 0.03)
                    {
                        BufferSteps.Add(new Step(0, tempBuffer));
                    }

                    tempArrayList.Clear();
                    tempArrayList.TrimExcess();

                }
                else
                {
                    tempArrayList.Add(buffer[i]);
                }
            }

            var upStepCounter = 0;
            foreach (var step in BufferSteps)
            {
                if (step.Type == 1)
                {
                    upStepCounter++;
                }
            }



            if (IsStatic(BufferSteps))
            {
                fact.Static++;
                if (direction == "X")
                {
                    fact.Standing++;
                }
                else if (direction == "Y")
                {
                    fact.Sitting++;
                }
            }
            else
            {
                fact.Dynamic++;
                if (IsPeriodic(BufferSteps))
                {
                    fact.Periodic++;
                    switch (upStepCounter)
                    {
                        case 2:
                            fact.Walking++;
                            break;
                        case 3:
                            fact.Running++;
                            break;
                        default:
                            break;
                    }


                }
                else
                {
                    fact.Nonuniform++;
                }

            }

            var ret = fact.GetFact();
            System.Diagnostics.Debug.WriteLine(ret);

            return ret;

        }

        public static bool IsStatic(List<Step> list)
        {
            if (list.Count > 0)
            {
                return false;
            }
            return true;
        }

        public static bool IsPeriodic(List<Step> list)
        {
            if (list.Count > 2)
            {
                var lastWidth = list[0].ValuesArray.Length + list[1].ValuesArray.Length;

                for (var i = 0; i < list.Count - 2; i = i + 2)
                {
                    var currentWidth = list[i].ValuesArray.Length + list[i + 1].ValuesArray.Length;
                    //30 is a minimum difference between step signals length
                    if (Math.Abs(currentWidth - lastWidth) > 50)
                    {
                        return false;
                    }
                }

                return true;
            }
            return false;

        }

        public static void FilterSignal(double[] input, double[] output)
        {
            MedianFilter(input, output);
            MovingAverageFilter(input, output);
            MovingAverageFilter(input, output);
            MovingAverageFilter(input, output);
            MovingAverageFilter(input, output);
        }


        public static void MedianFilter(double[] input, double[] output)
        {
            double[] temp = new double[5];

            output[0] = input[0];

            for (int i = 1; i < input.Length - 3; i++)
            {
                temp[0] = input[i - 1];
                temp[1] = input[i];
                temp[2] = input[i + 1];
                temp[3] = input[i + 2];
                temp[4] = input[i + 3];
                Array.Sort(temp);
                output[i] = temp[1];
            }
        }

        public static void MovingAverageFilter(double[] input, double[] output)
        {
            output[0] = input[0];
            output[1] = (input[0] + input[1] + input[2]) / 3;
            output[2] = (input[0] + input[1] + input[2] + input[3] + input[4]) / 5;
            output[3] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6]) / 7;
            output[4] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6] + input[7] + input[8]) / 9;
            output[5] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6] + input[7] + input[8] + input[9] + input[10]) / 11;
            for (int i = 6; i < input.Length - 6; i++)
            {
                output[i] = (input[i - 6] + input[i - 5] + input[i - 4] + input[i - 3] + input[i - 2] + input[i - 1] + input[i] + input[i + 1] + input[i + 2] + input[i + 3] + input[i + 4] + input[i + 5] + input[i + 6]) / 13;
            }

        }

        public static bool IsGravityOnAxis(double[] input)
        {
            for(var i = 0; i<input.Length; i++)
            {
                if(input[i] < 0.9)
                {
                    return false;
                }
            }
            return true;
        }
    }
}