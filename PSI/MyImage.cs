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
using System.Windows.Forms;
using System.ComponentModel;
using System.Numerics;

namespace PSI
{
    class MyImage
    {
        ////////////////////////////////////////////////
        ///////Constructeurs et Sauvegarde (TD2)////////
        ////////////////////////////////////////////////
        #region
        string myfile;
        byte[] tabfile;
        byte[] tabheader;
        string type;
        int taille;
        int offset;
        int largeur;
        int hauteur;
        int bitcolor;
        Pixel[,] image;
        public int Largeur
        {
            get { return largeur; }
        }
        public int Hauteur
        {
            get { return hauteur; }
        }
        /// <summary>
        /// Constructeur qui crée une nouvelle instance de MyImage vide
        /// </summary>
        public MyImage(){}
        /// <summary>
        /// Constructeur qui crée une nouvelle instance de MyImage à partir du nom de fichier de l'image choisie
        /// </summary>
        /// <param name="myfile"> String contenant le nom du fichier </param>
        public MyImage(string myfile)
        {
            this.myfile = myfile;
            tabfile = File.ReadAllBytes(myfile);
            //On extrait le header qui contient toutes les infos
            tabheader = new byte[54];
            for (int i = 0; i < 54; i++)
            {
                tabheader[i] = tabfile[i];
            }
            char a = Convert.ToChar(tabfile[0]);
            char b = Convert.ToChar(tabfile[1]);
            type = Convert.ToString(a) + Convert.ToString(b);
            //On crée des tableaux pour stocker chaque informations de l'image
            byte[] tabtaille = new byte[4];
            byte[] tablarg = new byte[4];
            byte[] tabhaut = new byte[4];
            byte[] taboffset = new byte[4];
            byte[] tabcolor = new byte[2];
            //On extrait ces informations
            for (int i = 0; i < 4; i++)
            {
                tabtaille[i] = tabfile[i + 2];
                tablarg[i] = tabfile[i + 18];
                tabhaut[i] = tabfile[i + 22];
                taboffset[i] = tabfile[i + 34];
            }
            for (int i = 0; i < 2; i++)
            {
                tabcolor[i] = tabfile[i + 28];
            }
            //On convertit ces informations en entiers pour pouvoir travailelr avec
            taille = Convertir_Endian_To_Int(tabtaille);
            largeur = Convertir_Endian_To_Int(tablarg);
            hauteur = Convertir_Endian_To_Int(tabhaut);
            offset = Convertir_Endian_To_Int(taboffset);
            bitcolor = Convertir_Endian_To_Int(tabcolor);
            image = new Pixel[hauteur, largeur];
            int l = 0;
            //On crée l'image en elle-même en extrayant chaque sous-pixels
            for (int i = 54; i < tabfile.Length; i = i + largeur * 3)
            {
                int c = 0;
                for (int j = i; j < i + largeur * 3; j = j + 3)
                {
                    image[hauteur - l - 1, c] = new Pixel(Convert.ToByte(tabfile[j + 2]), Convert.ToByte(tabfile[j + 1]), Convert.ToByte(tabfile[j]));
                    c++;
                }
                l++;
            }
        }
        /// <summary>
        /// Constructeur qui crée une nouvelle instance de MyImage à partir de dfférents paramètres la caractérisant
        /// </summary>
        /// <param name="taille"> Entier qui stock la taille en octets de l'image </param>
        /// <param name="largeur"> Entier qui stock la largeur de l'image </param>
        /// <param name="hauteur"> Entier qui stock la hauteur de l'image </param>
        /// <param name="matImage"> Entier qui sotck la matrice de pixels de l'image </param>
        public MyImage(int taille, int largeur, int hauteur, Pixel[,] matImage)
        {
            this.taille = taille;
            this.largeur = largeur;
            this.hauteur = hauteur;
            image = matImage;
            bitcolor = 24;
        }
        /// <summary>
        /// Fonction qui permet de reconvertir l'instance de MyImage en tableau de bytes pour sauvergarder une image
        /// </summary>
        /// <returns> Retourne un tableau de bytes contenant les informations de l'image </returns>
        public byte[] From_Image_To_File()
        {
            byte[] newtabfile = new byte[taille];
            //On stock aux bonnes positions les valeurs qui ne changent jamais pour une image bitmap
            newtabfile[0] = 66;
            newtabfile[1] = 77;
            newtabfile[10] = 54;
            newtabfile[14] = 40;
            newtabfile[26] = 1;
            //On convertit en little endian les différentes informations de l'image
            byte[] tabtaille = Convertir_Int_To_Endian(taille, 4);
            byte[] tablarg = Convertir_Int_To_Endian(largeur, 4);
            byte[] tabhaut = Convertir_Int_To_Endian(hauteur, 4);
            byte[] taboffset = Convertir_Int_To_Endian(offset, 4);
            byte[] tabcolor = Convertir_Int_To_Endian(bitcolor, 2);
            //On stock aux bonnes positions ces différents informations
            for (int i = 0; i < 4; i++)
            {
                newtabfile[i + 2] = tabtaille[i];
                newtabfile[i + 18] = tablarg[i];
                newtabfile[i + 22] = tabhaut[i];
                newtabfile[i + 34] = taboffset[i];
            }
            for (int i = 0; i < 2; i++)
            {
                newtabfile[i + 28] = tabcolor[i];
            }
            int c = 54;
            //On stock la valeur de chaque sous-pixels de l'image a sa bonne position
            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    newtabfile[c] = image[image.GetLength(0) - 1 - i, j].B;
                    newtabfile[c + 1] = image[image.GetLength(0) - 1 - i, j].G;
                    newtabfile[c + 2] = image[image.GetLength(0) - 1 - i, j].R;
                    c = c + 3;
                }
            }
            return newtabfile;
        }
        #endregion
        /////////////////////////////////////
        ///////Traiter une image (TD3)///////
        /////////////////////////////////////
        #region
        /// <summary>
        /// Fonction qui applique un filtre de nuances de gris sur l'instance de MyImage
        /// </summary>
        /// <returns> Retourne une nouvelle instance de MyImage en nuances de gris </returns>
        public MyImage NuanceDeGris()
        {
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    //On calcul la valeur moyenne du pixel avec les trois sous-pixels et on stock cette valeur dans chaque sous-pixel ce qui fait une nuance de gris
                    int moyenne = (image[i, j].R + image[i, j].G + image[i, j].B) / 3;
                    image[i, j].R = Convert.ToByte(moyenne);
                    image[i, j].G = Convert.ToByte(moyenne);
                    image[i, j].B = Convert.ToByte(moyenne);
                }
            }
            MyImage imagebis = new MyImage(taille, largeur, hauteur, image);
            return imagebis;
        }
        /// <summary>
        /// Fonction qui isole les pixels jaune sur l'instance de MyImage
        /// </summary>
        /// <returns> Retourne une nouvelle instance de MyImage avec isolation des pixels jaune et nuances de gris pour le reste </returns>
        public MyImage NuanceDeJaune()
        {
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    //De même que pour la fonction nuance de gris saud qu'on le fait que seulement si le pixel n'est pas une nuance de jaune, 
                    //c'est à dire que ses sous-pixels dépassent certaines valeurs
                    if (image[i, j].R < 170 || image[i, j].G < 125 || image[i, j].B > 60)
                    {
                        int moyenne = (image[i, j].R + image[i, j].G + image[i, j].B) / 3;
                        image[i, j].R = Convert.ToByte(moyenne);
                        image[i, j].G = Convert.ToByte(moyenne);
                        image[i, j].B = Convert.ToByte(moyenne);
                    }
                }
            }
            MyImage imagebis = new MyImage(taille, largeur, hauteur, image);
            return imagebis;
        }
        /// <summary>
        /// Fonction qui isole les pixels bleu sur l'instance de MyImage
        /// </summary>
        /// <returns> Retourne une nouvelle instance de MyImage avec isolation des pixels bleu et nuances de gris pour le reste </returns>
        public MyImage NuanceDeBleu()
        {
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    //De même que pour NuanceDeJaune
                    if (image[i, j].R > 100 || image[i, j].G > 180 || image[i, j].B < 100)
                    {
                        int moyenne = (image[i, j].R + image[i, j].G + image[i, j].B) / 3;
                        image[i, j].R = Convert.ToByte(moyenne);
                        image[i, j].G = Convert.ToByte(moyenne);
                        image[i, j].B = Convert.ToByte(moyenne);
                    }
                }
            }
            MyImage imagebis = new MyImage(taille, largeur, hauteur, image);
            return imagebis;
        }
        /// <summary>
        /// Fonction qui isole les pixels rouge sur l'instance de MyImage
        /// </summary>
        /// <returns> Retourne une nouvelle instance de MyImage avec isolation des pixels rouge et nuances de gris pour le reste </returns>
        public MyImage NuanceDeRouge()
        {
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    //De même que pour NuanceDeJaune
                    if (image[i, j].R < 80 || image[i, j].G > 80 || image[i, j].B > 90)
                    {
                        int moyenne = (image[i, j].R + image[i, j].G + image[i, j].B) / 3;
                        image[i, j].R = Convert.ToByte(moyenne);
                        image[i, j].G = Convert.ToByte(moyenne);
                        image[i, j].B = Convert.ToByte(moyenne);
                    }
                }
            }
            MyImage imagebis = new MyImage(taille, largeur, hauteur, image);
            return imagebis;
        }
        /// <summary>
        /// Fonction qui isole les pixels vert sur l'instance de MyImage
        /// </summary>
        /// <returns> Retourne une nouvelle instance de MyImage avec isolation des pixels vert et nuances de gris pour le reste </returns>
        public MyImage NuanceDeVert()
        {
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    //De même que pour NuanceDeJaune
                    if (image[i, j].R > 150 || image[i, j].G < 70 || image[i, j].B > 110)
                    {
                        int moyenne = (image[i, j].R + image[i, j].G + image[i, j].B) / 3;
                        image[i, j].R = Convert.ToByte(moyenne);
                        image[i, j].G = Convert.ToByte(moyenne);
                        image[i, j].B = Convert.ToByte(moyenne);
                    }
                }
            }
            MyImage imagebis = new MyImage(taille, largeur, hauteur, image);
            return imagebis;
        }
        /// <summary>
        /// Fonction qui applique un filtre de noir et blanc sur l'instance de MyImage
        /// </summary>
        /// <returns> Retourne une nouvelle instance de MyImage en nuances de gris </returns>
        public MyImage NoirEtBlanc()
        {
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    //On fait la moyenne de la valeur des sous-pixels et si c'est moyenne est inférieure à la moitié alors on attribue noir et sinon on attribue noir
                    int moyenne = (image[i, j].R + image[i, j].G + image[i, j].B) / 3;
                    if (moyenne < 128)
                    {
                        image[i, j] = new Pixel(0, 0, 0);
                    }
                    else
                    {
                        image[i,j] = new Pixel(255, 255, 255);
                    }
                }
            }
            MyImage imagebis = new MyImage(taille, largeur, hauteur, image);
            return imagebis;
        }
        /// <summary>
        /// Fonction qui applique un filtre d'effet miroir vertical sur l'instance de MyImage
        /// </summary>
        /// <returns> Retourne une nouvelle instance de MyImage en effet miroir </returns>
        public MyImage EffetMiroir()
        {
            Pixel[,] newImage = new Pixel[hauteur, largeur];
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    //On échange de position les pixels par rapport à la ligne du mileu verticale
                    newImage[i, largeur - j - 1] = image[i, j];
                }
            }
            MyImage imagebis = new MyImage(taille, largeur, hauteur, newImage);
            return imagebis;
        }
        /// <summary>
        /// Focntion qui redimensionne une image proportionnellement par rapport à un coefficient
        /// </summary>
        /// <param name="coeff"> cofficient par lequel la hauteur et largeur de l'image va être multipliée qui doit être > 0 </param>
        /// <returns> Retourne une nouvelle instance de MyImage qui est redimensionnée </returns>
        public MyImage Redimensionnement(double coeff)
        {
            //On calcule la nouvelle hauteur et largeur en l'arrondissant au multiple de 4 le plus proche inférieur
            int newhauteur = ConvertCoeff4(hauteur, coeff);
            int newlargeur = ConvertCoeff4(largeur, coeff);
            Pixel[,] matAgrandie = new Pixel[newhauteur, newlargeur];
            //On calcule le ratio sur la hauteur et la largeur qui va nous servir à trouver les coordonnées
            double ratioH = (double)newhauteur / (double)hauteur;
            double ratioL = (double)newlargeur / (double)largeur;
            for (int i = 0; i < newhauteur; i++)
            {
                for (int j = 0; j < newlargeur; j++)
                {
                    //On cherche les coordonées en hauteur et largeur du pixel de l'image de base par rapport à la nouvelle image
                    int coordx = Convert.ToInt32(Math.Floor(i / ratioH));
                    int coordy = Convert.ToInt32(Math.Floor(j / ratioL));
                    matAgrandie[i, j] = image[coordx, coordy];
                }
            }
            MyImage imagebis = new MyImage(newlargeur * newhauteur * 3 + 54, newlargeur, newhauteur, matAgrandie);
            return imagebis;
        }
        /// <summary>
        /// Fonction qui prend la dimension et le coeff afin de retourner la nouvelle dimension au multiple de 4 le proche inférieur
        /// </summary>
        /// <param name="dimension"> dimension de l'instance de MyImage </param>
        /// <param name="coeff"> coefficient qui sert à redimensionner l'image </param>
        /// <returns> Retourne la nouvelle valeur de la dimension </returns>
        public int ConvertCoeff4(int dimension, double coeff)
        {
            double res = 0;
            res = dimension * coeff;
            double div = res % 4;
            if (div == 0)
            {
                int calc = Convert.ToInt32(res);
                return calc;
            }
            else
            {
                //double diff = 4 - div;    Ces deux lignes permettait de prendre le multiple de 4 le plus proche au-dessus
                //res = res + diff;
                res = res - div;
                int calc = Convert.ToInt32(res);
                return calc;
            }
        }
        /// <summary>
        /// Fonction qui fait une rotation d'image avec un angle quelconque
        /// </summary>
        /// <param name="angle"> angle de rotation de l'image </param>
        /// <returns> Retourne une nouvelle instance de MyImage </returns>
        public MyImage Rotation(double angle)
        {
            angle = angle * Math.PI / 180;
            //On calcule la nouvelle hauteur et largeur de l'image après rotation avec les formules de trigonométrie
            int newhauteur = Convert.ToInt32(hauteur * Math.Cos(angle) + largeur * Math.Sin(angle));
            int newlargeur = Convert.ToInt32(hauteur * Math.Sin(angle) + largeur * Math.Cos(angle));
            //On les rappproche au multiple de 4 le plus proche
            newhauteur = Convert4(newhauteur);
            newlargeur = Convert4(newlargeur);
            Pixel[,] matRotation = new Pixel[newhauteur, newlargeur];
            for (int i = 0; i < newhauteur; i++)
            {
                for (int j = 0; j < newlargeur; j++)
                {
                    //Comme pour le redimensionnement on calcul l'endroit où on doit aller chercher le pixel dans l'image de base pour l'implémenter dans la nouvelle
                    //Pour cela on applique une formule trigonométrique où l'on centre le centre de rotation de l'image
                    int posx = Convert.ToInt32((i - matRotation.GetLength(0) / 2) * Math.Cos(angle) + (j - matRotation.GetLength(1) / 2) * Math.Sin(angle) + (image.GetLength(0) / 2));
                    int posy = Convert.ToInt32((i - matRotation.GetLength(0) / 2) * (-Math.Sin(angle)) + (j - matRotation.GetLength(1) / 2) * Math.Cos(angle) + (image.GetLength(1) / 2));
                    if (posx >= 0 && posx < image.GetLength(0) && posy >= 0 && posy < image.GetLength(1))
                    {
                        matRotation[i, j] = image[posx, posy];
                    }
                    else
                    {
                        matRotation[i, j] = new Pixel(255, 255, 255);
                    }
                }
            }
            MyImage imagebis = new MyImage(newhauteur * newlargeur * 3 + 54, newlargeur, newhauteur, matRotation);
            return imagebis;
        }
        /// <summary>
        /// Fonction qui prend la dimension afin de retourner la nouvelle dimension au multiple de 4 le plus proche
        /// </summary>
        /// <param name="dimension"> dimension de l'instance de MyImage </param>
        /// <returns> Retourne la nouvelle valeur de la dimension </returns>
        public int Convert4(int dimension)
        {
            double div = dimension % 4;
            double res = (double)dimension;
            if (div == 0)
            {
                int calc = Convert.ToInt32(res);
                return calc;
            }
            else
            {
                double diff = 4 - div;
                res = res + diff;
                int calc = Convert.ToInt32(res);
                return calc;
            }
        }
        #endregion
        ///////////////////////////////////////
        ///////Appliquer un filtre (TD4)///////
        ///////////////////////////////////////
        #region
        /// <summary>
        /// Fonction qui calcul la couleur du pixel en lui appliquant la matrice de kernel
        /// </summary>
        /// <param name="kernel"> Matrice de kernel qui contient les coefficients du filtre </param>
        /// <param name="i"> Position du numéro de ligne du pixel </param>
        /// <param name="j"> Position du numéro de colonne du pixel </param>
        /// <returns> Retourne le pixel avec ses nouvelles valeurs de rouge, vert et bleu </returns>
        public Pixel CalculKernel(int[,] kernel, int i, int j)
        {
            Pixel pixel = new Pixel(0, 0, 0);
            byte rouge;
            byte vert;
            byte bleu;
            int sommeR = 0;
            int sommeG = 0;
            int sommeB = 0;
            int sommeKernel = 0;
            int cpt = kernel.GetLength(0) / 2;
            for (int k = 0; k < kernel.GetLength(0); k++)
            {
                int x = (i - cpt) + k;
                //On regarde si on dépasse les bornes pour prendre l'opposé (principe de la matrice de convolution)
                if (x < 0) { x += image.GetLength(0); }
                if (x > image.GetLength(0) - 1) { x -= image.GetLength(0); }
                for (int l = 0; l < kernel.GetLength(1); l++)
                {
                    int y = (j - cpt) + l;
                    //On regarde si on dépasse les bornes pour prendre l'opposé (principe de la matrice de convolution)
                    if (y < 0) { y += image.GetLength(1); }
                    if (y > image.GetLength(1) - 1) { y -= image.GetLength(1); }
                    //On applique la matrice de kernel à chaque sous-pixel
                    sommeR += (image[x, y].R) * kernel[k, l];
                    sommeG += (image[x, y].G) * kernel[k, l];
                    sommeB += (image[x, y].B) * kernel[k, l];
                    sommeKernel += kernel[k, l];
                }
            }
            //On ajuste si la somme est inférieure à 0 ou supérieure à 255
            if (sommeR < 0) { rouge = 0; }
            else
            {
                if (sommeR > 255) { rouge = 255; }
                else
                {
                    //On applique le filtre au sous-pixel
                    if (sommeKernel == 0) { rouge = (byte)sommeR; }
                    else { rouge = (byte)(sommeR / sommeKernel); }
                }
            }
            //On ajuste si la somme est inférieure à 0 ou supérieure à 255
            if (sommeG < 0) { vert = 0; }
            else
            {
                if (sommeG > 255) { vert = 255; }
                else
                {
                    //On applique le filtre au sous-pixel
                    if (sommeKernel == 0) { vert = (byte)sommeG; }
                    else { vert = (byte)(sommeG / sommeKernel); }
                }
            }
            //On ajuste si la somme est inférieure à 0 ou supérieure à 255
            if (sommeB < 0) { bleu = 0; }
            else
            {
                if (sommeB > 255) { bleu = 255; }
                else
                {
                    //On applique le filtre au sous-pixel
                    if (sommeKernel == 0) { bleu = (byte)sommeB; }
                    else { bleu = (byte)(sommeB / sommeKernel); }
                }
            }
            if (sommeKernel > 1)
            {
                bleu = (byte)(sommeB / sommeKernel);
                vert = (byte)(sommeG / sommeKernel);
                rouge = (byte)(sommeR / sommeKernel);
            }
            pixel.R = rouge;
            pixel.G = vert;
            pixel.B = bleu;
            return pixel;
        }
        /// <summary>
        /// Fonction qui applique le filtre de Kernel
        /// </summary>
        /// <param name="kernel"> Matrice de Kernel qui contient les différents coefficients des filtres </param>
        /// <returns> Retourne une nouvelle instance de MyImage </returns>
        public MyImage AppliquerFiltre(int[,] kernel)
        {
            Pixel[,] matFiltre = new Pixel[hauteur, largeur];
            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    //On parcours la matrice et pour chaque pixel on applique la matrice de Kernel
                    Pixel pixel = CalculKernel(kernel, i, j);
                    matFiltre[i, j] = pixel;
                }
            }
            MyImage imagebis = new MyImage(taille, largeur, hauteur, matFiltre);
            return imagebis;
        }
        #endregion
        ////////////////////////////////////////////
        ///////Créer une nouvelle image (TD5)///////
        ////////////////////////////////////////////
        #region
        /// <summary>
        /// Fonction qui crée une fractale 
        /// </summary>
        /// <param name="c1"> Premier complexe de la fractale </param>
        /// <param name="c2"> Deuxième complexe de la fractale </param>
        /// <param name="name"> Nom de la fractale </param>
        /// <returns> Retourne une instance de MyImage contenant une fractale </returns>
        public MyImage Fractale(Complex c1, Complex c2, string name)
        {
            double zoom = 1000;
            double iterationmax = 50;
            //On calcule la largeur et la hauteur de la fractale et on arrondit au multiple de 4 le plus proche
            int hauteurmax = Convert4(Convert.ToInt32((c2.Real - c1.Real) * zoom));
            int largeurmax = Convert4(Convert.ToInt32((c2.Imaginary - c1.Imaginary) * zoom));
            Pixel[,] fractale = new Pixel[hauteurmax, largeurmax];
            for (int x = 0; x < hauteurmax; x++)
            {
                for (int y = 0; y < largeurmax; y++)
                {
                    Complex c = new Complex(0, 0);
                    Complex z = new Complex(0, 0);
                    int pas = 0;
                    //On change les complexes en fonction de la fractale que l'on veut
                    if (name == "Mandelbrot")
                    {
                        c = new Complex(x / zoom + c1.Real, y / zoom + c1.Imaginary);
                        z = new Complex(0, 0);
                        pas = 5;
                    }
                    else if (name == "Julia")
                    {
                        c = new Complex(0.285, 0.01);
                        z = new Complex(x / zoom + c1.Real, y / zoom + c1.Imaginary);
                        pas = 20;
                    }
                    int i = 0;
                    //On applique la formule mathématique du calcul d'une fractale
                    do
                    {
                        z = z * z + c;
                        i++;
                    }
                    while (z.Magnitude < 2 && i < iterationmax);
                    if (i == iterationmax)
                    {
                        fractale[x, y] = new Pixel(0, 0, 0);
                    }
                    //On applique différentes couleurs afin de la rendre un peu plus jolie
                    if (i < pas)
                    {
                        fractale[x, y] = new Pixel((byte)((i - pas) * 255 / (pas)), 0, 0);
                    }
                    else if (i < pas * 2)
                    {
                        fractale[x, y] = new Pixel(0, (byte)((i - pas * 2) * 255 / pas), 0);
                    }
                    else if (i < pas * 3)
                    {
                        fractale[x, y] = new Pixel(0, 0, (byte)((i - pas * 3) * 255 / pas));
                    }
                    else if (i < pas * 4)
                    {
                        fractale[x, y] = new Pixel((byte)((i - pas * 4) * 255 / pas), 0, (byte)((i - pas * 4) * 255 / pas));
                    }
                    else if (i < pas * 5)
                    {
                        fractale[x, y] = new Pixel((byte)((i - pas * 5) * 255 / pas), (byte)((i - pas * 5) * 255 / pas), (byte)((i - pas * 5) * 255 / pas));
                    }
                    else if (i < pas * 6)
                    {
                        fractale[x, y] = new Pixel((byte)((i - pas * 6) * 255 / pas), (byte)((i - pas * 6) * 255 / pas), (byte)((i - pas * 6) * 255 / pas));
                    }
                    else if (i < pas * 7)
                    {
                        fractale[x, y] = new Pixel((byte)((i - pas * 7) * 255 / pas), (byte)((i - pas * 7) * 255 / pas), (byte)((i - pas * 7) * 255 / pas));
                    }
                    else
                    {
                        fractale[x, y] = new Pixel(0, 0, 0);
                    }
                }
            }
            MyImage imagebis = new MyImage(hauteurmax * largeurmax * 3 + 54, largeurmax, hauteurmax, fractale);
            return imagebis;
        }
        /// <summary>
        /// Fonction qui crée l'histogramme d'une image
        /// </summary>
        /// <returns> Retourne une instance de MyImage contenant l'histogramme </returns>
        public MyImage Histogramme()
        {
            int[] valHistoR = new int[256];
            int[] valHistoG = new int[256];
            int[] valHistoB = new int[256];
            //On compte le nombre d'occurences de chaque valeur du sous-pixel
            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    valHistoR[image[i, j].R]++;
                    valHistoG[image[i, j].G]++;
                    valHistoB[image[i, j].B]++;
                }
            }
            //On définit la hauteur de l'histogramme avec la valeur la plus haute
            int hauteurHisto = 0;
            for (int i = 0; i < 256; i++)
            {
                if (hauteurHisto < valHistoR[i]) { hauteurHisto = valHistoR[i]; }
                if (hauteurHisto < valHistoG[i]) { hauteurHisto = valHistoG[i]; }
                if (hauteurHisto < valHistoB[i]) { hauteurHisto = valHistoB[i]; }
            }
            Pixel deepskyblue = new Pixel(0, 191, 255);
            Pixel rouge = new Pixel(255, 0, 0);
            Pixel vert = new Pixel(0, 255, 0);
            Pixel bleu = new Pixel(0, 0, 255);
            Pixel[,] histogramme = new Pixel[hauteurHisto, 768];
            //On remplit l'arrière plan de l'histogramme
            for (int i = 0; i < hauteurHisto; i++)
            {
                for (int j = 0; j < 768; j++)
                {
                    histogramme[i, j] = deepskyblue;
                }
            }
            //On remplit chaque colonne avec le nombre d'occurences de la valeur du sous-pixel
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < valHistoR[i]; j++)
                {
                    histogramme[j, i] = rouge;
                }
                for (int j = 0; j < valHistoG[i]; j++)
                {
                    histogramme[j, i + 256] = vert;
                }
                for (int j = 0; j < valHistoB[i]; j++)
                {
                    histogramme[j, i + 512] = bleu;
                }
            }
            MyImage imagebis = new MyImage(hauteurHisto * 768 * 3 + 54, 768, hauteurHisto, histogramme);
            return imagebis;
        }
        /// <summary>
        /// Fonction qui cache une image dans une 
        /// </summary>
        /// <param name="image2"> Image que l'on va cacher dans un autre </param>
        /// <returns> Retourne une instance de MyImage contenant l'image cachée dans une autre </returns>
        public MyImage CoderImage(MyImage image2)
        {
            //On redimensionne l'image que l'on va cacher si elle est plus grande que l'image de base
            double ratioLargeur = (double)hauteur / (double)image2.Hauteur;
            double ratioHauteur = (double)largeur / (double)image2.Largeur;
            if (ratioHauteur < ratioLargeur)
            {
                image2 = image2.Redimensionnement(ratioHauteur);
            }
            else
            {
                image2 = image2.Redimensionnement(ratioLargeur);
            }
            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    //On convertit en binaire la valeur de chaque sous-pixel de l'image de base
                    int[] tabBleu1 = ConvertByteEnBase2(image[i, j].B);
                    int[] tabVert1 = ConvertByteEnBase2(image[i, j].G);
                    int[] tabRouge1 = ConvertByteEnBase2(image[i, j].R);
                    int[] tabBleu2;
                    int[] tabVert2;
                    int[] tabRouge2;
                    //On fait de même avec l'image que l'on va cacher, mais vu qu'elle est redimensionnée proportionnellement on prend du blanc si on sort de l'image
                    if (i >= image2.Hauteur || j >= image2.Largeur)
                    {
                        tabBleu2 = ConvertByteEnBase2(255);
                        tabVert2 = ConvertByteEnBase2(255);
                        tabRouge2 = ConvertByteEnBase2(255);
                    }
                    else
                    {
                        tabBleu2 = ConvertByteEnBase2(image2.image[i, j].B);
                        tabVert2 = ConvertByteEnBase2(image2.image[i, j].G);
                        tabRouge2 = ConvertByteEnBase2(image2.image[i, j].R);
                    }
                    int[] tabBleu3 = new int[8];
                    int[] tabVert3 = new int[8];
                    int[] tabRouge3 = new int[8];
                    //On stock les 4 bits de poids forts de l'image de base au début du tableau
                    for (int k = 0; k < 4; k++)
                    {
                        tabBleu3[k] = tabBleu1[k];
                        tabVert3[k] = tabVert1[k];
                        tabRouge3[k] = tabRouge1[k];
                    }
                    //On stock les 4 bits de poids forts de l'image que l'on veut cacher à la fin du tableau
                    int cpt = 0;
                    for (int l = 4; l < 8; l++)
                    {
                        tabBleu3[l] = tabBleu2[cpt];
                        tabVert3[l] = tabVert2[cpt];
                        tabRouge3[l] = tabRouge2[cpt];
                        cpt++;
                    }
                    //On convertit le binaire en byte pour chaque sous-pixel 
                    image[i, j].B = ConvertBase2EnByte(tabBleu3);
                    image[i, j].G = ConvertBase2EnByte(tabVert3);
                    image[i, j].R = ConvertBase2EnByte(tabRouge3);
                }
            }
            MyImage imagebis = new MyImage(this.taille, this.largeur, this.hauteur, image);
            return imagebis;
        }
        /// <summary>
        /// Fonction qui décode une image cachée dans une autre
        /// </summary>
        /// <returns> Retourne une instance de MyImage contenant l'image décodée </returns>
        public MyImage DecoderImage()
        {
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    //On récupère la valeur de chaque sous-pixel et on la convertit en binaire
                    int[] tabBleu1 = ConvertByteEnBase2(image[i, j].B);
                    int[] tabVert1 = ConvertByteEnBase2(image[i, j].G);
                    int[] tabRouge1 = ConvertByteEnBase2(image[i, j].R);
                    int[] tabBleu2 = new int[8];
                    int[] tabVert2 = new int[8];
                    int[] tabRouge2 = new int[8];
                    //On stock les 4 derniers bits dans les bits de poids forts (début du tableau) et on compléte avec des 0
                    for (int k = 0; k < 4; k++)
                    {
                        tabBleu2[k] = tabBleu1[k + 4];
                        tabVert2[k] = tabVert1[k + 4];
                        tabRouge2[k] = tabRouge1[k + 4];
                        tabBleu2[k + 4] = 0;
                        tabVert2[k + 4] = 0;
                        tabRouge2[k + 4] = 0;
                    }
                    //On reconvertit le binaire en byte
                    image[i, j].B = ConvertBase2EnByte(tabBleu2);
                    image[i, j].G = ConvertBase2EnByte(tabVert2);
                    image[i, j].R = ConvertBase2EnByte(tabRouge2);
                }
            }
            MyImage imagebis = new MyImage(this.taille, this.largeur, this.hauteur, image);
            return imagebis;
        }
        #endregion
        /////////////////////////////////
        ///////Conversion de bases///////
        /////////////////////////////////
        #region
        /// <summary>
        /// Fonction qui convertit des données en little endian en entier
        /// </summary>
        /// <param name="tab"> tableau contenant les valeurs en little endian </param>
        /// <returns> Retourne la valeur convertit en entier </returns>
        static int Convertir_Endian_To_Int(byte[] tab)
        {
            int conv = 0;
            for (int i = 0; i < tab.Length; i++)
            {
                conv += tab[i] * Convert.ToInt32(Math.Pow(256, i));
            }
            return conv;
        }
        /// <summary>
        /// Fonction qui convertit des données en entier en little endian
        /// </summary>
        /// <param name="val"> valeur en entier à convertir </param>
        /// <param name="taille"> taille du tableau de byte de sortie </param>
        /// <returns> Retourne la valeur convertit en little endian </returns>
        static byte[] Convertir_Int_To_Endian(int val, int taille)
        {
            byte[] conv = new byte[taille];
            for (int i = 0; i < taille; i++)
            {
                int div = val / Convert.ToInt32(Math.Pow(256, taille - i - 1));
                val = val % Convert.ToInt32(Math.Pow(256, taille - i - 1));
                conv[taille - i - 1] = Convert.ToByte(div);
            }
            return conv;
        }
        /// <summary>
        /// Fonction qui convertit la matrice de Pixels en tableau de bytes
        /// </summary>
        /// <returns> Retourne le tableau de bytes contenant les valeurs des pixels rouge, vert et bleu </returns>
        public byte[] MatToTab()
        {
            byte[] tableau = new byte[4 * image.Length];
            int compteur = 0;
            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    tableau[compteur] = image[i, j].B;
                    tableau[compteur + 1] = image[i, j].G;
                    tableau[compteur + 2] = image[i, j].R;
                    tableau[compteur + 3] = 0;
                    compteur += 4;
                }
            }
            return tableau;
        }
        /// <summary>
        /// Fonction qui convertit un byte en binaire
        /// </summary>
        /// <param name="b"> byte que l'on veut convertir </param>
        /// <returns> Retourne la valeur du byte en binaire </returns>
        public int[] ConvertByteEnBase2(byte b)
        {
            int valeur = Convert.ToInt32(b);
            int[] tab = new int[8];
            for (int i = 0; i < tab.Length; i++)
            {
                tab[7 - i] = valeur % 2;
                valeur = valeur / 2;
            }
            return tab;
        }
        /// <summary>
        /// Fonction qui covnertit une valeur en binaire en byte
        /// </summary>
        /// <param name="tab"> tableau qui contient la valeur en binaire </param>
        /// <returns> Retourne la valeur en byte du binaire </returns>
        public byte ConvertBase2EnByte(int[] tab)
        {
            int valeur = 0;
            for (int i = 0; i < tab.Length; i++)
            {
                valeur = valeur + (Convert.ToInt32(Math.Pow(2, 7 - i))) * tab[i];
            }
            byte b = (byte)valeur;
            return b;
        }
        #endregion
        ///////////////////////////////////////////
        ///////QR Code Version 1 & 2 (TD6-7)///////
        ///////////////////////////////////////////
        #region
            //-----------------------------//
            ////Generation chaine QR code////
            //-----------------------------//
            #region
        /// <summary>
        /// Méthode principale de la génération de QR code qui contient toutes les autres sous fonctions 
        /// </summary>
        /// <param name="text"> Le message qu'on voudra convertir </param>
        /// <returns> Une liste de byte qui contiendra toute la chaine de bits </returns>
        public List<byte> GenerationQRchaine(string text)
        {
            //Choix Version
            int version = 0;
            if (text.Length <= 25){version = 1;}
            else{version = 2;}
            //Génération de la liste qui va contenir le QRcode
            List<byte> QRcode = new List<byte>();
            //Génération de l'indicateur du mode
            byte[] IndicMode = new byte[4] { 0, 0, 1, 0 };
            AddTabtoList(QRcode, IndicMode);
            //Génération de l'indicateur du nb de caractère
            IndicNbCarac(QRcode, text);
            //Conversion des données 
            ConvertDonnee(QRcode, text);
            //Ajout des derniers 0
            TerminaisonAnd8(QRcode, version);
            return QRcode;
        }
        /// <summary>
        /// Une simple méthode qui ajoute un tableau de byte à une liste de byte
        /// </summary>
        /// <param name="Liste"> La liste qui recevra le tableau de byte </param>
        /// <param name="tab"> Le tableau de byte qu'on ajoutera à la liste </param>
        public void AddTabtoList(List<byte> Liste, byte[] tab)
        {
            for (int i = 0; i < tab.Length; i++)
            {
                Liste.Add(tab[i]);
            }
        }
        /// <summary>
        /// Methode qui va encoder les données du message 
        /// </summary>
        /// <param name="Liste"> On ajoutera les données à cette liste </param>
        /// <param name="text"> message qui se fera encoder </param>
        public void ConvertDonnee(List<byte> Liste, string text)
        {
            // Séparer les mots par deux
            int taille = 0;
            if (text.Length % 2 == 0)
            {
                taille = text.Length / 2;
            }
            else
            {
                taille = text.Length / 2 + 1;
            }
            string[] Tab2Lettres = new string[taille];
            int[] TabInt = new int[taille];
            // Récupère les différents caractères pour les mettre dans le tableau
            for (int i = 0; i < taille; i++)
            {
                if ((text.Length % 2 == 1) && i == taille - 1)
                {
                    Tab2Lettres[i] = text.Substring(2 * i, 1);
                }
                else
                {
                    Tab2Lettres[i] = text.Substring(2 * i, 2);
                }
                // Les convertir avec la norme dans un tableau de int
                char c1 = Convert.ToChar(Tab2Lettres[i][0]);
                if (Tab2Lettres[i].Length == 2)
                {
                    char c2 = Convert.ToChar(Tab2Lettres[i][1]);
                    TabInt[i] = 45 * TranslateCharToInt(c1) + TranslateCharToInt(c2);
                    byte[] tab = ConvertIntToBi(TabInt[i], 11);
                    AddTabtoList(Liste, tab);
                }
                else
                {
                    TabInt[i] = TranslateCharToInt(c1);
                    byte[] tab = ConvertIntToBi(TabInt[i], 6);
                    AddTabtoList(Liste, tab);
                }
            }
        }
        /// <summary>
        /// Methode qui va décrypter les données encodées 
        /// </summary>
        /// <param name="chaine"> La liste qui contient la chaine de bits </param>
        /// <param name="NbCarac"> Le nombre de caractère que contient le message </param>
        /// <returns> Le message que le QRcode contient </returns>
        public string DeconvertDonnee(List<byte> chaine, int NbCarac)
        {
            string message = "";
            int c = 0;
            //On retranscrit les données deux par deux comme défini par la norme
            byte[] tabtempo11 = new byte[11];
            for (int i = 0; i < 11 * (NbCarac / 2); i++)
            {
                tabtempo11[c] = chaine[i + 13];
                c++;
                if (c == 11)
                {
                    c = 0;
                    int value = ConvertBiToInt(tabtempo11);
                    int c1 = value / 45;
                    int c2 = value % 45;
                    message += TranslateIntToChar(c1);
                    message += TranslateIntToChar(c2);
                }
            }
            if (NbCarac % 2 != 0)
            {
                byte[] tabtempo6 = new byte[6];
                for (int i = 11 * (NbCarac / 2); i < 11 * (NbCarac / 2) + 6; i++)
                {
                    tabtempo6[c] = chaine[i + 13];
                    c++;
                    if (c == 6)
                    {
                        int value = ConvertBiToInt(tabtempo6);
                        message += TranslateIntToChar(value);
                    }
                }
            }
            return message;
        }
        /// <summary>
        /// Un simple tableau pour se retrouver dans l'encodage du message, on retrouvera chaque lettre correspondante à l'aide de son index
        /// </summary>
        /// <returns> Le tableau en question </returns>
        public char[] TableauTranslate()
        {
            char[] dico = new char[45];
            for (int i = 0; i < 10; i++)
            {
                dico[i] = (char)i;
            }
            for (int i = 0; i < 26; i++)
            {
                dico[i + 10] = (char)(65 + i);
            }
            dico[36] = ' ';
            dico[37] = '$';
            dico[38] = '%';
            dico[39] = '*';
            dico[40] = '+';
            dico[41] = '-';
            dico[42] = '.';
            dico[43] = '/';
            dico[44] = ':';
            return dico;
        }
        /// <summary>
        /// Methode qui va permettre à l'aide du tableau de traduction de traduire un char en int en renvoyant son index
        /// </summary>
        /// <param name="c"> Le caractère à traduire </param>
        /// <returns> index du caractère </returns>
        public int TranslateCharToInt(char c)
        {
            int value = -1;
            char[] tab = TableauTranslate();
            c = Char.ToUpper(c);
            for (int i = 0; i < 45; i++)
            {
                if (tab[i] == c){value = i;}
            }
            return value;
        }
        /// <summary>
        /// Methode qui va traduire un int en char à l'aide du tableauTranslate via son index
        /// </summary>
        /// <param name="value"> valeur à traduire mais aussi index </param>
        /// <returns> valeur du tableau à tel index </returns>
        public char TranslateIntToChar(int value)
        {
            return TableauTranslate()[value];
        }
        /// <summary>
        /// Methode qui va convertir un binaire en entier
        /// </summary>
        /// <param name="tab"> binaire représenté par un tableau de byte </param>
        /// <returns> l'entier traduit du binaire </returns>
        public int ConvertBiToInt(byte[] tab)
        {
            int conv = 0;
            for (int i = 0; i < tab.Length; i++)
            {
                conv += tab[tab.Length - 1 - i] * Convert.ToInt32(Math.Pow(2, i));
            }
            return conv;
        }
        /// <summary>
        /// Methode qui va convertir un entier en binaire representé par un tableau de byte
        /// </summary>
        /// <param name="val"> entier à traduire </param>
        /// <param name="taille"> taille de la chaine de binaire qu'on voudra retourner </param>
        /// <returns> binaire retourné representé par un tableau de byte </returns>
        public byte[] ConvertIntToBi(int val, int taille)
        {
            byte[] conv = new byte[taille];
            for (int i = 0; i < taille; i++)
            {
                int div = val / Convert.ToInt32(Math.Pow(2, taille - i - 1));
                val = val % Convert.ToInt32(Math.Pow(2, taille - i - 1));
                conv[i] = Convert.ToByte(div);
            }
            return conv;
        }
        /// <summary>
        /// Methode qui va ajouter à une liste l'indicateur du nombre de caractère
        /// </summary>
        /// <param name="Liste"> liste qui contient la chaîne de données en bits </param>
        /// <param name="mess"> message crypté par le QR code</param>
        public void IndicNbCarac(List<byte> Liste, string mess)
        {
            int taille = mess.Length;
            byte[] tab = ConvertIntToBi(taille, 9);
            AddTabtoList(Liste, tab);
        }
        /// <summary>
        /// Methode qui va s'occuper d'ajouter à notre liste la terminaison + les bits qu'il faut pour en faire un multiple de 8 
        /// + ajout du pattern pour remplir le reste + code de la correction d'erreur
        /// </summary>
        /// <param name="Liste"> liste qui se verra ajouter tous les éléments ci dessus </param>
        /// <param name="version"> version du qrcode car la taille varie en fonction de la varie </param>
        public void TerminaisonAnd8(List<byte> Liste, int version)
        {
            // Partie terminaison
            int taille = Liste.Count;
            int tailleTotale = 0;
            if (version == 1)
            {
                tailleTotale = 152;
            }
            else
            {
                tailleTotale = 272;
            }
            int diff = tailleTotale - taille;
            int taillebit = 0;
            if (diff < 4)
            {
                taillebit = diff;
            }
            else if (diff >= 4)
            {
                taillebit = 4;
            }
            byte[] terminaison = ConvertIntToBi(0, taillebit);
            AddTabtoList(Liste, terminaison);
            // Partie 8
            taille = Liste.Count;
            diff = taille % 8;
            if (diff != 0) {
                byte[] eight = ConvertIntToBi(0, 8 - diff);
                AddTabtoList(Liste, eight);
            } 
            // Remplir le reste
            taille = Liste.Count;
            diff = tailleTotale - taille;
            diff = diff / 8;
            byte[] bytenull1 = new byte[8] { 1, 1, 1, 0, 1, 1, 0, 0 };
            byte[] bytenull2 = new byte[8] { 0, 0, 0, 1, 0, 0, 0, 1 };
            for (int i = 0; i < diff; i++)
            {
                if (i % 2 == 0)
                {
                    AddTabtoList(Liste, bytenull1);
                }
                else
                {
                    AddTabtoList(Liste, bytenull2);
                }
            }
            // Partie correction d'erreur
            int placedonnee = 19;
            int placecorrection = 7;
            if (version == 2)
            {
                placedonnee = 34;
                placecorrection = 10;
            }
            byte[] bytetab = new byte[placedonnee];
            // On convertit chaque octet pour les placer dans le bytetab
            for (int i = 0; i < placedonnee; i++)
            {
                byte[] binaire = new byte[8];
                for (int j = 0; j < 8; j++)
                {
                    binaire[j] = Liste[i * 8 + j];
                }
                bytetab[i] = Convert.ToByte(ConvertBiToInt(binaire));
            }
            // On utilise l'algorithme fourni pour renvoyer un tableau de byte
            byte[] result = ReedSolomonAlgorithm.Encode(bytetab, placecorrection, ErrorCorrectionCodeType.QRCode);
            // On retranscrit chaque bits de chaque octet dans le tableau ce et on l'ajoute à la liste
            byte[] ce = new byte[placecorrection * 8];
            for (int i = 0; i < result.Length; i++)
            {
                byte[] tempo = ConvertIntToBi(result[i], 8);
                for (int j = 0; j < 8; j++)
                {
                    ce[8 * i + j] = tempo[j];
                }
            }
            AddTabtoList(Liste, ce);
            // Pour les codes de version 2 : il manque des résidus de correction d'erreur qu'on remplacera par des 0. 
            // Ces données seront dans la chaine de bits mais n'aura aucun effet dans l'écriture du QRcode si c'est une version 1
            AddTabtoList(Liste, new byte[7] { 0, 0, 0, 0, 0, 0, 0 });
        }
        #endregion
            //------------------//
            ////Dessin QR code////
            //------------------//
            #region
        /// <summary>
        /// Methode qui va permettre de dessiner un module avec la couleur de notre choix
        /// </summary>
        /// <param name="moduleLength"> taille du module afin de pouvoir colorier chaque pixel </param>
        /// <param name="i"> coordonnée en abcisee du module </param>
        /// <param name="j"> coordonnée en ordonnée du module </param>
        /// <param name="color"> la couleur de notre choix </param>
        /// <param name="qrCode"> la matrice de pixels du QR code </param>
        public void DessinQRmodule(int moduleLength, int i, int j, Pixel color, Pixel[,] qrCode)
        {
            for (int l = i * moduleLength; l < (i + 1) * moduleLength; l++)
            {
                for (int c = j * moduleLength; c < (j + 1) * moduleLength; c++)
                {
                    qrCode[l, c] = color;
                }
            }
        }
        /// <summary>
        /// Methode qui vérifie si le module correspondant est "libre" càd s'il n'est ni bleu ni noir ni blanc
        /// </summary>
        /// <param name="moduleLength"> la taille du module en pixel </param>
        /// <param name="i"> coordonnée en abscisse </param>
        /// <param name="j"> coordonnée en ordonnée </param>
        /// <param name="qrCode">la matrice de pixels du QR code </param>
        /// <returns> booléen qui est vraie si le module est libre </returns>
        public bool Testmodule(int moduleLength, int i, int j, Pixel[,]qrCode)
        {
            bool free = true;
            int l = i * moduleLength;
            int c = j * moduleLength;
            if ((qrCode[l, c].R == 0 && qrCode[l, c].G == 0 && qrCode[l, c].B == 0) || (qrCode[l, c].R == 255 && qrCode[l, c].G == 255 && qrCode[l, c].B == 255) || qrCode[l, c].B == 255)
            {
                free = false;
            }
            return free;
        }
        /// <summary>
        /// Méthode qui va dessiner les données encodées sur le QR code
        /// </summary>
        /// <param name="taille_version"> la taille du qrcode en terme de module </param>
        /// <param name="moduleLength"> la taille d'un module </param>
        /// <param name="chaine"> la liste qui contient les données encodées </param>
        /// <param name="qrCode">la matrice de pixels du QR code</param>
        public void ParcourirQR(int taille_version, int moduleLength, List<byte> chaine, Pixel[,]qrCode)
        {
            Pixel blanc = new Pixel(255, 255, 255);
            Pixel noir = new Pixel(0, 0, 0);
            int total = taille_version / 2;
            for (int colonne = 0; colonne < total; colonne++)
            {
                int c = taille_version - 2 - 2 * colonne;
                bool decal = false;
                // Nous décalons lorsque c <= 6 car il y a une colonne qu'il ne faut pas prendre en compte
                if (c <= 6)
                {
                    decal = true;
                }
                // Afin de gérer le sens de l'écriture, on va tester la parité des colonnes
                if (colonne % 2 == 0)
                {
                    for (int i = 0; i < taille_version; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            c = taille_version - 1 - 2 * colonne - j;
                            //Si on a passé la colonne vide, on reculera les coordonnées en absisse de chaque colonne étudiée
                            if (decal)
                            {
                                c--;
                            }
                            //On teste si le moduke est occupé
                            if (Testmodule(moduleLength, taille_version - 1 - i, c, qrCode))
                            {
                                //Condition pour le masque 0 où les couleurs sont permutées
                                if ((taille_version - 1 - i + c) % 2 == 0)
                                {
                                    if (chaine[0] == 0)
                                    {
                                        DessinQRmodule(moduleLength, taille_version - 1 - i, c, noir, qrCode);
                                    }
                                    else
                                    {
                                        DessinQRmodule(moduleLength, taille_version - 1 - i, c, blanc, qrCode);
                                    }
                                }
                                else
                                {
                                    if (chaine[0] == 0)
                                    {
                                        DessinQRmodule(moduleLength, taille_version - 1 - i, c, blanc, qrCode);
                                    }
                                    else
                                    {
                                        DessinQRmodule(moduleLength, taille_version - 1 - i, c, noir, qrCode);
                                    }
                                }
                                //On enlève le caractère de la liste
                                chaine.RemoveAt(0);
                            }
                        }
                    }
                }
                //Même raisonnment mais avec sens de lecture inversé
                else
                {
                    for (int i = 0; i < taille_version; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            c = taille_version - 1 - 2 * colonne - j;
                            if (decal)
                            {
                                c--;
                            }
                            if (Testmodule(moduleLength, i, c, qrCode))
                            {
                                if ((i + c) % 2 == 0)
                                {
                                    if (chaine[0] == 0)
                                    {
                                        DessinQRmodule(moduleLength, i, c, noir, qrCode);
                                    }
                                    else
                                    {
                                        DessinQRmodule(moduleLength, i, c, blanc, qrCode);
                                    }
                                }
                                else
                                {
                                    if (chaine[0] == 0)
                                    {
                                        DessinQRmodule(moduleLength, i, c, blanc, qrCode);
                                    }
                                    else
                                    {
                                        DessinQRmodule(moduleLength, i, c, noir, qrCode);
                                    }
                                }
                                chaine.RemoveAt(0);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Methode principale qui réunira toutes les autres méthodes afin de pouvoir dessiner le QRcode 
        /// </summary>
        /// <param name="message"> message que l'on veut encoder dans un QR Code</param>
        /// <returns>Retourne l'instance de MyImage contenant le QR code</returns>
        public MyImage DessinQRcode(string message)
        {
            // Quelques initialisations pour préparer la suite
            Pixel blanc = new Pixel(255, 255, 255);
            Pixel noir = new Pixel(0, 0, 0);
            int moduleLength = 32;
            // On génére déjà la chaine de bits
            List<byte> QRcode = GenerationQRchaine(message);
            // On choisit la version
            int version = 1;
            if (message.Length > 25)
            {
                version = 2;
            }
            int taille_version = 21;
            if (version == 2)
            {
                taille_version = 25;
            }
            // On initialise la matrice de pixels
            int largeurQR = moduleLength * taille_version;
            int hauteurQR = moduleLength * taille_version;
            Pixel[,] qrCode = new Pixel[hauteurQR, largeurQR];
            int tailleRecherche = 7 * moduleLength;
            for (int i = 0; i < hauteurQR; i++)
            {
                for (int j = 0; j < largeurQR; j++)
                {
                    qrCode[i, j] = new Pixel(125, 125, 125);
                }
            }
            // Motif de recherche 
            for (int i = 0; i < tailleRecherche; i++)
            {
                for (int j = 0; j < tailleRecherche; j++)
                {
                    // C'est une condition très longue mais simple : Nous cherchons à dessiner en blanc le deuxième carré en partant des côtés et il
                    // faut prendre en compte les différentes exceptions
                    if ((i >= moduleLength && j >= moduleLength && (i <= 2 * moduleLength || j <= 2 * moduleLength)) && (i <= tailleRecherche - 1 - moduleLength && j <= tailleRecherche - 1 - moduleLength) || (i <= tailleRecherche - 1 - moduleLength && j <= tailleRecherche - 1 - moduleLength) && (i >= tailleRecherche - 1 - 2 * moduleLength || j >= tailleRecherche - 1 - 2 * moduleLength) && (i >= moduleLength) && (j >= moduleLength))
                    {
                        qrCode[i, j] = blanc;
                        qrCode[i, largeurQR - 1 - j] = blanc;
                        qrCode[hauteurQR - 1 - i, j] = blanc;
                    }
                    else
                    {
                        qrCode[i, j] = noir;
                        qrCode[i, largeurQR - 1 - j] = noir;
                        qrCode[hauteurQR - 1 - i, j] = noir;
                    }
                }
            }
            // Séparateur
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (i >= 7)
                    {
                        DessinQRmodule(moduleLength, i, j, blanc, qrCode);
                        DessinQRmodule(moduleLength, i, taille_version - 1 - j, blanc, qrCode);
                        DessinQRmodule(moduleLength, taille_version - 1 - i, j, blanc, qrCode);
                    }
                    if (j >= 7)
                    {
                        DessinQRmodule(moduleLength, i, j, blanc, qrCode);
                        DessinQRmodule(moduleLength, i, taille_version - 1 - j, blanc, qrCode);
                        DessinQRmodule(moduleLength, taille_version - 1 - i, j, blanc, qrCode);
                    }
                }
            }
            // Motif d'alignement 
            if (version == 2)
            {
                tailleRecherche = 5;
                for (int i = 0; i < tailleRecherche; i++)
                {
                    for (int j = 0; j < tailleRecherche; j++)
                    {
                        if (i >= 4 || i < 1 || j >= 4 || j < 1)
                        {
                            DessinQRmodule(moduleLength, i + 16, j + 16, noir, qrCode);
                        }
                        else
                        {
                            DessinQRmodule(moduleLength, i + 16, j + 16, blanc, qrCode);
                        }
                    }
                }
                DessinQRmodule(moduleLength, 18, 18, noir, qrCode);
            }
            // Motif de syncro
            int diff = 5;
            if (version == 2)
            {
                diff = 9;
            }
            bool black = true;
            for (int i = 0; i < diff; i++)
            {
                if (black)
                {
                    DessinQRmodule(moduleLength, 8 + i, 6, noir, qrCode);
                    DessinQRmodule(moduleLength, 6, 8 + i, noir, qrCode);
                    black = false;
                }
                else
                {
                    DessinQRmodule(moduleLength, 8 + i, 6, blanc, qrCode);
                    DessinQRmodule(moduleLength, 6, 8 + i, blanc, qrCode);
                    black = true;
                }
            }
            // Motif noir 
            DessinQRmodule(moduleLength, (4 * version) + 9, 8, noir, qrCode);
            // Code pour le masque 0
            byte[] mask = new byte[15] { 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0 };
            int a = 0;
            for (int i = 0; i < 9; i++)
            {
                if (i != 6)
                {
                    if (mask[a] == 0)
                    {
                        DessinQRmodule(moduleLength, 8, i, blanc, qrCode);
                    }
                    else
                    {
                        DessinQRmodule(moduleLength, 8, i, blanc, qrCode);
                    }
                    a++;
                }
            }
            int b = 8;
            int c = 7;
            for (int i = 0; i < 8; i++)
            {
                if (i != 1)
                {
                    if (mask[b] == 0)
                    {
                        DessinQRmodule(moduleLength, 7 - i, 8, blanc, qrCode);
                    }
                    else
                    {
                        DessinQRmodule(moduleLength, 7 - i, 8, noir, qrCode);
                    }
                    b++;
                }
                if (mask[c] == 0)
                {
                    DessinQRmodule(moduleLength, 8, taille_version - 8 + i, blanc, qrCode);
                }
                else
                {
                    DessinQRmodule(moduleLength, 8, taille_version - 8 + i, noir, qrCode);
                }
                c++;
            }
            for (int i = 0; i < 7; i++)
            {
                if (mask[i] == 0)
                {
                    DessinQRmodule(moduleLength, taille_version - 1 - i, 8, blanc, qrCode);
                }
                else
                {
                    DessinQRmodule(moduleLength, taille_version - 1 - i, 8, noir, qrCode);
                }
            }
            // Bits de données 
            ParcourirQR(taille_version, moduleLength, QRcode, qrCode);
            MyImage qrCodeFinal = new MyImage(hauteurQR * largeurQR * 3 + 54, largeurQR, hauteurQR, qrCode);
            return qrCodeFinal;
        }
        #endregion
            //---------------------//
            ////Décrypter QR code////
            //---------------------//
            #region
        /// <summary>
        /// Méthode qui permet de capturer la couleur d'un module, il renvoie un 1 s'il est noir, un 0 s'il est blanc et un 2 si c'est autre
        /// </summary>
        /// <param name="moduleLength"> taille du module </param>
        /// <param name="i"> coordonnée en abscisse </param>
        /// <param name="j"> coordonnée en ordonnée </param>
        /// <returns> le bit correspondant à la couleur du module </returns>
        public byte DetectionModule(int moduleLength, int i, int j)
        {
            byte value = 2;
            int l = i * moduleLength + 2;
            int c = j * moduleLength + 2;
            if (image[l, c].R == 0 && image[l, c].G == 0 && image[l, c].B == 0)
            {
                value = 1;
            }
            else if (image[l, c].R == 255 && image[l, c].G == 255 && image[l, c].B == 255)
            {
                value = 0;
            }
            else
            {
                value = 2;
            }
            return value;
        }
        /// <summary>
        /// Méthode qui permet de de vérifier si un carré correspond à un module de recherche
        /// </summary>
        /// <param name="moduleLength"> taille du module </param>
        /// <param name="i"> coordonnée en abscisse </param>
        /// <param name="j"> coorodnnée en ordonnée </param>
        /// <returns> un booléen qui dit si oui on non le carré correspond à un module </returns>
        public bool DetectionRecherche(int moduleLength, int i, int j)
        {
            int[,] Recherche = new int[7, 7] { { 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 0, 0, 1 }, { 1, 0, 1, 1, 1, 0, 1 }, { 1, 0, 1, 1, 1, 0, 1 }, { 1, 0, 1, 1, 1, 0, 1 }, { 1, 0, 0, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1, 1, 1 } };
            bool finded = true;
            for (int l = 0; l < 7; l++)
            {
                for (int c = 0; c < 7; c++)
                {
                    if (DetectionModule(moduleLength, l + i, c + j) != Recherche[l, c])
                    {
                        finded = false;
                    }
                }
            }
            return finded;
        }
        /// <summary>
        /// Méthode qui va permettre de colorier tous les patterns en bleu et ne laisser que le message visible afin de faciliter la lecture de celui ci
        /// Elle va aussi permettre de récupérer les données du masque
        /// </summary>
        /// <param name="moduleLength"> taille d'un module </param>
        /// <param name="taille_version"> taille du qr code en module </param>
        /// <returns> Les données du masque </returns>
        public byte[] CacherPatterns(int moduleLength, int taille_version)
        {
            // Modules de recherche
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    DessinQRmodule(moduleLength, i, j, new Pixel(0, 0, 125),image);
                    DessinQRmodule(moduleLength, i, taille_version - 8 + j, new Pixel(0, 0, 125),image);
                    DessinQRmodule(moduleLength, taille_version - 8 + i, j, new Pixel(0, 0, 125),image);
                }
            }
            // Timing Patterns
            for (int i = 0; i < taille_version; i++)
            {
                DessinQRmodule(moduleLength, 6, i, new Pixel(0, 0, 125),image);
                DessinQRmodule(moduleLength, i, 6, new Pixel(0, 0, 125),image);
            }
            // Point noir
            DessinQRmodule(moduleLength, taille_version - 8, 8, new Pixel(0, 0, 125),image);
            // Module d'alignement
            if (taille_version == 25)
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        DessinQRmodule(moduleLength, 16 + i, 16 + j, new Pixel(0, 0, 125),image);
                    }
                }
            }
            // Masque
            byte[] mask = new byte[15];
            int c = 0;
            int k = 8;
            for (int i = 0; i < 9; i++)
            {
                if (i != 6)
                {
                    mask[c] = DetectionModule(moduleLength, 8, i);
                    DessinQRmodule(moduleLength, 8, i, new Pixel(0, 0, 125),image);
                    c++;
                }
            }
            for (int i = 0; i < 8; i++)
            {
                if (i != 1)
                {
                    mask[k] = DetectionModule(moduleLength, 7 - i, 8);
                    DessinQRmodule(moduleLength, 7 - i, 8, new Pixel(0, 0, 125),image);
                    k++;
                }
                DessinQRmodule(moduleLength, 8, taille_version - 1 - i, new Pixel(0, 0, 125),image);
            }
            for (int i = 0; i < 7; i++)
            {
                DessinQRmodule(moduleLength, taille_version - 1 - i, 8, new Pixel(0, 0, 125),image);
            }
            return mask;
        }
        /// <summary>
        /// Méthode qui va maintenant récupérer la chaine de bits pour les données contenu par le qrcode
        /// Simple variation de la méthode ParcourirQR
        /// </summary>
        /// <param name="taille_version"> taille du qr code en terme de module </param>
        /// <param name="moduleLength"> taille du module </param>
        /// <returns> une liste de byte qui correspond à la chaine de bits </returns>
        public List<byte> Detection_messageQR(int taille_version, int moduleLength)
        {
            int total = taille_version / 2;
            List<byte> chaine = new List<byte>();
            Demask(moduleLength, taille_version);
            for (int colonne = 0; colonne < total; colonne++)
            {
                int c = taille_version - 2 - 2 * colonne;
                bool decal = false;
                if (c <= 6)
                {
                    decal = true;
                }
                if (colonne % 2 == 0)
                {
                    for (int i = 0; i < taille_version; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            c = taille_version - 1 - 2 * colonne - j;
                            if (decal)
                            {
                                c--;
                            }
                            if (DetectionModule(moduleLength, taille_version - 1 - i, c) == 0)
                            {
                                chaine.Add(0);
                            }
                            else if (DetectionModule(moduleLength, taille_version - 1 - i, c) == 1)
                            {
                                chaine.Add(1);
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < taille_version; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            c = taille_version - 1 - 2 * colonne - j;
                            if (decal)
                            {
                                c--;
                            }
                            if (DetectionModule(moduleLength, i, c) == 0)
                            {
                                chaine.Add(0);
                            }
                            else if (DetectionModule(moduleLength, i, c) == 1)
                            {
                                chaine.Add(1);
                            }
                        }
                    }
                }
            }
            return chaine;
        }
        /// <summary>
        /// Méthode qui va permettre d'enlever le masque 0 afin de faciliter la lecture du message
        /// </summary>
        /// <param name="moduleLength"> taille d'un module </param>
        /// <param name="taille_version"> taille d'un qrcode en terme de module </param>
        public void Demask(int moduleLength, int taille_version)
        {
            for (int i = 0; i < taille_version; i++)
            {
                for (int j = 0; j < taille_version; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        if (DetectionModule(moduleLength, i, j) == 0)
                        {
                            DessinQRmodule(moduleLength, i, j, new Pixel(0, 0, 0),image);
                        }
                        else if (DetectionModule(moduleLength, i, j) == 1)
                        {
                            DessinQRmodule(moduleLength, i, j, new Pixel(255, 255, 255),image);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Méthode principale pour le décryptage de QR code
        /// </summary>
        /// <returns> le message crypté du QR code </returns>
        public string DetectionQR()
        {
            int version = 0;
            if (hauteur == largeur && hauteur % 21 == 0)
            {
                version = 1;
            }
            else if (hauteur == largeur && hauteur % 25 == 0)
            {
                version = 2;
            }
            int taille_version = 21;
            int moduleLength = hauteur / 21;
            if (version == 2)
            {
                taille_version = 25;
                moduleLength = hauteur / 25;
            }
            // Lecture du message
            byte[] mask = CacherPatterns(moduleLength, taille_version);
            List<byte> QRmessage = Detection_messageQR(taille_version, moduleLength);
            // Description Mode
            byte[] IndicMode = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                IndicMode[i] = QRmessage[i];
            }
            // Description Nbre caractère
            byte[] IndicCarac = new byte[9];
            for (int i = 0; i < 9; i++)
            {
                IndicCarac[i] = QRmessage[i + 4];
            }
            int NbCarac = ConvertBiToInt(IndicCarac);
            // Conversion des données 
            string message = DeconvertDonnee(QRmessage, NbCarac);
            return message;
        }
            #endregion
        #endregion
    }
}