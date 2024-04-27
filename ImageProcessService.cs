using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace MyBitMap
{
    public class ImageProcessService
    {
        // * ATTRIBUTS *
        private MyImage _myImage;

        public ImageProcessService(MyImage myImage)
        {
            _myImage = myImage;
        }

        /// <summary>
        /// Traitez chaque pixel de la matrice en appliquant la méthode de classe pour appliquer la nuance de gris
        /// </summary>
        public void grayScalePicture()
        {
            int row = 0;
            while (row < _myImage.hauteur)
            {
                int column = 0;
                while (column < _myImage.largeur)
                {
                    _myImage.image[row, column].GrayScale();
                    column++;
                }
                row++;
            }
            _myImage.CreerImage(0); // Rien à modifier dans le header
        }


        /// <summary>
        /// Traitez chaque pixel de la matrice en appliquant la méthode de classe pour appliquer la binarisation de l'image (noir ou blanc)
        /// </summary>
        public void BinarizePicture()
        {
            int row = 0;
            while (row < _myImage.hauteur)
            {
                int column = 0;
                while (column < _myImage.largeur)
                {
                    _myImage.image[row, column].BinarizeColor();
                    column++;
                }
                row++;
            }
            _myImage.CreerImage(0); // Rien à modifier dans le header
        }
        
        /// <summary>
        /// Applique une rotation de 90° de 1 à 3 fois en fonction de ce qui est demandé
        /// Si la rotation demandé est de 180° le header sera identique à celui de l'image avant rotation
        /// </summary>
        public void rotation()
        {
            int angleDegree = 0;
            bool isValidAngle = false;

            while (!isValidAngle)
            {
                Console.Write("Choisissez un angle entre 90, 180 ou 270 : ");
                try
                {
                    angleDegree = Convert.ToInt32(Console.ReadLine());
                    isValidAngle = angleDegree == 90 || angleDegree == 180 || angleDegree == 270;
                    if (!isValidAngle)
                    {
                        Console.WriteLine("\nLa rotation ne peut s'effectuer qu'à 90, 180 ou 270°");
                    }
                }
                catch
                {
                    Console.WriteLine("\nLa rotation ne peut s'effectuer qu'à 90, 180 ou 270°");
                }
            }

            switch (angleDegree)
            {
                case 90:
                    rotation90();
                    break;
                case 180:
                    rotation90();
                    rotation90();
                    break;
                case 270:
                    rotation90();
                    rotation90();
                    rotation90();
                    break;
            }

            if (angleDegree == 180)
            {
                _myImage.CreerImage(0); // Rien à modifier dans le header
            }
            else
            {
                _myImage.CreerImage(2); // Besoin d'intervertir la largeur et hauteur dans le header
            }
        }


        private void rotation90()
        {
            Pixel[,] imageTemporaire = new Pixel[_myImage.largeur, _myImage.hauteur];
            for (int row = 0; row < _myImage.hauteur; row++)
            {
                for (int column = 0; column < _myImage.largeur; column++)
                {
                    imageTemporaire[column, _myImage.image.GetLength(0) - 1 - row] = _myImage.image[row, column];
                }
            }
            _myImage.image = imageTemporaire;
        }
        
        /// <summary>
        /// Créer une matrice temporaire plus grande en multipliant ses dimensions par x, remplit les cases vides à chaque index (grande image) par l'index/x (de l'image réel) ce qui reviendra à reprendre le même pixel plusieurs fois.
        /// L'image étant plus grande le tableau de byte devra être agrandi en conséquence. De plus, la largeur et hauteur dans le header devront être modifiées.
        /// </summary>
        public void agrandir()
        {
            int multiple = 0;
            bool possible;
            do
            {
                possible = true;
                Console.Write("Par combien voulez vous agrandir votre image : ");
                try { multiple = Convert.ToInt32(Console.ReadLine()); }
                catch { Console.Write("\nVeuillez entrer un entier\n"); possible = false; }
            } while (possible == false);
            
            Pixel[,] imageGrande = new Pixel[_myImage.hauteur * multiple, _myImage.largeur * multiple];
            for (int row = 0; row < imageGrande.GetLength(0); row++)
            {
                for (int column = 0; column < imageGrande.GetLength(1); column++)
                {
                    imageGrande[row, column] = _myImage.image[row / multiple, column / multiple];
                }
            }
            _myImage.image = imageGrande;
            _myImage.CreerImage(3, multiple); // Largeur et hauteur à modifier dans le header + tableau byte plus grand
        }

        // * FILTRE *

        /// <summary>
        /// Définit la matrice de convolution en fonction du filtre demandé, par ailleurs si on veut appliquer un filtre flou il faudra alors effectuer une division par 9 qui sera effectuer dans la méthode convolution
        /// </summary>
        public void filtre(String filter)    
        {
            int[,] convolution = null;
            bool estFlou = false;

            switch (filter) // On definit la matrice de convolution en fonction du filtre demandé 
            {
                case "contour":
                    int[,] contour1 = { { 1, 0, -1 }, { 0, 0, 0 }, { -1, 0, 1 } };
                    convolution = contour1;
                    break;
                // case 2:
                //     int[,] contour2 = { { 0, 1, 0 }, { 1, -4, 1 }, { 0, 1, 0 } };
                //     convolution = contour2;
                //     break;
                // case 3:
                //     int[,] contour3 = { { -1, -1, -1 }, { -1, 8, -1 }, { -1, -1, -1 } };
                //     convolution = contour3;
                //     break;
                case "renforcement":
                    int[,] renforcement = { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
                    convolution = renforcement;
                    break;
                case "flou":
                    int[,] flou = { { 1 , 1, 1}, { 1 , 1 , 1}, { 1 , 1, 1} };
                    convolution = flou;
                    estFlou = true;
                    break;
                case "repoussage":
                    int[,] repoussage = { { -2, -1, 0 }, { -1, 1, 1 }, { 0, 1, 2 } };
                    convolution = repoussage;
                    break;
            }
            Convolution(convolution, estFlou);
        }

        /// <summary>
        /// Effectue le calcul permettant d'obtenir les nouvelles valeurs des couleurs bleues, vertes et rouges en appliquant la matrice de convolution à l'image. 
        /// Pour appliquer la matrice de convolution à notre image, il faut accepter de sacrifier les pixels sur les extrémités ce qui va modifier les dimensions de l'image et par conséquent la taille du tableau de bits.
        /// Pour y remédier, les bords vide sont remplit par les pixels du nouveau bord.
        /// </summary>
        /// <param name="convolution"> matrice de convolution défini dans la méthode "filtre" en fonction de l'attente de l'usager </param>
        /// <param name="flou"> Si on veut appliquer un flou alors le bool sera "true" </param>
        private void Convolution(int[,] convolution, bool flou)
        {
            Pixel[,] imageTemporaire = new Pixel[_myImage.hauteur, _myImage.largeur];
            for (int i = 1; i < _myImage.hauteur - 1; i ++)
            {
                for (int j = 1; j < _myImage.largeur - 1; j++)
                {
                    int valueBlue = _myImage.image[i - 1, j - 1].Blue * convolution[0, 0] + _myImage.image[i - 1, j].Blue * convolution[0, 1] + _myImage.image[i - 1, j + 1].Blue * convolution[0, 2] +
                                    _myImage.image[i, j - 1].Blue * convolution[1, 0] + _myImage.image[i, j].Blue * convolution[1, 1] + _myImage.image[i, j + 1].Blue * convolution[1, 2] +
                                    _myImage.image[i + 1, j - 1].Blue * convolution[2, 0] + _myImage.image[i + 1, j].Blue * convolution[2, 1] + _myImage.image[i + 1, j + 1].Blue * convolution[2, 2];
                    

                    int valueGreen = _myImage.image[i - 1, j - 1].Green * convolution[0, 0] + _myImage.image[i - 1, j].Green * convolution[0, 1] + _myImage.image[i - 1, j + 1].Green * convolution[0, 2] +
                                     _myImage.image[i, j - 1].Green * convolution[1, 0] + _myImage.image[i, j].Green * convolution[1, 1] + _myImage.image[i, j + 1].Green * convolution[1, 2] +
                                     _myImage.image[i + 1, j - 1].Green * convolution[2, 0] + _myImage.image[i + 1, j].Green * convolution[2, 1] + _myImage.image[i + 1, j + 1].Green * convolution[2, 2];
                    

                    int valueRed =  _myImage.image[i - 1, j - 1].Red * convolution[0, 0] + _myImage.image[i - 1, j].Red * convolution[0, 1] + _myImage.image[i - 1, j + 1].Red * convolution[0, 2] +
                                    _myImage.image[i, j - 1].Red * convolution[1, 0] + _myImage.image[i, j].Red * convolution[1, 1] + _myImage.image[i, j + 1].Red * convolution[1, 2] +
                                    _myImage.image[i + 1, j - 1].Red * convolution[2, 0] + _myImage.image[i + 1, j].Red * convolution[2, 1] + _myImage.image[i + 1, j + 1].Red * convolution[2, 2];

                    if(flou)
                    {
                        valueBlue = valueBlue / 9;
                        valueGreen = valueGreen / 9;
                        valueRed = valueRed / 9;
                    }

                    if (valueBlue < 0) { valueBlue = 0; }
                    if (valueBlue > 255) { valueBlue = 255; }

                    if (valueGreen < 0) { valueGreen = 0; }
                    if (valueGreen > 255) { valueGreen = 255; }

                    if (valueRed < 0) { valueRed = 0; }
                    if (valueRed > 255) { valueRed = 255; }

                    imageTemporaire[i, j] = new Pixel(valueBlue, valueGreen, valueRed);
                }
            }

            imageTemporaire[0, 0] = imageTemporaire[1, 1]; // coin supérieur gauche
            imageTemporaire[0, _myImage.largeur - 1] = imageTemporaire[1, _myImage.largeur - 2]; // coin supérieur droit
            imageTemporaire[_myImage.hauteur - 1, 0] = imageTemporaire[_myImage.hauteur - 2, 1]; // coin inférieur gauche
            imageTemporaire[_myImage.hauteur - 1, _myImage.largeur - 1] = imageTemporaire[_myImage.hauteur - 2, _myImage.largeur - 2]; // coin inférieur droit

            for (int row = 1; row < _myImage.hauteur - 1; row++) 
            {
                imageTemporaire[row, 0] = imageTemporaire[row, 1]; // bord gauche
                imageTemporaire[row, _myImage.largeur - 1] = imageTemporaire[row, _myImage.largeur - 2]; // bord droit
            }

            for (int column = 1; column < _myImage.largeur - 1; column++)
            {
                imageTemporaire[0, column] = imageTemporaire[1, column]; // bord supérieur
                imageTemporaire[_myImage.hauteur - 1, column] = imageTemporaire[_myImage.hauteur - 2, column]; // inférieur
            }

            _myImage.image = imageTemporaire;
            _myImage.CreerImage(0); // Rien à modifier dans le header
        }

        /// <summary>
        /// Convertit un entier en un octet (tableau d'entier de dimension 8)
        /// </summary>
        /// <param name="nombre"> nombre à convertir </param>
        /// <returns> tableau d'entier de dimension 8 </returns>
        public int[] Convertir_Int_ToByte(int nombre)
        {
            int reste;
            int[] octet = new int [8];
            int i = 0;
            while(nombre != 0 && i < 8)
            {
                if (nombre == 0)
                {
                    octet[i] = 0;
                }
                reste = nombre % 2;
                octet[i] = reste;
                nombre = (nombre - reste) / 2;
                i++;
            }
            return octet;
        }

        /// <summary>
        /// Convertit un octet (tableau d'entier de dimension 8) en un entier
        /// </summary>
        /// <param name="octet"> octet à convertir en entier </param>
        /// <returns> retourne un entier </returns>
        public int Convertir_Byte_ToInt(int[] octet)
        {
            int nombre = 0;
            for (int i = 0; i < 8; i++)
            {
                nombre += Convert.ToInt32(octet[i] * Math.Pow(2, i));
            }
            return nombre;
        }

        // Fractale de Mandelbrot

        /// <summary>
        /// Dessine la fractale de Mandelbrot en noir et bleue
        /// </summary>
        public void fractale()
        {
            // L'ensemble de Mandelbrot est toujours compris entre -2.1 et 0.6 sur l'axe des abscisse et entre -1.2 et 1.2 sur l'axe des ordonnées.
            double x1 = -2.1; // limite gauche
            double x2 = 0.6; // limite droite
            double y1 = -1.2; // limite hausse
            double y2 = 1.2; // limite basse
            int zoom = 100;
            int iterationMax = 50;

            int imageX = Convert.ToInt32((x2 - x1) * zoom);
            _myImage.largeur = imageX;
            int imageY = Convert.ToInt32((y2 - y1) * zoom);
            _myImage.hauteur = imageY;

            Pixel[,] fractale = new Pixel[imageX, imageY];

            for (int x = 0; x < imageX; x++)
            {
                for (int y = 0; y < imageY; y++)
                {
                    double c_r = (x / zoom) + x1;
                    double c_i = (y / zoom) + y1;
                    double z_r = 0; // partie réelle
                    double z_i = 0; // partie imaginaire
                    int i = 0;

                    do
                    {
                        double temporaire = z_r;
                        z_r = z_r * z_r - z_i * z_i + c_r;
                        z_i = 2 * z_i * temporaire + c_i;
                        i++;
                    }
                    while ((z_r * z_r + z_i * z_i) < 4 && i < iterationMax); // évite de calculer le module de z (appliquer des racines), donc on compare à 4 au lieu de 2

                    if (i == iterationMax)
                    {
                        fractale[x, y] = new Pixel(0, 0, 0);
                    }
                    else
                    {
                        fractale[x, y] = new Pixel(0, 0, (i * 255) / iterationMax);
                    }
                }
            }

            _myImage.image = fractale;
            _myImage.CreerImage(5); // Tout le header est à faire
        }
    }
}