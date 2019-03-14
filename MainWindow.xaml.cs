using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Impinj.OctaneSdk;
using System.Drawing;
using System.Timers;
using System.Windows.Media;
using System.Windows.Controls;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        private ImpinjReader reader = new ImpinjReader();
        private static System.Timers.Timer aTimer;
        private static System.Timers.Timer bTimer;
        public byte gb;
        public byte rb;
        int r;
        int g;
        public bool hit = true;
        public bool yellow = false;
        public bool start = true;
        string ipAddress = "169.254.1.1";  //Default IP for the impinj reader 

        public MainWindow()
        {
            InitializeComponent();
            SetTimer();
            epcLabel.Content = " ";
        }

        private void Initialise()
        {

            Settings settings = reader.QueryDefaultSettings();
            settings.Report.IncludeAntennaPortNumber = true;
            settings.Report.IncludePeakRssi = true;
            // settings.Antennas.TxPowerMax = true;           
            int TxPowerInDbm = boxValue();
            settings.Antennas.TxPowerInDbm = TxPowerInDbm;
            //settings.Antennas.RxSensitivityInDbm = 3;
            settings.Antennas.RxSensitivityMax = true;
            //settings.Antennas.TxPowerInDbm = 15;
            // Send a tag report for every tag read.
            settings.Report.Mode = ReportMode.Individual;
            // Apply the newly modified settings.
            reader.ApplySettings(settings);
            reader.TagsReported += OnTagsReported;

        }

        public int boxValue()
        {
            ListBoxItem lbi = lbxItems.SelectedItem as ListBoxItem;
            String selItem = lbi.Content.ToString();
            int dbValue = Convert.ToInt32(selItem);

            return dbValue;
        }

        private void UpdateListbox(List<Tag> list)
        {
            // Console.WriteLine(list.Count);

            // Loop through each tag is the list and add it to the Listbox.
            foreach (var tag in list)
            {
                // listTags.Items.Add(tag.Epc + ", " + tag.AntennaPortNumber + ", " + tag.PeakRssiInDbm + ", " + tag.Tid);
                String EPCCap = tag.Epc.ToString();
                //if (EPCCap.StartsWith("E280 1160") == true) // just looking for the start of the epc of a dogbone.
                //{
                    epcBottom.Content = tag.Epc.ToString();
                    double rssi = tag.PeakRssiInDbm;
                    rssiLabel.Content = rssi.ToString();
                    if (rssi < -15 && rssi > -50)
                    {
                        YellowLight.Fill = new SolidColorBrush(Colors.White);
                        GreenLight.Fill = new SolidColorBrush(Colors.Green);
                    }
                    else if (rssi < -51 && rssi > -75)
                    {
                        yellow = false;
                        YellowLight.Fill = new SolidColorBrush(Colors.Yellow);
                        GreenLight.Fill = new SolidColorBrush(Colors.White);

                    }

                    rssi = ((-rssi) - 15) * 3.8;
                    int rssiCompu = Convert.ToInt32(rssi);

                    r = 255 - rssiCompu;
                    g = 0 + rssiCompu;
                    CheckConstraints(r, 0, 255);
                    CheckConstraints(g, 0, 255);
                    rb = Convert.ToByte(r);
                    gb = Convert.ToByte(g);
                    EPCCap.StartsWith("E280 1160");

                    tagLabel.Content = EPCCap.Trim();
                    hit = true;
                    yesno.Content = "RSSI :\n" + tag.PeakRssiInDbm.ToString();
                    yesno.Background = new SolidColorBrush(Color.FromArgb(255, gb, rb, 0));

                //}
                //else
                //{
                //    epcLabel.Content = "Beware Foreign Tag Detected\nEPC: " + tag.Epc.ToString();

                //}
            }
        }

        // Make sure that the values we are using don't come out of a 0-255 Range.
        private void CheckConstraints(int r, int min, int max)
        {
            if (r > max)
            {
                r = max;
            }
            if (r < min)
            {
                r = min;
            }
        }

        private void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(500);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }


        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            //Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
            //              e.SignalTime);
            //Console.WriteLine(hit);
            Action action = delegate ()
            {
                Update1();
            };
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);

        }

        void Update1()
        {
            if (hit == true)
            {
                status.Fill = new SolidColorBrush(Colors.Green);
                // GreenLight.Fill = new SolidColorBrush(Colors.Green);
                RedLight.Fill = new SolidColorBrush(Colors.White);

                hit = false;
            }
            else if (hit == false)
            {
                status.Fill = new SolidColorBrush(Colors.Red);
                RedLight.Fill = new SolidColorBrush(Colors.Red);
                YellowLight.Fill = new SolidColorBrush(Colors.White);
                GreenLight.Fill = new SolidColorBrush(Colors.White);
            }
        }

        private void OnTagsReported(ImpinjReader sender, TagReport report)
        {
            // This event handler gets called when a tag report is available.
            // Since it is executed in a different thread, we cannot operate
            // directly on UI elements (the Listbox) in this method.
            // We must execute another method (UpdateListbox) on the main thread
            // using BeginInvoke. We will pass updateListbox a List of tags.
            Action action = delegate ()
            {

                UpdateListbox(report.Tags);
                Console.WriteLine(report.Tags.Count);
            };
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }

        //private void ButtonStop_Click(object sender, RoutedEventArgs e)
        //{
        //    // Don't call the Stop method if the
        //    // reader is already stopped.
        //    if (reader.QueryStatus().IsSingulating)
        //    {
        //        reader.Stop();
        //    }
        //}


        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (start == true)
                {
                    reader.Connect(ipAddress);
                    Initialise();
                    start = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot Connect to Reader\nEither try again in 30s or reboot the reader and this application",
    "Error creating connection",
    MessageBoxButton.OK, MessageBoxImage.Error);


            }

            try
            {

                if (!reader.QueryStatus().IsSingulating)
                {
                    Initialise();
                    reader.Start();

                    buttonStart.Content = "STOP";
                }
                else if (reader.QueryStatus().IsSingulating)
                {
                    reader.Stop();
                    buttonStart.Content = "START";
                }

            }
            catch (OctaneSdkException ex)
            {
                // An Octane SDK exception occurred. Handle it here.
                System.Diagnostics.Trace.
                    WriteLine("An Octane SDK exception has occurred : {0}", ex.Message);
            }
            catch (Exception ex)
            {
                // A general exception occurred. Handle it here.
                System.Diagnostics.Trace.
                    WriteLine("An exception has occurred : {0}", ex.Message);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // The application is closing.
            // Stop the reader and disconnect.
            try
            {
                // Don't call the Stop method if the
                // reader is already stopped.
                if (reader.QueryStatus().IsSingulating)
                {
                    reader.Stop();
                }
                // Disconnect from the reader.
                reader.Disconnect();
            }
            catch (OctaneSdkException ex)
            {
                // An Octane SDK exception occurred. Handle it here.
                System.Diagnostics.Trace.
                    WriteLine("An Octane SDK exception has occurred : {0}", ex.Message);
            }
            catch (Exception ex)
            {
                // A general exception occurred. Handle it here.
                System.Diagnostics.Trace.
                    WriteLine("An exception has occurred : {0}", ex.Message);
            }
        }

        private void epcPower_PreviewGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {

        }

        private void epcPower_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
