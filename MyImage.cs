﻿using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace MyBitMap
{
    public class MyImage
    {
        public string nom; // name of the image
        public string typeImage; // type of picture
        public int tailleFichier; // file size
        public int tailleOffset; // offset size
        public int largeur; // width
        public int hauteur; // height
        public int nb_Bit_Couleur; //number of bits per pixel
        public int offset; // index
        public byte[] header = new byte[54]; // header content
        public byte[] myfile; // header content + image content

        public Pixel[,] image; // matrix of pixels
        public ImageProcessService imageProcessService;

                public string Nom
        {
            get { return nom; }
        }

        public string TypeImage
        {
            get { return typeImage; }
        }

        public int TailleFichier
        {
            get { return tailleFichier; }
        }

        public int TailleOffset
        {
            get { return tailleOffset; }
        }

        public int Largeur
        {
            get { return largeur; }
        }

        public int Hauteur
        {
            get { return hauteur; }
        }

        public int NbBitCouleurs
        {
            get { return nb_Bit_Couleur; }
        }

        public Pixel[,] Mat_Pixel
        {
            get { return image; }
        }


        /// <summary>
        /// Extrait les informations de l'image, bit par bit, et les stocke dans le tableau myfile.
        /// Ensuite, parcourt ce tableau pour récupérer les attributs.
        /// </summary>
        /// <param name="filename"> A partir du filename on récupère l'image </param>
        public MyImage(string filename)
        {
            imageProcessService = new ImageProcessService(this);
            From_Image_To_File(filename);
            nom = filename;
            GetHeader();
            GetTypeImage();
            GetSizeFile();
            GetInformationsImage();
            getImageMatrix();
        }

        /// <summary>
        /// Consutructeur vide (pour les fractales)
        /// </summary>
        public MyImage()
        {
            imageProcessService = new ImageProcessService(this);
        }

        /// <summary>
        /// Récupère les 54 premiers bits du fichier pour les stocker dans le header.
        /// </summary>
        private void GetHeader()
        {
            for (int i = 0; i < 54; i++)
            {
                header[i] = myfile[i];
            }
        }

        /// <summary>
        /// Récupère le type de l'image.
        /// </summary>
        private void GetTypeImage()
        {
            for (offset = 0; offset < 2; offset++)
            {
                if (myfile[offset] == 65) { typeImage += "A"; }
                if (myfile[offset] == 66) { typeImage += "B"; }
                if (myfile[offset] == 67) { typeImage += "C"; }
                if (myfile[offset] == 73) { typeImage += "I"; }
                if (myfile[offset] == 77) { typeImage += "M"; }
                if (myfile[offset] == 80) { typeImage += "P"; }
                if (myfile[offset] == 84) { typeImage += "T"; }
            }
        }

        /// <summary>
        /// Récupère la taille du fichier.
        /// </summary>
        private void GetSizeFile()
        {
            offset = 2;
            tailleFichier = Convertir_Endian_To_Int(4);
        }

        /// <summary>
        /// Récupère les informations de l'image.
        /// </summary>
        private void GetInformationsImage()
        {
            offset = 14;
            tailleOffset = Convertir_Endian_To_Int(4);

            offset = 18;
            largeur = Convertir_Endian_To_Int(4);

            offset = 22;
            hauteur = Convertir_Endian_To_Int(4);

            offset = 28;
            nb_Bit_Couleur = Convertir_Endian_To_Int(2);
        }


        /// <summary>
        /// Récupère les pixels de l'image.
        /// </summary>
        private void getImageMatrix()
        {
            offset = 54;
            image = new Pixel[hauteur, largeur];

            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    image[i, j] = new Pixel(myfile[offset], myfile[offset + 1], myfile[offset + 2]);
                    offset += 3;
                }
            }
        }

        /// <summary>
        /// Convertit l'image en tableau de byte
        /// </summary>
        /// <param name="filename"> Nom de l'image </param>
        /// <example> From_Image_To_File("image.bmp") </example>
        public void From_Image_To_File(string filename)
        {
            myfile = File.ReadAllBytes(filename);
        }

        /// <summary>
        /// Convertit le tableau de byte en image
        /// </summary>
        /// <param name="nbOctet"> Nombre d'octet à convertir </param>
        /// <param name="fin"> Index de fin de la conversion </param>
        /// <param name="valeur"> Valeur de l'octet converti </param>
        /// <example> Convertir_Endian_To_Int(4) </example>
        public int Convertir_Endian_To_Int(int nbOctet, int fin = 0, int valeur = 0)
        {
            for (int i = fin; i < nbOctet; i++)
            {
                valeur += Convert.ToInt32(myfile[offset + i] * Math.Pow(256, i));
            }
            return valeur;
        }

        /// <summary>
        /// Convertit un entier en endian
        /// </summary>
        /// <param name="nombre"> Nombre à convertir en "endian" </param>
        /// <example> Convertir_Int_To_Endian(5) </example>
        public byte[] Convertir_Int_To_Endian(int nombre)  // Récupéré sur internet
        {
            byte[] endian = new byte[4];
            endian[0] = (byte)nombre;
            endian[1] = (byte)(((uint)nombre >> 8) & 0xFF);
            endian[2] = (byte)(((uint)nombre >> 16) & 0xFF);
            endian[3] = (byte)(((uint)nombre >> 24) & 0xFF);
            return endian;
        }

        /// <summary>
        /// Commence par ajuster le header en fonction du traitement effectué.
        /// Ensuite, remplit entièrement le nouveau tableau de bytes en copiant d'abord les 54 premiers bits du header, puis les valeurs de bleu, vert et rouge pour le reste du tableau.
        /// Enfin, écrit, crée et affiche l'image.
        /// </summary>
        /// <param name="operation">Opération à effectuer sur le header en fonction du traitement.</param>
        /// <param name="multiple">Facteur d'agrandissement ou de rétrécissement de l'image.</param>

        public void CreerImage(int operation, int multiple = 0)
        {
            byte[] creerFile = null; // Même principe que le tableau de byte myfile
            byte[] temporaire = new byte[4];

            switch (operation)
            {
                case 0: // nuance gris ; noir et blanc ; rotation 180 ; convolution ; insérer image dans une autre
                    creerFile = new byte[tailleFichier];
                    break;
                case 2: // Rotation 90° ou 270°
                    creerFile = new byte[tailleFichier];
                    for (int i = 0; i <= 3; i++)
                    {
                    // Permuter dans le header, la largeur et hauteur
                        temporaire[i] = header[22 + i];
                        header[22 + i] = header[18 + i];
                        header[18 + i] = temporaire[i];
                    }
                    break;
                case 3: // Agrandir
                    creerFile = new byte[((largeur * multiple) * (hauteur * multiple) * 3) + 54]; // Multiplier par 3 pour les 3 couleurs primaires
                    temporaire = Convertir_Int_To_Endian(largeur * multiple);
                    for (int i = 0; i <= 3; i++)
                    {
                        header[18 + i] = temporaire[i];
                    }
                    temporaire = Convertir_Int_To_Endian(hauteur * multiple);
                    for (int i = 0; i <= 3; i++)
                    {
                        header[22 + i] = temporaire[i];
                    }
                    break;
                case 4: // Rétrécir
                    creerFile = new byte[((largeur / multiple) * (hauteur / multiple) * 3) + 54]; // Multiplier par 3 pour les 3 couleurs primaires
                    temporaire = Convertir_Int_To_Endian(largeur / multiple);
                    for (int i = 0; i <= 3; i++)
                    {
                        header[18 + i] = temporaire[i];
                    }
                    temporaire = Convertir_Int_To_Endian(hauteur / multiple);
                    for (int i = 0; i <= 3; i++)
                    {
                        header[22 + i] = temporaire[i];
                    }
                    break;
                case 5: // Fractale
                    tailleFichier = (largeur * hauteur * 3) + 54;
                    creerFile = new byte[tailleFichier];

                    header[0] = 66; // B
                    header[1] = 77; // M

                    temporaire = Convertir_Int_To_Endian(tailleFichier);
                    for (int i = 0; i <= 3; i++) // taille fichier
                    {
                        header[2 + i] = temporaire[i];
                    }

                    for (int i = 0; i <= 3; i++)
                    {
                        header[6 + i] = 0;
                    }

                    header[10] = 54; // taille header

                    for (int i = 0; i <= 2; i++)
                    {
                        header[11 + i] = 0;
                    }

                    header[14] = 40; // taille offset

                    for (int i = 0; i <= 2; i++)
                    {
                        header[15 + i] = 0; // taille offset
                    }

                    temporaire = Convertir_Int_To_Endian(largeur);
                    for (int i = 0; i <= 3; i++)
                    {
                        header[18 + i] = temporaire[i]; // largeur
                    }

                    temporaire = Convertir_Int_To_Endian(hauteur);
                    for (int i = 0; i <= 3; i++)
                    {
                        header[22 + i] = temporaire[i]; // hauteur
                    }

                    header[26] = 1;
                    header[27] = 0;
                    header[28] = 24; // nb bit par couleur + (i = 29)

                    for (int i = 0; i <= 24; i++)
                    {
                        header[29 + i] = 0;
                    }
                    break;
            }

            for (int offset = 0; offset < 54; offset++)
            {
                creerFile[offset] = header[offset];
            }

            int ajustOffset = 0;

                for (int ligne = 0; ligne < image.GetLength(0); ligne++)
                {
                    for (int colonne = 0; colonne < image.GetLength(1); colonne++)
                    {
                        creerFile[54 + ajustOffset] = Convert.ToByte(image[ligne, colonne].Blue);
                        creerFile[55 + ajustOffset] = Convert.ToByte(image[ligne, colonne].Green);
                        creerFile[56 + ajustOffset] = Convert.ToByte(image[ligne, colonne].Red);
                        ajustOffset += 3;
                    }
                }
    
            Console.WriteLine("Voulez-vous donner un nom à votre image ? (oui/non)");
            string reponse = Console.ReadLine();
            if (reponse == "oui")
            {
                Console.Write("Nom de l'image : ");
                nom = Console.ReadLine() + ".bmp";
            }
            else
            {
                nom = "nouvel_image" + ".bmp";
            }

            File.WriteAllBytes(nom,creerFile);
            // string path = nom + ".bmp";
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = nom,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}