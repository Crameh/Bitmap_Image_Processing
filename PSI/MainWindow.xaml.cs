using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;
using System.Windows;

namespace PSI
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void ChoisirPage(object sender, RoutedEventArgs e)
        {
            if (Nuances.IsChecked == true)
            {
                Nuances nuances = new Nuances();
                nuances.Show();
            }
            if(Deformation.IsChecked == true)
            {
                Deformation deformation = new Deformation();
                deformation.Show();
            }
            if (Filtre.IsChecked == true)
            {
                Filtre filtre = new Filtre();
                filtre.Show();
            }
            if(Fractales.IsChecked == true)
            {
                Fractale fractale = new Fractale();
                fractale.Show();
            }
            if(Histogramme.IsChecked == true)
            {
                Histogramme histogramme = new Histogramme();
                histogramme.Show();
            }
            if(Cacher.IsChecked == true)
            {
                Cacher cacher = new Cacher();
                cacher.Show();
            }
            if(QRCode.IsChecked == true)
            {
                QrCode qrcode = new QrCode();
                qrcode.Show();
            }
            this.Close();
        }
    }
}
