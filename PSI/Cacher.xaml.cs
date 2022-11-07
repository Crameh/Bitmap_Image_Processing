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

namespace PSI
{
    /// <summary>
    /// Logique d'interaction pour Cacher.xaml
    /// </summary>
    public partial class Cacher : Window
    {
        MyImage image;
        MyImage image2;
        WriteableBitmap imagelecture;
        WriteableBitmap imagelecture2;
        WriteableBitmap imagelecture3;
        WriteableBitmap imagelecture4;
        OpenFileDialog dlg;
        public Cacher()
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
        private void Charger2Images(object sender, RoutedEventArgs e)
        {
            string filename = null;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filename = dlg.FileName;
            }
            imagelecture = new WriteableBitmap(new BitmapImage(new Uri(filename)));
            image = new MyImage(filename);
            ImageViewer.Source = imagelecture;
            System.Windows.Forms.MessageBox.Show("Sélectionner maintenant une deuxième image différente","Message",MessageBoxButtons.OK, MessageBoxIcon.Information);
            string filenamebis = null;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filenamebis = dlg.FileName;
            }
            imagelecture2 = new WriteableBitmap(new BitmapImage(new Uri(filenamebis)));
            image2 = new MyImage(filenamebis);
            ImageViewer2.Source = imagelecture2;
        }

        private void CacherImage(object sender, RoutedEventArgs e)
        {
            if(image == null || image2 == null)
            {
                System.Windows.Forms.MessageBox.Show("Vous n'avez pas sélectionné deux images","Message",MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                image = image.CoderImage(image2);
                Int32Rect rectangle = new Int32Rect(0, 0, image.Largeur, image.Hauteur);
                byte[] tableau = image.MatToTab();
                imagelecture3 = new WriteableBitmap(image.Largeur, image.Hauteur, 96, 96, PixelFormats.Bgr32, null);
                imagelecture3.WritePixels(rectangle, tableau, image.Largeur * 4, 0, 0);
                Coder.Source = imagelecture3;
            }
        }
        private void DevoilerImage(object sender, RoutedEventArgs e)
        {
            if (image == null)
            {
                System.Windows.Forms.MessageBox.Show("Vous n'avez pas encore caché une image dans une autre","Message",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                image = image.DecoderImage();
                Int32Rect rectangle = new Int32Rect(0, 0, image.Largeur, image.Hauteur);
                byte[] tableau = image.MatToTab();
                imagelecture4 = new WriteableBitmap(image.Largeur, image.Hauteur, 96, 96, PixelFormats.Bgr32, null);
                imagelecture4.WritePixels(rectangle, tableau, image.Largeur * 4, 0, 0);
                Decoder.Source = imagelecture4;
            }
        }
        private void Save(object sender, RoutedEventArgs e)
        {
            if (image == null)
            {
                System.Windows.Forms.MessageBox.Show("Il n'y a pas d'image à sauvegarder", "Message", MessageBoxButtons.OK,MessageBoxIcon.Error);
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
