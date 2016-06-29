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
using System.Net;

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

        Button button1, button2, button3, button4, button5;


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

            button1 = FindViewById<Button>(Resource.Id.button1);
            button2 = FindViewById<Button>(Resource.Id.button2);
            button3 = FindViewById<Button>(Resource.Id.button3);
            button4 = FindViewById<Button>(Resource.Id.button4);
            button5 = FindViewById<Button>(Resource.Id.button5);
            

            button1.Click += (object sender, EventArgs e) =>
            {
                RequestPostAsync("3");
            };

            button2.Click += (object sender, EventArgs e) =>
            {
                RequestPostAsync("4");
            };

            button3.Click += (object sender, EventArgs e) =>
            {
                RequestPostAsync("2");
            };

            button4.Click += (object sender, EventArgs e) =>
            {
                RequestPostAsync("1");
            };

            button5.Click += (object sender, EventArgs e) =>
            {
                RequestPostAsync("Outside");
            };


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

       
        

        public static async void RequestPostAsync(string BoardSensor)
        {
            var ResultToSend = new Result(BoardSensor, DateTime.Now);
            var payload = JsonConvert.SerializeObject(ResultToSend, Formatting.Indented);

            var request = System.Net.HttpWebRequest.Create(string.Format(@"http://192.168.0.101:3591/api/board"));
            request.ContentType = "application/json";
            request.Method = "POST";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                var definition = new { type = "room", room = BoardSensor, date = DateTime.Now };
                var json1 = JsonConvert.SerializeObject(definition, Formatting.Indented);

                streamWriter.Write(json1);

            }
            var content = new MemoryStream();

            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    await responseStream.CopyToAsync(content);
                }
            }
            Console.WriteLine(content.ToArray());

        }
    }
}

