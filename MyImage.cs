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
    

        // Créer la nouvelle image

        /// <summary>
        /// Effectue dans un premier temps les modications nécessaire dans le header en fonction du traitement effectué, puis remplit entièrement le nouveau tableau de byte en reprenant le header pour les 54 premiers bits, puis les valeurs des couleurs bleue, vert et rouge pour le reste du tableau.
        /// Finit par écrire l'image, la créer puis l'afficher.
        /// </summary>
        /// <param name="operation"> modification à apporter dans le header selon le traitement à effectuer </param>
        /// <param name="multiple"> agrandissement ou rétrécissement de l'image </param>
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