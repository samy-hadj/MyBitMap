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

        
        
        // * METHODE TRAITEMENT IMAGE *

        /// <summary>
        /// Parcours la matrice de pixel et applique à chaque Pixel la méthode de classe permettant de créer la nuance de gris
        /// </summary>
        public void enGris()
        {
            for (int ligne = 0; ligne < _myImage.hauteur; ligne++)
            {
                for (int colonne = 0; colonne < _myImage.largeur; colonne++)
                {
                    _myImage.image[ligne, colonne].GrayScale();
                }
            }
            CreerImage(0); // Rien à modifier dans le header
        }

        /// <summary>
        /// Parcours la matrice de pixel et applique à chaque Pixel la méthode de classe pour créer du noir ou blanc
        /// </summary>
        public void NetB()
        {
            for (int ligne = 0; ligne < _myImage.hauteur; ligne++)
            {
                for (int colonne = 0; colonne < _myImage.largeur; colonne++)
                {
                    _myImage.image[ligne, colonne].BinarizeColor();
                }
            }
            CreerImage(0); // Rien à modifier dans le header
        }
        
        /// <summary>
        /// Applique une rotation de 90° de 1 à 3 fois en fonction de ce qui est demandé
        /// Si la rotation demandé est de 180° le header sera identique à celui de l'image avant rotation
        /// </summary>
        public void rotation()
        {
            int angle = 0;
            do
            {
                Console.Write("Choisissez un angle entre 90, 180 ou 270 : ");
                try { angle = Convert.ToInt32(Console.ReadLine()); }
                catch { Console.Write("\nLa rotation ne peut s'effectuer qu'à 90, 180 ou 270°"); }
            } while (angle != 90 && angle != 180 && angle != 270);
            
            if (angle == 90)
            {
                rotation90();
            }
            else if (angle == 180)
            {
                rotation90();
                rotation90();
            }
            else if (angle == 270)
            {
                rotation90();
                rotation90();
                rotation90();
            }

            if (angle == 180)
            {
                CreerImage(0); // Rien à modifier dans le header
            }
            else
            {
                CreerImage(2); // Besoin d'intervertir la largeur et hauteur dans le header
            }
        }

        private void rotation90()
        {
            Pixel[,] imageTemporaire = new Pixel[_myImage.largeur, _myImage.hauteur];
            for (int ligne = 0; ligne < _myImage.hauteur; ligne++)
            {
                for (int colonne = 0; colonne < _myImage.largeur; colonne++)
                {
                    imageTemporaire[colonne, _myImage.image.GetLength(0) - 1 - ligne] = _myImage.image[ligne, colonne];
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
            for (int ligne = 0; ligne < imageGrande.GetLength(0); ligne++)
            {
                for (int colonne = 0; colonne < imageGrande.GetLength(1); colonne++)
                {
                    imageGrande[ligne, colonne] = _myImage.image[ligne / multiple, colonne / multiple];
                }
            }
            _myImage.image = imageGrande;
            CreerImage(3, multiple); // Largeur et hauteur à modifier dans le header + tableau byte plus grand
        }

        /// <summary>
        /// Créer une matrice temporaire plus petite en divisant ses dimensions par x, on remplit les à chaque index (grande image) par l'index*x (de l'image réel) ce qui reviendra à perdre certains pixels.
        /// L'image étant plus petite le tableau de byte devra être réduit en conséquence. De plus, la largeur et hauteur dans le header devront être modifiées.
        /// </summary>
        public void retrecir()
        {
            int diviseur = 0;
            bool possible;
            do
            {
                possible = true;
                Console.Write("Par combien voulez vous rétrécir votre image : ");
                try { diviseur = Convert.ToInt32(Console.ReadLine()); }
                catch { Console.Write("\nVeuillez entrer un entier\n"); possible = false; }
            } while (possible == false || diviseur == 0); // division par 0

            Pixel[,] imagePetite = new Pixel[_myImage.hauteur / diviseur, _myImage.largeur / diviseur];
            for (int ligne = 0; ligne < imagePetite.GetLength(0); ligne++)
            {
                for (int colonne = 0; colonne < imagePetite.GetLength(1); colonne++)
                {
                    imagePetite[ligne, colonne] = _myImage.image[ligne * diviseur, colonne * diviseur];
                }
            }
            _myImage.image = imagePetite;
            CreerImage(4, diviseur); // Largeur et hauteur à modifier dans le header + tableau byte plus petit
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

            for (int ligne = 1; ligne < _myImage.hauteur - 1; ligne++) 
            {
                imageTemporaire[ligne, 0] = imageTemporaire[ligne, 1]; // bord gauche
                imageTemporaire[ligne, _myImage.largeur - 1] = imageTemporaire[ligne, _myImage.largeur - 2]; // bord droit
            }

            for (int colonne = 1; colonne < _myImage.largeur - 1; colonne++)
            {
                imageTemporaire[0, colonne] = imageTemporaire[1, colonne]; // bord supérieur
                imageTemporaire[_myImage.hauteur - 1, colonne] = imageTemporaire[_myImage.hauteur - 2, colonne]; // inférieur
            }

            _myImage.image = imageTemporaire;
            CreerImage(0); // Rien à modifier dans le header
        }


        // * AUTRES *

        /// <summary>
        /// Dissimule une image au centre d'une autre (possèdant de plus grandes dimensions sinon l'image ne sera plus vraiment cachée...).
        /// </summary>
        public void dissimulerImage()
        {
            Pixel[,] imageTemporaire = new Pixel[_myImage.hauteur, _myImage.largeur];
            MyImage aDissimuler = null;
            do
            {
                aDissimuler = new MyImage(Selection2());
            } while (aDissimuler.Hauteur > _myImage.Hauteur && aDissimuler.Largeur > _myImage.Largeur);
            Pixel[,] image2 = aDissimuler.Mat_Pixel; // Récupérer la matrice de pixels de l'image qu'on veut dissimuler

            int ajustHauteur = (_myImage.hauteur - aDissimuler.Hauteur) / 2; 
            int ajustLargeur = (_myImage.largeur - aDissimuler.Largeur) / 2;

            for (int ligne = 0; ligne < _myImage.hauteur; ligne++)
            {
                for (int colonne = 0; colonne < _myImage.largeur; colonne++)
                {
                    if (ligne >= ajustHauteur && ligne < (_myImage.hauteur - ajustHauteur) && colonne >= ajustLargeur && colonne < (_myImage.largeur - ajustLargeur))
                    {
                        int[] binaire1_Blue = Convertir_Int_ToByte(_myImage.image[ligne, colonne].Blue); // On récupère l'octet associé à la couleur bleue à cet emplacement
                        int[] binaire2_Blue = Convertir_Int_ToByte(image2[ligne - ajustHauteur, colonne - ajustLargeur].Blue); // On récupère l'octet associé à la couleur bleue de l'image à dissimuler
                        int[] nouveauBinaire_Blue = new int[8];
                        for (int i = 0; i < 8; i++)
                        {
                            if (i <= 3)
                            {
                                nouveauBinaire_Blue[i] = binaire2_Blue[4 + i]; // Les 4 premiers bits du nouvelle octet correspondent aux 4 derniers de l'image à dissimuler
                            }
                            else
                            {
                                nouveauBinaire_Blue[i] = binaire1_Blue[i]; // Les 4 derniers bits du nouvelle octet correspondent aux 4 premiers de l'image dans laquelle on veut dissimuler
                            }
                        }
                        int valueBlue = Convert.ToInt32(Convertir_Byte_ToInt(nouveauBinaire_Blue));

                        int[] binaire1_Green = Convertir_Int_ToByte(_myImage.image[ligne, colonne].Green);
                        int[] binaire2_Green = Convertir_Int_ToByte(image2[ligne - ajustHauteur, colonne - ajustLargeur].Green);
                        int[] nouveauBinaire_Green = new int[8];
                        for (int i = 0; i < 8; i++)
                        {
                            if (i <= 3)
                            {
                                nouveauBinaire_Green[i] = binaire2_Green[4 + i];
                            }
                            else
                            {
                                nouveauBinaire_Green[i] = binaire1_Green[i];
                            }
                        }
                        int valueGreen = Convert.ToInt32(Convertir_Byte_ToInt(nouveauBinaire_Green));

                        int[] binaire1_Red = Convertir_Int_ToByte(_myImage.image[ligne, colonne].Red);
                        int[] binaire2_Red = Convertir_Int_ToByte(image2[ligne - ajustHauteur, colonne - ajustLargeur].Red);
                        int[] nouveauBinaire_Red = new int[8];
                        for (int i = 0; i < 8; i++)
                        {
                            if (i <= 3)
                            {
                                nouveauBinaire_Red[i] = binaire2_Red[4 + i];
                            }
                            else
                            {
                                nouveauBinaire_Red[i] = binaire1_Red[i];
                            }
                        }
                        int valueRed = Convertir_Byte_ToInt(nouveauBinaire_Red);

                        imageTemporaire[ligne, colonne] = new Pixel(valueBlue, valueGreen, valueRed);
                    }
                    else // Si on ne se situe pas dans la zone où l'on veut dissimuler l'image
                    {
                        imageTemporaire[ligne, colonne] = _myImage.image[ligne, colonne]; 
                    }
                }
            }
            _myImage.image = imageTemporaire;
            CreerImage(0); // Rien à modifier dans le header
        }

        private string Selection2()
        {
            Console.WriteLine("Choisissez une image à dissimuler qui soit plus petite que celle que vous aviez sélectionner" +
                "\n" +
                "\n0. Entrer un nom de fichier" +
                "\n1. Coco" +
                "\n2. Lac en montagne" +
                "\n3. Lena" +
                "\n4. Test\n " +
                "\n9. Sortir\n");
            int numeroImage = -1;
            do
            {
                Console.Write("> ");
                try { numeroImage = Convert.ToInt32(Console.ReadLine()); }
                catch { Console.Write("Entrez un numéro valide\n"); }
            } while (numeroImage != 0 && numeroImage != 1 && numeroImage != 2 && numeroImage != 3 && numeroImage != 4 && numeroImage != 9);

            string filename = null;
            switch (numeroImage)
            {
                case 0:
                    byte[] existence = null;
                    do
                    {
                        Console.Write("Entrer le nom du fichier : ");
                        string fichier = Convert.ToString(Console.ReadLine());
                        filename = fichier + ".bmp";
                        try { existence = File.ReadAllBytes(filename); }
                        catch { Console.WriteLine("Saisissez un nom de fichier existant."); }
                    } while (existence == null);
                    break;
                case 1:
                    filename = "coco.bmp";
                    break;
                case 2:
                    filename = "lac_en_montagne.bmp";
                    break;
                case 3:
                    filename = "lena.bmp";
                    break;
                case 4:
                    filename = "Test.bmp";
                    break;
                case 9:
                    Environment.Exit(0);
                    break;
            }
            return filename;
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
            CreerImage(5); // Tout le header est à faire
        }

        // Créer la nouvelle image

        /// <summary>
        /// Effectue dans un premier temps les modications nécessaire dans le header en fonction du traitement effectué, puis remplit entièrement le nouveau tableau de byte en reprenant le header pour les 54 premiers bits, puis les valeurs des couleurs bleue, vert et rouge pour le reste du tableau.
        /// Finit par écrire l'image, la créer puis l'afficher.
        /// </summary>
        /// <param name="operation"> modification à apporter dans le header selon le traitement à effectuer </param>
        /// <param name="multiple"> agrandissement ou rétrécissement de l'image </param>
        private void CreerImage(int operation, int multiple = 0)
        {
            byte[] creerFile = null; // Même principe que le tableau de byte myfile
            byte[] temporaire = new byte[4];

            switch (operation)
            {
                case 0: // nuance gris ; noir et blanc ; rotation 180 ; convolution ; insérer image dans une autre
                    creerFile = new byte[_myImage.tailleFichier];
                    break;
                // case 1: // Miroir
                //     int nouvelLargeur = 2 * largeur;
                //     temporaire = Convertir_Int_To_Endian(nouvelLargeur);
                //     int nouvelTaille = (tailleFichier - 54) * 2 + 54;
                //     creerFile = new byte[nouvelTaille];
                //     for (int i = 0; i <= 3; i++)
                //     {
                //         header[18 + i] = temporaire[i];
                //     }
                //     break;
                case 2: // Rotation 90° ou 270°
                    creerFile = new byte[_myImage.tailleFichier];
                    for (int i = 0; i <= 3; i++)
                    {
                    // Permuter dans le header, la largeur et hauteur
                        temporaire[i] = _myImage.header[22 + i];
                        _myImage.header[22 + i] = _myImage.header[18 + i];
                        _myImage.header[18 + i] = temporaire[i];
                    }
                    break;
                case 3: // Agrandir
                    creerFile = new byte[((_myImage.largeur * multiple) * (_myImage.hauteur * multiple) * 3) + 54]; // Multiplier par 3 pour les 3 couleurs primaires
                    temporaire = _myImage.Convertir_Int_To_Endian(_myImage.largeur * multiple);
                    for (int i = 0; i <= 3; i++)
                    {
                        _myImage.header[18 + i] = temporaire[i];
                    }
                    temporaire = _myImage.Convertir_Int_To_Endian(_myImage.hauteur * multiple);
                    for (int i = 0; i <= 3; i++)
                    {
                        _myImage.header[22 + i] = temporaire[i];
                    }
                    break;
                case 4: // Rétrécir
                    creerFile = new byte[((_myImage.largeur / multiple) * (_myImage.hauteur / multiple) * 3) + 54]; // Multiplier par 3 pour les 3 couleurs primaires
                    temporaire = _myImage.Convertir_Int_To_Endian(_myImage.largeur / multiple);
                    for (int i = 0; i <= 3; i++)
                    {
                        _myImage.header[18 + i] = temporaire[i];
                    }
                    temporaire = _myImage.Convertir_Int_To_Endian(_myImage.hauteur / multiple);
                    for (int i = 0; i <= 3; i++)
                    {
                        _myImage.header[22 + i] = temporaire[i];
                    }
                    break;
                case 5: // Fractale
                    _myImage.tailleFichier = (_myImage.largeur * _myImage.hauteur * 3) + 54;
                    creerFile = new byte[_myImage.tailleFichier];

                    _myImage.header[0] = 66; // B
                    _myImage.header[1] = 77; // M

                    temporaire = _myImage.Convertir_Int_To_Endian(_myImage.tailleFichier);
                    for (int i = 0; i <= 3; i++) // taille fichier
                    {
                        _myImage.header[2 + i] = temporaire[i];
                    }

                    for (int i = 0; i <= 3; i++)
                    {
                        _myImage.header[6 + i] = 0;
                    }

                    _myImage.header[10] = 54; // taille header

                    for (int i = 0; i <= 2; i++)
                    {
                        _myImage.header[11 + i] = 0;
                    }

                    _myImage.header[14] = 40; // taille offset

                    for (int i = 0; i <= 2; i++)
                    {
                        _myImage.header[15 + i] = 0; // taille offset
                    }

                    temporaire = _myImage.Convertir_Int_To_Endian(_myImage.largeur);
                    for (int i = 0; i <= 3; i++)
                    {
                        _myImage.header[18 + i] = temporaire[i]; // largeur
                    }

                    temporaire = _myImage.Convertir_Int_To_Endian(_myImage.hauteur);
                    for (int i = 0; i <= 3; i++)
                    {
                        _myImage.header[22 + i] = temporaire[i]; // hauteur
                    }

                    _myImage.header[26] = 1;
                    _myImage.header[27] = 0;
                    _myImage.header[28] = 24; // nb bit par couleur + (i = 29)

                    for (int i = 0; i <= 24; i++)
                    {
                        _myImage.header[29 + i] = 0;
                    }
                    break;
            }

            for (int offset = 0; offset < 54; offset++)
            {
                creerFile[offset] = _myImage.header[offset];
            }

            int ajustOffset = 0;

                for (int ligne = 0; ligne < _myImage.image.GetLength(0); ligne++)
                {
                    for (int colonne = 0; colonne < _myImage.image.GetLength(1); colonne++)
                    {
                        creerFile[54 + ajustOffset] = Convert.ToByte(_myImage.image[ligne, colonne].Blue);
                        creerFile[55 + ajustOffset] = Convert.ToByte(_myImage.image[ligne, colonne].Green);
                        creerFile[56 + ajustOffset] = Convert.ToByte(_myImage.image[ligne, colonne].Red);
                        ajustOffset += 3;
                    }
                }
    
            Console.WriteLine("Voulez-vous donner un nom à votre image ? (oui/non)");
            string reponse = Console.ReadLine();
            if (reponse == "oui")
            {
                Console.Write("Nom de l'image : ");
                _myImage.nom = Console.ReadLine() + ".bmp";
            }
            else
            {
                _myImage.nom = "nouvel_image" + ".bmp";
            }

            File.WriteAllBytes(_myImage.nom,creerFile);
            // string path = nom + ".bmp";
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = _myImage.nom,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}