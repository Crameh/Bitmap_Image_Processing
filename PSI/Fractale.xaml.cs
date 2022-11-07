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
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using System.Numerics;

namespace PSI
{
    /// <summary>
    /// Logique d'interaction pour Fractale.xaml
    /// </summary>
    public partial class Fractale : Window
    {
        MyImage image = new MyImage();
        WriteableBitmap imagelecture;
        OpenFileDialog dlg;
        public Fractale()
        {
            InitializeComponent();
            dlg = new OpenFileDialog();
            dlg.Filter = "Image files (*.bmp)|*.bmp";
        }
        private void Retour(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        private void Generer(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Cela peut prendre du temps", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (Mandelbrot.IsChecked == true)
            {
                Complex c1 = new Complex(-2.1, -1.2);
                Complex c2 = new Complex(0.6, 1.2);
                string nomFractale = "Mandelbrot";
                image = image.Fractale(c1, c2, nomFractale);
            }
            if (Julia.IsChecked == true)
            {
                Complex c1 = new Complex(-1, -1.2);
                Complex c2 = new Complex(1, 1.2);
                string nomFractale = "Julia";
                image = image.Fractale(c1, c2, nomFractale);
            }
            Int32Rect rectangle = new Int32Rect(0, 0, image.Largeur, image.Hauteur);
            byte[] tableau = image.MatToTab();
            imagelecture = new WriteableBitmap(image.Largeur, image.Hauteur, 96, 96, PixelFormats.Bgr32, null);
            imagelecture.WritePixels(rectangle, tableau, image.Largeur * 4, 0, 0);
            ImageViewer.Source = imagelecture;
        }
        private void Save(object sender, RoutedEventArgs e)
        {
            if (image == null)
            {
                System.Windows.Forms.MessageBox.Show("Vous n'avez pas généré de fractale", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Stream myStream;
                SaveFileDialog fichier = new SaveFileDialog();
                fichier.Filter = "Image files (*.bmp)|*.bmp";
                if (fichier.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if ((myStream = fichier.OpenFile()) != null)
                    {
                        myStream.Close();
                    }
                }
                File.WriteAllBytes(fichier.FileName, image.From_Image_To_File());
            }
        }
    }
}
