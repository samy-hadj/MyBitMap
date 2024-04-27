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
        public string nom; // name of the image
        public string typeImage; // type of picture
        public int sizeFile; // file size
        public int offsetSize; // offset size
        public int width; // width
        public int height; // height
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

        public int SizeFile
        {
            get { return sizeFile; }
        }

        public int OffsetSize
        {
            get { return offsetSize; }
        }

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
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
            sizeFile = Convertir_Endian_To_Int(4);
        }

        /// <summary>
        /// Récupère les informations de l'image.
        /// </summary>
        private void GetInformationsImage()
        {
            offset = 14;
            offsetSize = Convertir_Endian_To_Int(4);

            offset = 18;
            width = Convertir_Endian_To_Int(4);

            offset = 22;
            height = Convertir_Endian_To_Int(4);

            offset = 28;
            nb_Bit_Couleur = Convertir_Endian_To_Int(2);
        }


        /// <summary>
        /// Récupère les pixels de l'image.
        /// </summary>
        private void getImageMatrix()
        {
            offset = 54;
            image = new Pixel[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
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

        public void CreateNewImage(int operation, int multiple = 0)
        {
            byte[] createdFile = null;
            byte[] temp = new byte[4];

            switch (operation)
            {
                case 0:
                    createdFile = CreateFileSimple();
                    break;
                case 2:
                    createdFile = CreateFileRotation90or270(temp);
                    break;
                case 3:
                    createdFile = CreateFileMoreBig(multiple, temp);
                    break;
                case 5:
                    createdFile = CreateFileFractale(temp);
                    break;
            }

            CopyHeaderAndImageIntoFile(createdFile);

            string nom = GetNameNewImage();
            SaveAndOpenFile(createdFile, nom);
        }

         /// <summary>
         /// Crée un fichier image simple.
         /// </summary>
        private byte[] CreateFileSimple()
        {
            return new byte[sizeFile];
        }

        /// <summary>
        /// Crée un fichier image en rotation de 90 ou 270 degrés.
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        private byte[] CreateFileRotation90or270(byte[] temp)
        {
            byte[] createdFile = new byte[sizeFile];
            for (int i = 0; i <= 3; i++)
            {
                temp[i] = header[22 + i];
                header[22 + i] = header[18 + i];
                header[18 + i] = temp[i];
            }
            return createdFile;
        }

        /// <summary>
        /// Crée un fichier image plus grand.
        /// </summary>
        /// <param name="multiple"></param>
        /// <param name="temp"></param>
        /// <returns></returns>
        private byte[] CreateFileMoreBig(int multiple, byte[] temp)
        {
            byte[] createdFile = new byte[((width * multiple) * (height * multiple) * 3) + 54];
            temp = Convertir_Int_To_Endian(width * multiple);
            for (int i = 0; i <= 3; i++)
            {
                header[18 + i] = temp[i];
            }
            temp = Convertir_Int_To_Endian(height * multiple);
            for (int i = 0; i <= 3; i++)
            {
                header[22 + i] = temp[i];
            }
            return createdFile;
        }

        /// <summary>
        /// Crée un fichier image fractale.
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        private byte[] CreateFileFractale(byte[] temp)
        {
            sizeFile = (width * height * 3) + 54;
            byte[] createdFile = new byte[sizeFile];

            //.bmp type
            createdFile[0] = 66; // B
            createdFile[1] = 77; // M

            temp = Convertir_Int_To_Endian(sizeFile);
            for (int i = 0; i < 4; i++)
            {
                createdFile[2 + i] = temp[i];
            }

            for (int i = 0; i < 4; i++)
            {
                createdFile[6 + i] = 0;
            }

            createdFile[10] = 54;

            for (int i = 0; i < 3; i++)
            {
                createdFile[11 + i] = 0;
            }

            createdFile[14] = 40;

            for (int i = 0; i < 3; i++)
            {
                createdFile[15 + i] = 0;
            }

            temp = Convertir_Int_To_Endian(width);
            for (int i = 0; i < 4; i++)
            {
                createdFile[18 + i] = temp[i];
            }

            temp = Convertir_Int_To_Endian(height);
            for (int i = 0; i < 4; i++)
            {
                createdFile[22 + i] = temp[i];
            }

            createdFile[26] = 1;
            createdFile[27] = 0;
            createdFile[28] = 24;

            for (int i = 0; i < 24; i++)
            {
                createdFile[29 + i] = 0;
            }

            return createdFile;
        }

        /// <summary>
        /// Copie le header et l'image dans le fichier.
        /// </summary>
        /// <param name="createdFile"></param>

        private void CopyHeaderAndImageIntoFile(byte[] createdFile)
        {
            for (int offset = 0; offset < 54; offset++)
            {
                createdFile[offset] = header[offset];
            }

            int ajustOffset = 0;

            for (int ligne = 0; ligne < image.GetLength(0); ligne++)
            {
                for (int colonne = 0; colonne < image.GetLength(1); colonne++)
                {
                    createdFile[54 + ajustOffset] = Convert.ToByte(image[ligne, colonne].Blue);
                    createdFile[55 + ajustOffset] = Convert.ToByte(image[ligne, colonne].Green);
                    createdFile[56 + ajustOffset] = Convert.ToByte(image[ligne, colonne].Red);
                    ajustOffset += 3;
                }
            }
        }

        /// <summary>
        /// Demande à l'utilisateur s'il veut donner un nom à l'image.
        /// </summary>
        /// <returns></returns>
        private string GetNameNewImage()
        {
            Console.WriteLine("Voulez-vous donner un nom à votre image ? (oui/non)");
            string res = Console.ReadLine();
            if (res == "oui")
            {
                Console.Write("Nom de l'image : ");
                return Console.ReadLine() + ".bmp";
            }
            else
            {
                return "nouvel_image" + ".bmp";
            }
        }

        /// <summary>
        /// Enregistre et ouvre le fichier.
        /// </summary>
        /// <param name="createdFile"></param>
        /// <param name="nom"></param>
        private void SaveAndOpenFile(byte[] createdFile, string nom)
        {
            File.WriteAllBytes(nom, createdFile);
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = nom,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}