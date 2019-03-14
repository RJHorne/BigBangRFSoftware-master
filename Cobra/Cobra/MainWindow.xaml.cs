using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Impinj.OctaneSdk;
using System.Drawing;
using System.Timers;
using System.Windows.Threading;

namespace Cobra
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private ImpinjReader reader = new ImpinjReader();
        double red, green, blue;
        private static System.Timers.Timer aTimer;
        Boolean maxPowerRx;
        Boolean maxPowerTx;
        string ipAddress = "169.254.1.75";  //Default IP for the impinj reader


        public MainWindow()
        {
            InitializeComponent();

            //SetTimer();

           // sliderLabel.Content = (powerSlider.Value.ToString());
            //StartupInitialisation();

            


        } 




        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Button Pressed");
            Boolean run = false;
            if (run == false)
            {
                
                try
                {
                    reader.Connect("169.254.1.75");

                    // Get the default settings
                    // We'll use these as a starting point
                    // and then modify the settings we're 
                    // interested in.

                    Settings settings = reader.QueryDefaultSettings();

                    // Tell the reader to include the antenna number
                    // in all tag reports. Other fields can be added
                    // to the reports in the same way by setting the 
                    // appropriate Report.IncludeXXXXXXX property.
                    settings.Report.IncludeAntennaPortNumber = true;
                    settings.Report.IncludePeakRssi = true;
                    settings.Report.IncludeFastId = true;
                    settings.Report.IncludePhaseAngle = true;
                    settings.Report.IncludeChannel = true;
                    // The reader can be set into various modes in which reader
                    // dynamics are optimized for specific regions and environments.
                    // The following mode, AutoSetDenseReader, monitors RF noise and interference and then automatically
                    // and continuously optimizes the reader’s configuration
                    settings.ReaderMode = ReaderMode.AutoSetDenseReader;
                    settings.SearchMode = SearchMode.DualTargetBtoASelect;

                    settings.Session = 1;


                    // Enable antenna #1. Disable all others.
                    settings.Antennas.DisableAll();
                    settings.Antennas.GetAntenna(1).IsEnabled = true;

                    // Set the Transmit Power and 
                    // Receive Sensitivity to the maximum.
                    settings.Antennas.GetAntenna(1).MaxTxPower = true;
                    settings.Antennas.GetAntenna(1).MaxRxSensitivity = true;
                    bool isMaxTX = settings.Antennas.GetAntenna(1).MaxTxPower;
                    // You can also set them to specific values like this...
                    //settings.Antennas.GetAntenna(1).TxPowerInDbm = 20;
                    //settings.Antennas.GetAntenna(1).RxSensitivityInDbm = -70;

                    // Apply the newly modified settings.
                    reader.ApplySettings(settings);

                    // Assign the TagsReported event handler.
                    // This specifies which method to call
                    // when tags reports are available.
                    reader.TagsReported += OnTagsReported;

                    // Start reading.
                    reader.Start();
                    run = true;
                }
                catch
                {
                    Console.WriteLine("Reader cannot be detected");
                }
            }
            else
            {
                ButtonStart.Content = "Stop";
                reader.Disconnect();
                run = false;
            }
        }

        //private void rebootFirmware(reboot)
        //{
        //    if(reboot == true)
        //    reader.ApplyFactorySettings();
        //}


        //private void SetTimer()
        //{
        //    // Create a timer with a two second interval.
        //    aTimer = new System.Timers.Timer(20);
        //    // Hook up the Elapsed event for the timer. 
        //    aTimer.Elapsed += OnTimedEvent;
        //    aTimer.AutoReset = true;
        //    aTimer.Enabled = true;
        //}

        //private void OnTimedEvent(Object source, ElapsedEventArgs e)
        //{
        //    //Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
        //    //              e.SignalTime);
        //    //Console.WriteLine(hit);
        //    Action action = delegate ()
        //    {
        //        something(powerSlider.Value);
        //        RBG();
        //        sliderLabel.Content = (powerSlider.Value.ToString());
        //    };
        //    Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);

        //}
        private void OnTagsReported(ImpinjReader sender, TagReport report)
        {
            // This event handler gets called when a tag report is available.
            // Since it is executed in a different thread, we cannot operate
            // directly on UI elements (the Listbox) in this method.
            // We must execute another method (UpdateListbox) on the main thread
            // using BeginInvoke. We will pass updateListbox a List of tags.
            Action action = delegate ()
            {
              //  Console.WriteLine("Iam being called");
                Update(report.Tags);
               // Console.WriteLine(report.Tags.Count);
            };
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }

        private void Update(List<Tag> list)
        {
            foreach (var tag in list)
            {
                double value = tag.PeakRssiInDbm;
                double phase = tag.PhaseAngleInRadians;
                // value = ((-value) *2.5 ) -100;
                string epc = tag.Epc.ToHexString();
                string channel = tag.ChannelInMhz.ToString();

                //value2 = convertToRGB(value);
                red =  convertToR(value);
                green =  (255 - convertToG(value));
                blue = 0;
                Console.WriteLine(epc + " " + value + "," + green +  "," + red);
                int redint = CheckConstraints(Convert.ToInt32(red),0,255);
                int greenint = CheckConstraints(Convert.ToInt32(green),0,255);
                int blueint = CheckConstraints(Convert.ToInt32(blue), 0, 255);
                cLabel.Content = channel;
                update(System.Convert.ToByte(redint), System.Convert.ToByte(greenint), System.Convert.ToByte(blueint));
                //_0001.Fill = new SolidColorBrush(Color.FromRgb(System.Convert.ToByte(CheckConstraints(redint,0,255)), System.Convert.ToByte(CheckConstraints(greenint, 0, 255)), System.Convert.ToByte(blue)));
            }
        }

        void update(byte r,byte g, byte b)
        {
            _0001.Fill = new SolidColorBrush(Color.FromRgb(r, g, b));
        }

        static double convertToR(double input)
        {

            input = -input;
            input = input - 40;
            input = input * 6;

            return input;
        }

        static double convertToG(double input)
        {
            input = -input;
            input = input - 15;
            input = input * 6;

            return input;
        }


        static int  CheckConstraints(int r, int min, int max)
        {
            if (r > max)
            {
                r = max;
            }
            if (r < min)
            {
                r = min;
            }

            return r;
        }
    }
}
