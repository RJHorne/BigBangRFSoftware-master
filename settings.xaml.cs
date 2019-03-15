using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for settings.xaml
    /// </summary>
    public partial class settings : Window
    {
        public settings()
        {
            InitializeComponent();
           
        }

        private void DefaultsApply_Click(object sender, RoutedEventArgs e)
        {
            ipAddressTextBox.Text = "169.254.1.1";
            powerBox.Text = "25";            
            greenValueTextBox.Text = "-50";
            yellowValueTextBox.Text = "-60";
            epcFilterTextBox.Text = "E280 1160";
        }


       public int power { get; set; }
       public string ipAddressValue { get; set; }
       public int greenValue { get; set; }
       public int yellowValue { get; set; }
       public String epcFilter { get; set; }


        private void SettingsApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ipAddressValue = ipAddressTextBox.Text;
            power = Convert.ToInt32(powerBox.Text);
            greenValue = Convert.ToInt32(greenValueTextBox.Text);
            yellowValue = Convert.ToInt32(yellowValueTextBox.Text);
            epcFilter = epcFilterTextBox.Text;
            this.Close();
        }
    }
}
