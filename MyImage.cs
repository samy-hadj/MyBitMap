using System;
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
        // * ATTRIBUTS *

        public string nom;
        public string typeImage;
        public int tailleFichier;
        public int tailleOffset;
        public int largeur;
        public int hauteur;
        public int nb_Bit_Couleur;
        public int offset; // index
        public byte[] header = new byte[54];
        public byte[] myfile; // Contient le header

        public Pixel[,] image; // 
        public ImageProcessService imageProcessService;


        
        // * CONSTRUCTEUR *

        /// <summary>
        /// Récupère les informations concernant l'image bit par bit qu'elle transmet dans le tableau myfile.
        /// Puis parcours myfile afin de récupérer les attributs
        /// </summary>
        /// <param name="filename"> On récupère l'image à partir de son nom </param>
        public MyImage(string filename)
        {
            imageProcessService = new ImageProcessService(this);
            From_Image_To_File(filename);
            nom = filename;

            // Récupérer le header : i : 0 à 53

            for (int i = 0; i < 54; i++)
            {
                header[i] = myfile[i];
            }

            // typeImage : i = 0 et 1
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

            // tailleFichier : i = 2 à 5
            offset = 2;
            tailleFichier = Convertir_Endian_To_Int(4);


                // INFORMATIONS CONCERNANT L'IMAGE

            // taille offset : i = 14 à 17
            offset = 14;
            tailleOffset = Convertir_Endian_To_Int(4);

            // largeur : i = 18 et 21
            offset = 18;
            largeur = Convertir_Endian_To_Int(4);

            // hauteur : i = 22 et 25
            offset = 22;
            hauteur = Convertir_Endian_To_Int(4);

            // nb_Bit_Couleur : i = 28 et 29
            offset = 28;
            nb_Bit_Couleur = Convertir_Endian_To_Int(2);
            

                // RECUPERER IMAGE -> transformation en matrice de pixels
            
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
        /// Vide dans le cas où on veut dessiner et donc on a rien à récupérer, tout à créer
        /// </summary>
        public MyImage()
        {
            
        }

        
        // * ACCESSEUR *

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

        /// <summary>
        /// Pour dissimuler une image dans une autre il faut récupérer sa matrice de pixel
        /// </summary>
        public Pixel[,] Mat_Pixel
        {
            get { return image; }
        }

        
        // * METHODE *

        public void From_Image_To_File(string filename)
        {
            myfile = File.ReadAllBytes(filename);
        }

        public int Convertir_Endian_To_Int(int nbOctet, int fin = 0, int valeur = 0)
        {
            if(fin == nbOctet)
            {
                return valeur;
            }
            else
            {
                valeur += Convert.ToInt32(myfile[offset + fin] * Math.Pow(256, fin));
                fin++;
                return Convertir_Endian_To_Int(nbOctet, fin, valeur);
            }

        }

        public byte[] Convertir_Int_To_Endian(int nombre)  // Récupéré sur internet
        {
            byte[] endian = new byte[4];
            endian[0] = (byte)nombre;
            endian[1] = (byte)(((uint)nombre >> 8) & 0xFF);
            endian[2] = (byte)(((uint)nombre >> 16) & 0xFF);
            endian[3] = (byte)(((uint)nombre >> 24) & 0xFF);
            return endian;
        }
    }
}