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
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;

namespace PSI
{
    /// <summary>
    /// Logique d'interaction pour Nuances.xaml
    /// </summary>
    public partial class Nuances : Window
    {
        MyImage image;
        WriteableBitmap imagelecture;
        OpenFileDialog dlg;
        public Nuances()
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
            if(image == null)
            {
                System.Windows.Forms.MessageBox.Show("vous n'avez pas sélectionné d'image", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (NuancesGris.IsChecked == true)
                {
                    image = image.NuanceDeGris();
                }
                if (NuancesJaune.IsChecked == true)
                {
                    image = image.NuanceDeJaune();
                }
                if(NuancesBleu.IsChecked == true)
                {
                    image = image.NuanceDeBleu();
                }
                if(NuancesRouge.IsChecked == true)
                {
                    image = image.NuanceDeRouge();
                }
                if(NuancesVert.IsChecked == true)
                {
                    image = image.NuanceDeVert();
                }
                if (NoirEtBlanc.IsChecked == true)
                {
                    image = image.NoirEtBlanc();
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
