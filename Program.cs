﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace MyBitMap
{
    class Program
    {
        /// <summary>
        /// Cette méthode permet à l'utilisateur de choisir une image parmi celles disponibles ou d'en spécifier une en saisissant son nom
        /// Sert aussi à obtenir un aperçu de l'image sélectionnée.
        /// </summary>
        /// <returns>Un string qui représente le nom du fichier afin de récupérer ses caractéristiques.</returns>
        static string Run()
        {
            Console.WriteLine("Choisissez une image ou déposez-en une à la racine du projet et entrez son nom\n" +
                "\n0. Testez votre fichier personnel" +
                "\n1. Jamel" +
                "\n2. Test" +
                "\n\n------------- TD5... -------------\n" +
                "\n3. Dessiner la fractale de Mandelbrot\n");

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
                    else if (pictureNumber == 3)
                    {
                        MyImage image = new MyImage();
                        image.imageProcessService.fractale();
                        Environment.Exit(0);
                    }
                    else if (pictureNumber == 4)
                    {
                        MyImage image = new MyImage();
                        image.imageProcessService.steganography();
                        Environment.Exit(0);
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

            if (!string.IsNullOrEmpty(filename))
            {
                OpenImage(filename);
            }
            return filename;
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
        /// Ouvre l'image spécifiée par le nom du fichier.
        /// </summary>
        /// <param name="filename">Le nom du fichier image à openPicture.</param>
        static void OpenImage(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                Console.WriteLine("\n\nVoulez-vous ouvrir cette image ?\n1. Oui\n2. Non\n");

                int openPicture = -1;
                while (openPicture != 1 && openPicture != 2)
                {
                    Console.Write("> ");
                    string input = Console.ReadLine();

                    if (int.TryParse(input, out openPicture))
                    {
                        if (openPicture != 1 && openPicture != 2)
                        {
                            Console.Write("Entrez un numéro valide\n");
                        }
                    }
                    else
                    {
                        Console.Write("Entrez un numéro valide\n");
                    }
                }

                if (openPicture == 1)
                {
                    string path = Path.GetFullPath(filename);
                    Console.WriteLine("Ouverture de l'image... : " + path);

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
            }
        }

        /// <summary>
        /// Cette fontiononne permet d'afficher les propriétés de l'image.
        /// </summary>
        /// <param name="image"> La classe avec laquelle l'image selectionée à été instanciée </param>
        static void DisplayDataPicture(MyImage image)
        {
            Console.WriteLine("\n\nSelectionner une option pour " 
            + image.Nom 
            + " ?\n1. Afficher les détails de l'image\n2. Passer cette étape\n");
            int display;
            
            while (!int.TryParse(Console.ReadLine(), out display) || (display != 2 && display != 1 ))
            {
                Console.Write("Entrez un numéro valide\n");
            }
            
            switch (display)
            {
                case 1:
                    Console.WriteLine("Caractéristique de " + image.Nom + "\n");
                    Console.WriteLine("Type du fichier: " + image.TypeImage);
                    Console.WriteLine($"Taille du fichier: {image.SizeFile} bits\n" +
                    $"Taille de l'Offset: {image.OffsetSize}\n" +
                    $"Largeur: {image.Width} pixels\n" +
                    $"Hauteur: {image.Height} pixels\n");
                    Console.WriteLine("Appuyez sur une touche pour passer au traitement d'image !");
                    Console.ReadKey();
                    break;
            }
        }


        /// <summary>
        /// Gere le traitement d'image, differentes options sont disponibles pour l'utilisateur
        /// </summary>
        /// <param name="image"> La classe de l'image sur laquelle on veut effectuer les traitements d'images </param>
        static void ProcessPicture(MyImage image)
        {
            int pictureNumber = -1;
            DisplayOptionPicture(image.Nom);
            for (; ; )
            {
                Console.Write("> ");
                try { pictureNumber = Convert.ToInt32(Console.ReadLine()); }
                catch { Console.Write("Entrez un numéro valide\n"); }

                if (pictureNumber == 0)
                    break;
                if (pictureNumber >= 1 && pictureNumber <= 9)
                    break;
            }

            if (pictureNumber == 0)
            {
                Environment.Exit(0);
            }
            else if (pictureNumber == 1)
            {
                image.imageProcessService.grayScalePicture();
            }
            else if (pictureNumber == 2)
            {
                image.imageProcessService.BinarizePicture();
            }
            else if (pictureNumber == 3)
            {
                image.imageProcessService.rotation();
            }
            else if (pictureNumber == 4)
            {
                image.imageProcessService.moreBigPicture();
            }
            else if (pictureNumber == 5)
            {
                Console.Clear();
                image.imageProcessService.filter("contour");
            }
            else if (pictureNumber == 6)
            {
                Console.Clear();
                image.imageProcessService.filter("renforcement");
            }
            else if (pictureNumber == 7)
            {
                Console.Clear();
                image.imageProcessService.filter("flou");
            }
            else if (pictureNumber == 8)
            {
                Console.Clear();
                image.imageProcessService.filter("repoussage");
            }
            else if (pictureNumber == 9)
            {
                Console.Clear();
                image.imageProcessService.steganography();
            }
        }

        static void DisplayOptionPicture(String imageName)
        {
            Console.WriteLine("\nChoisissez l'opération que vous voulez appliqué à l'image \'" + imageName + "\' :\n" +
                "------------- TD1-2... -------------\n" +
                "\n1. Appliquer une nuance de gris" +
                "\n2. Appliquer un filtre noir et blanc\n" +
                "------------- TD3... -------------\n" +
                "\n3. Effectuer une rotation" +
                "\n4. Agrandir l'image avec un coéfficient quelconque (entier)\n" +
                "------------- TD4... -------------\n" +
                "\n5. Appliquer la détection de contour" +
                "\n6. Appliquer le renforcement des bords de l'image" +
                "\n7. Appliquer un flou sur l'image" +
                "\n8. Appliquer un repoussage sur l'image" +
                "\n------------- TD5... -------------\n" +
                "\n9. Dessiner la steganography\n" + 
                "------------- TD6-7... -------------\n" +
                "\n      STARFOULLAH JPP\n" + 
    
                "\n\n0. Quitter\n");
        }

        static void Main(string[] args)
        {
            MyImage picture = new MyImage(Run());
            DisplayDataPicture(picture);
            ProcessPicture(picture);
            Console.WriteLine("\n\nL'image a été traitée avec succès !\n\n" 
            + "Vous la trouverez à la racine du projet\n\n");
        }
    }
}