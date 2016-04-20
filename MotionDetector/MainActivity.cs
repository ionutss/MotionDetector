using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Hardware;
using System.IO;
using Newtonsoft.Json;

namespace MotionDetector
{
    [Activity(Label = "MotionDetector", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, ISensorEventListener
    {
        static readonly object _syncLock = new object();
        SensorManager _sensorManager;
        TextView _sensorTextView;

        public FileStream fs = null;
        static string path = global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
        string AppPath = System.IO.Path.Combine(path.ToString(), "awala3.txt");
        StreamWriter sw = null;
        JsonWriter writer = null;
        Scanner scanner = Scanner.Instance;


        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            //as
        }

        public void OnSensorChanged(SensorEvent e)
        {
            //Console.WriteLine(e.Values[0]);
            lock (_syncLock)
            {
                _sensorTextView.Text = string.Format("x={0:f}, y={1:f}, z={2:f}", e.Values[0] / 10, e.Values[1] / 10, e.Values[2] / 10);
                //writer.WriteValue(e.Values[0] / 10);

                scanner.Write(e.Values[0] / 10, e.Values[1] / 10, e.Values[2] / 10);
                scanner.Execute();
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            _sensorManager = (SensorManager)GetSystemService(Context.SensorService);
            _sensorTextView = FindViewById<TextView>(Resource.Id.accelerometer_text);

            fs = File.Create(AppPath);
            sw = new StreamWriter(fs);
            sw.Write("IROD");
            sw.Close();
            //writer = new JsonTextWriter(sw);
            //writer.WriteStartObject();
            //writer.WritePropertyName("acc_x");
            //writer.WriteStartArray();

            scanner.Execute();

        }

        protected override void OnResume()
        {
            base.OnResume();
            _sensorManager.RegisterListener(this,
                                            _sensorManager.GetDefaultSensor(SensorType.Accelerometer),
                                            SensorDelay.Fastest);

            Console.WriteLine("Resume");
        }

        protected override void OnPause()
        {
            base.OnPause();
            //_sensorManager.UnregisterListener(this);
            Console.WriteLine("Pause");

        }

        protected override void OnStop()
        {
            base.OnStop();
            //writer.WriteEndArray();
            //writer.WriteEndObject();

            Console.WriteLine(" Stop");
        }
    }
}

