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
            while (row < _myImage.height)
            {
                int column = 0;
                while (column < _myImage.width)
                {
                    _myImage.image[row, column].GrayScale();
                    column++;
                }
                row++;
            }
            _myImage.CreateNewImage(0); // Rien à modifier dans le header
        }


        /// <summary>
        /// Traitez chaque pixel de la matrice en appliquant la méthode de classe pour appliquer la binarisation de l'image (noir ou blanc)
        /// </summary>
        public void BinarizePicture()
        {
            int row = 0;
            while (row < _myImage.height)
            {
                int column = 0;
                while (column < _myImage.width)
                {
                    _myImage.image[row, column].BinarizeColor();
                    column++;
                }
                row++;
            }
            _myImage.CreateNewImage(0); // Rien à modifier dans le header
        }
        
        /// <summary>
        /// Demande à l'utilisateur de choisir un angle de rotation (90, 180 ou 270°) et effectue la rotation de l'image en conséquence.
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
                _myImage.CreateNewImage(0); // Rien à modifier dans le header
            }
            else
            {
                _myImage.CreateNewImage(2); // Besoin d'intervertir la width et height dans le header
            }
        }

        /// <summary>
        /// Effectue une rotation de 90° de l'image en créant une matrice temporaire de la même taille que l'image mais en inversant les dimensions.
        /// </summary>
        private void rotation90()
        {
            Pixel[,] imageTemporaire = new Pixel[_myImage.width, _myImage.height];
            int row = 0;
            while (row < _myImage.height)
            {
                int column = 0;
                while (column < _myImage.width)
                {
                    imageTemporaire[column, _myImage.image.GetLength(0) - 1 - row] = _myImage.image[row, column];
                    column++;
                }
                row++;
            }
            _myImage.image = imageTemporaire;
        }
        
        /// <summary>
        /// Demande à l'utilisateur de choisir un facteur d'agrandissement et effectue l'agrandissement de l'image en conséquence.
        /// </summary>
        public void moreBigPicture()
        {
            int multiple = 0;
            bool possible = false;

            while (!possible)
            {
                Console.Write("Par combien voulez vous agrandir votre image : ");
                try
                {
                    multiple = Convert.ToInt32(Console.ReadLine());
                    possible = true;
                }
                catch
                {
                    Console.Write("\nVeuillez entrer un entier\n");
                }
            }

            Pixel[,] bigPicture = new Pixel[_myImage.height * multiple, _myImage.width * multiple];

            int row = 0;
            while (row < bigPicture.GetLength(0))
            {
                int column = 0;
                while (column < bigPicture.GetLength(1))
                {
                    bigPicture[row, column] = _myImage.image[row / multiple, column / multiple];
                    column++;
                }
                row++;
            }

            _myImage.image = bigPicture;
            _myImage.CreateNewImage(3, multiple); // width et height à modifier dans le header + tableau byte plus grand
        }

        /// <summary>
        /// Applique le filtre choisis à l'image parmis ces filtres : contour, renforcement, flou, repoussage.
        /// </summary>
        /// <param name="filter"></param>
        public void filter(String filter)    
        {
            int[,] convolution = null;
            bool isFlue = false;

            if (filter == "contour")
            {
            //  int[,] contour1 = { { 1, 0, -1 }, { 0, 0, 0 }, { -1, 0, 1 } };
                int[,] contour2 = { { 0, 1, 0 }, { 1, -4, 1 }, { 0, 1, 0 } };
            //  int[,] contour3 = { { -1, -1, -1 }, { -1, 8, -1 }, { -1, -1, -1 } };
                convolution = contour2;
            }
            else if (filter == "renforcement")
            {
                int[,] renforcement = { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
                convolution = renforcement;
            }
            else if (filter == "flou")
            {
                int[,] flou = { { 1 , 1, 1}, { 1 , 1 , 1}, { 1 , 1, 1} };
                convolution = flou;
                isFlue = true;
            }
            else if (filter == "repoussage")
            {
                int[,] repoussage = { { -2, -1, 0 }, { -1, 1, 1 }, { 0, 1, 2 } };
                convolution = repoussage;
            }

            convolutionMatrix(convolution, isFlue);
        }

        /// <summary>
        /// Applique la matrice de convolution sur l'image pour appliquer un filtre.
        /// </summary>
        /// <param name="convolution"> Matrice de convolution à appliquer sur l'image </param>
        /// <param name="isFlou"> Flag pour savoir si on applique un flou ou non </param>
        private void convolutionMatrix(int[,] convolution, bool isFlou)
        {
            Pixel[,] imageTemporaire = new Pixel[_myImage.height, _myImage.width];
            int i = 1;
            while (i < _myImage.height - 1)
            {
                int j = 1;
                while (j < _myImage.width - 1)
                {
                    int valueBlue = _myImage.image[i - 1, j - 1].Blue * convolution[0, 0] + _myImage.image[i - 1, j].Blue * convolution[0, 1] + _myImage.image[i - 1, j + 1].Blue * convolution[0, 2] +
                                    _myImage.image[i, j - 1].Blue * convolution[1, 0] + _myImage.image[i, j].Blue * convolution[1, 1] + _myImage.image[i, j + 1].Blue * convolution[1, 2] +
                                    _myImage.image[i + 1, j - 1].Blue * convolution[2, 0] + _myImage.image[i + 1, j].Blue * convolution[2, 1] + _myImage.image[i + 1, j + 1].Blue * convolution[2, 2];


                    int valueGreen = _myImage.image[i - 1, j - 1].Green * convolution[0, 0] + _myImage.image[i - 1, j].Green * convolution[0, 1] + _myImage.image[i - 1, j + 1].Green * convolution[0, 2] +
                                    _myImage.image[i, j - 1].Green * convolution[1, 0] + _myImage.image[i, j].Green * convolution[1, 1] + _myImage.image[i, j + 1].Green * convolution[1, 2] +
                                    _myImage.image[i + 1, j - 1].Green * convolution[2, 0] + _myImage.image[i + 1, j].Green * convolution[2, 1] + _myImage.image[i + 1, j + 1].Green * convolution[2, 2];


                    int valueRed = _myImage.image[i - 1, j - 1].Red * convolution[0, 0] + _myImage.image[i - 1, j].Red * convolution[0, 1] + _myImage.image[i - 1, j + 1].Red * convolution[0, 2] +
                                    _myImage.image[i, j - 1].Red * convolution[1, 0] + _myImage.image[i, j].Red * convolution[1, 1] + _myImage.image[i, j + 1].Red * convolution[1, 2] +
                                    _myImage.image[i + 1, j - 1].Red * convolution[2, 0] + _myImage.image[i + 1, j].Red * convolution[2, 1] + _myImage.image[i + 1, j + 1].Red * convolution[2, 2];

                    if (isFlou)
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
                    j++;
                }
                i++;
            }

            imageTemporaire[0, 0] = imageTemporaire[1, 1]; // coin supérieur gauche
            imageTemporaire[0, _myImage.width - 1] = imageTemporaire[1, _myImage.width - 2]; // coin supérieur droit
            imageTemporaire[_myImage.height - 1, 0] = imageTemporaire[_myImage.height - 2, 1]; // coin inférieur gauche
            imageTemporaire[_myImage.height - 1, _myImage.width - 1] = imageTemporaire[_myImage.height - 2, _myImage.width - 2]; // coin inférieur droit

            int row = 1;
            while (row < _myImage.height - 1)
            {
                imageTemporaire[row, 0] = imageTemporaire[row, 1]; // bord gauche
                imageTemporaire[row, _myImage.width - 1] = imageTemporaire[row, _myImage.width - 2]; // bord droit
                row++;
            }

            int column = 1;
            while (column < _myImage.width - 1)
            {
                imageTemporaire[0, column] = imageTemporaire[1, column]; // bord supérieur
                imageTemporaire[_myImage.height - 1, column] = imageTemporaire[_myImage.height - 2, column]; // inférieur
                column++;
            }

            _myImage.image = imageTemporaire;
            _myImage.CreateNewImage(0); // Rien à modifier dans le header
        }

        /// <summary>
        /// Permets de convertir un entier en tableau d'entier de dimension 8
        /// Chaque élément du tableau correspond à un bit de l'entier
        /// </summary>
        /// <param name="number"> number à convertir </param>
        /// <returns> tableau d'entier de dimension 8 </returns>
        public int[] Convertir_Int_ToByte(int number)
        {
            int rest;
            int[] octet = new int[8];

            int i = 0;
            while (number != 0 && i < 8)
            {
                if (number == 0)
                {
                    octet[i] = 0;
                }
                rest = number % 2;
                octet[i] = rest;
                number = (number - rest) / 2;

                i+=1;
            }
            return octet;
        }

        /// <summary>
        /// Permets de convertir un octet en entier
        /// Chaque élément du tableau correspond à un bit de l'entier
        /// </summary>
        /// <param name="octet"> octet à convertir en entier </param>
        /// <returns> retourne un entier </returns>
        public int Convertir_Byte_ToInt(int[] octet)
        {
            int number = 0;
            for (int i = 0; i < 8; i++)
            {
                number += octet[i] * (int)Math.Pow(2, i);
                i++;
            }
            return number;
        }

        /// <summary>
        /// Applique l'algorithme de Mandelbrot pour générer une fractale
        /// </summary>
        public void fractale()
        {
            // Toujours situé entre -2.1 et 0.6 sur l'axe des abscisses et entre -1.2 et 1.2 sur l'axe des ordonnées, l'ensemble de Mandelbrot...
            double x1 = -2.1;
            double x2 = 0.6;
            double y1 = -1.2;
            double y2 = 1.2;
            int zoom = 100;
            int iterationMax = 50;

            int imageX = Convert.ToInt32((x2 - x1) * zoom);
            _myImage.width = imageX;
            int imageY = Convert.ToInt32((y2 - y1) * zoom);
            _myImage.height = imageY;

            Pixel[,] fractale = new Pixel[imageX, imageY];

            int x = 0;
            while (x < imageX)
            {
                int y = 0;
                while (y < imageY)
                {
                    double c_r = (x / zoom) + x1;
                    double c_i = (y / zoom) + y1;
                    double z_r = 0;
                    double z_i = 0;
                    int i = 0;

                    while ((z_r * z_r + z_i * z_i) < 4 && i < iterationMax)
                    {
                        double temporaire = z_r;
                        z_r = z_r * z_r - z_i * z_i + c_r;
                        z_i = 2 * z_i * temporaire + c_i;
                        i++;
                    }

                    if (i == iterationMax)
                    {
                        fractale[x, y] = new Pixel(0, 0, 0);
                    }
                    else
                    {
                        fractale[x, y] = new Pixel(0, 0, (i * 255) / iterationMax);
                    }
                    y++;
                }
                x++;
            }
            _myImage.image = fractale;
            _myImage.CreateNewImage(5); // Creer l'image entierement
        }

        /// <summary>
        /// Permet à l'utilisateur de choisir un fichier image en saisissant son nom.
        /// </summary>
        /// <returns>Le nom du fichier sélectionné.</returns>
        static string ChooseFile()
        {
            string filename = null;
            byte[] exist = null;

            while (exist == null)
            {
                Console.Write("Entrer le nom du fichier : ");
                string fichier = Console.ReadLine();
                filename = fichier + ".bmp";

                try
                {
                    exist = File.ReadAllBytes(filename);
                }
                catch
                {
                    Console.WriteLine("L'image que vous avez saisis n'existe pas.");
                }
            }

            return filename;
        }

        /// <summary>
        /// Permet à l'utilisateur de choisir une image à dissimuler dans l'image actuelle.
        /// </summary>
        /// <returns></returns>
        private string addSecondImage()
        {
            Console.WriteLine("Choisissez une image à dissimuler qui soit plus petite que celle que vous aviez sélectionner\n" +
                "\n0. Enter un nom de fichier" +
                "\n1. Jamel" +
                "\n2. Test");

            int pictureNumber = -1;
            string filename = null;

            while (pictureNumber != 0 && pictureNumber != 1 && pictureNumber != 2 && pictureNumber != 3 && pictureNumber != 4 && pictureNumber != 5)
            {
                Console.Write("> ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out pictureNumber))
                {
                    if (pictureNumber == 0)
                    {
                        filename = ChooseFile();
                    }
                    else if (pictureNumber == 1)
                    {
                        filename = "jamel.bmp";
                    }
                    else if (pictureNumber == 2)
                    {
                        filename = "test.bmp";
                    }
                    else
                    {
                        Console.WriteLine("Votre numéro est invalide\n");
                    }
                }
                else
                {
                    Console.WriteLine("Votre numéro est invalide\n");
                }
            }
            return filename;
        }

        /// <summary>
        /// Coder et Décoder une image dans une image (stéganographie) 
        /// </summary>
        public void steganography()
        {
            Pixel[,] imageTemporaire = new Pixel[_myImage.Height, _myImage.Width];
            MyImage aDissimuler = null;

            while (true)
            {
                aDissimuler = new MyImage(addSecondImage());
                if (aDissimuler.Height <= _myImage.Height && aDissimuler.Width <= _myImage.Width)
                    break;
                Console.WriteLine("L'image sélectionnée est plus grande que l'image principale. Sélectionnez une image plus petite.");
            }

            Pixel[,] image2 = aDissimuler.Mat_Pixel;

            int fixHeight = (_myImage.Height - aDissimuler.Height) / 2;
            int fixWidth = (_myImage.Width - aDissimuler.Width) / 2;

            int row = 0;
            int column = 0;

            while (row < _myImage.Height)
            {
                column = 0;
                while (column < _myImage.Width)
                {
                    if (row >= fixHeight && row < (_myImage.Height - fixHeight) && column >= fixWidth && column < (_myImage.Width - fixWidth))
                    {
                        int[] binaire1_Blue = Convertir_Int_ToByte(_myImage.image[row, column].Blue);
                        int[] binaire2_Blue = Convertir_Int_ToByte(image2[row - fixHeight, column - fixWidth].Blue);
                        int[] newBinary_Blue = new int[8];
                        for (int i = 0; i < 8; i++)
                        {
                            if (i <= 3)
                            {
                                newBinary_Blue[i] = binaire2_Blue[4 + i];
                            }
                            else
                            {
                                newBinary_Blue[i] = binaire1_Blue[i];
                            }
                        }
                        int valueBlue = Convert.ToInt32(Convertir_Byte_ToInt(newBinary_Blue));

                        int[] binaire1_Green = Convertir_Int_ToByte(_myImage.image[row, column].Green);
                        int[] binaire2_Green = Convertir_Int_ToByte(image2[row - fixHeight, column - fixWidth].Green);
                        int[] newBinary_Green = new int[8];
                        for (int i = 0; i < 8; i++)
                        {
                            if (i <= 3)
                            {
                                newBinary_Green[i] = binaire2_Green[4 + i];
                            }
                            else
                            {
                                newBinary_Green[i] = binaire1_Green[i];
                            }
                        }
                        int valueGreen = Convert.ToInt32(Convertir_Byte_ToInt(newBinary_Green));

                        int[] binaire1_Red = Convertir_Int_ToByte(_myImage.image[row, column].Red);
                        int[] binaire2_Red = Convertir_Int_ToByte(image2[row - fixHeight, column - fixWidth].Red);
                        int[] newBinary_Red = new int[8];
                        for (int i = 0; i < 8; i++)
                        {
                            if (i <= 3)
                            {
                                newBinary_Red[i] = binaire2_Red[4 + i];
                            }
                            else
                            {
                                newBinary_Red[i] = binaire1_Red[i];
                            }
                        }
                        int valueRed = Convertir_Byte_ToInt(newBinary_Red);

                        imageTemporaire[row, column] = new Pixel(valueBlue, valueGreen, valueRed);
                    }
                    else
                    {
                        imageTemporaire[row, column] = _myImage.image[row, column];
                    }
                    column++;
                }
                row++;
            }

            _myImage.image = imageTemporaire;
            _myImage.CreateNewImage(0);
        }


        //Implémentation des algorithmes de compression QuadTree ou Huffman
        
    }
}