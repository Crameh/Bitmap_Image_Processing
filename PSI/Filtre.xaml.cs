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
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Shapes;

namespace PSI
{
    /// <summary>
    /// Logique d'interaction pour Filtre.xaml
    /// </summary>
    public partial class Filtre : Window
    {
        MyImage image;
        WriteableBitmap imagelecture;
        OpenFileDialog dlg;
        public Filtre()
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
        private void ChargerImage(object sender, RoutedEventArgs e)
        {
            string filename = null;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filename = dlg.FileName;
            }
            imagelecture = new WriteableBitmap(new BitmapImage(new Uri(filename)));
            image = new MyImage(filename);
            ImageViewer.Source = imagelecture;
        }
        private void Appliquer(object sender, RoutedEventArgs e)
        {
            if (image == null)
            {
                System.Windows.Forms.MessageBox.Show("vous n'avez pas sélectionné d'image", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (Contours.IsChecked == true)
                {
                    int[,] detectionContours = { { -1, -1, -1 }, { -1, 8, -1 }, { -1, -1, -1 } };
                    image = image.AppliquerFiltre(detectionContours);
                }
                if (Bords.IsChecked == true)
                {
                    int[,] renforcementBords = { { 0, 0, 0 }, { -1, 1, 0 }, { 0, 0, 0 } };
                    image = image.AppliquerFiltre(renforcementBords);
                }
                if (Flou.IsChecked == true)
                {
                    int[,] flou = { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
                    image = image.AppliquerFiltre(flou);
                }
                if (Repoussage.IsChecked == true)
                {
                    int[,] repoussage = { { -2, -1, 0 }, { -1, 1, 1 }, { 0, 1, 2 } };
                    image = image.AppliquerFiltre(repoussage);
                }
                if (Contrastes.IsChecked == true)
                {
                    int[,] augmentationContraste = { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
                    image = image.AppliquerFiltre(augmentationContraste);
                }
                if (FlouGauss.IsChecked == true)
                {
                    int[,] flouGaussien = { { 1, 4, 6, 4, 1 }, { 4, 16, 24, 16, 4 }, { 6, 24, 36, 24, 6 }, { 4, 16, 24, 16, 4 }, { 1, 4, 6, 4, 1 } };
                    image = image.AppliquerFiltre(flouGaussien);
                }
                Int32Rect rectangle = new Int32Rect(0, 0, image.Largeur, image.Hauteur);
                byte[] tableau = image.MatToTab();
                imagelecture.WritePixels(rectangle, tableau, image.Largeur * 4, 0, 0);
                ImageViewer.Source = imagelecture;
            }
        }
        private void Save(object sender, RoutedEventArgs e)
        {
            if (image == null)
            {
                System.Windows.Forms.MessageBox.Show("Il n'y a pas d'image à sauvegarder", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
