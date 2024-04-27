﻿using System;
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
        static string Selection()
        {
            Console.WriteLine("Choisissez une image ou déposez-en une à la racine du projet et entrez son nom\n" +
                "\n0. Testez votre fichier personnel" +
                "\n1. Jamel" +
                "\n2. Test" +
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
                        image.fractale();
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
            byte[] existence = null;

            while (existence == null)
            {
                Console.Write("Entrer le nom du fichier : ");
                string fichier = Console.ReadLine();
                filename = fichier + ".bmp";

                try
                {
                    existence = File.ReadAllBytes(filename);
                }
                catch
                {
                    Console.WriteLine("Saisissez un nom d'image existant.");
                }
            }

            return filename;
        }

        /// <summary>
        /// Ouvre l'image spécifiée par le nom du fichier.
        /// </summary>
        /// <param name="filename">Le nom du fichier image à ouvrir.</param>
        static void OpenImage(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                Console.WriteLine("\n\nVoulez-vous ouvrir cette image ?\n1. Oui\n2. Non\n");

                int ouvrir = -1;
                while (ouvrir != 1 && ouvrir != 2)
                {
                    Console.Write("> ");
                    string input = Console.ReadLine();

                    if (int.TryParse(input, out ouvrir))
                    {
                        if (ouvrir != 1 && ouvrir != 2)
                        {
                            Console.Write("Entrez un numéro valide\n");
                        }
                    }
                    else
                    {
                        Console.Write("Entrez un numéro valide\n");
                    }
                }

                if (ouvrir == 1)
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
        /// Donne la possibilité d'afficher les propriétés de l'image.
        /// </summary>
        /// <param name="image"> La classe de l'image dont on veut afficher les propriétés </param>
        static void Parametre(MyImage image)
        {
            Console.WriteLine("\n\nVoulez afficher les caracteristiques de " + image.Nom + " ?\n1. Oui\n2. Non\n");
            int afficher = -1;
            do
            {
                Console.Write("> ");
                try { afficher = Convert.ToInt32(Console.ReadLine()); }
                catch { Console.Write("Entrez un numéro valide\n"); }
            } while (afficher != 1 && afficher != 2);

            Console.Clear();
            if (afficher == 1)
            {
                Console.WriteLine("Caractéristique de " + image.Nom + "\n");
                Console.WriteLine("Format : " + image.TypeImage + "\nTaille : " + image.TailleFichier + " bits\nTaille de l'Offset : " + image.TailleOffset + "\nLargeur : " + image.Largeur + " pixels\nHauteur : " + image.Hauteur + " pixels\n\n");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Affiche les différents traitements appliquable à l'image et laisse le choix à l'utilisateur
        /// </summary>
        /// <param name="image"> La classe de l'image sur laquelle on veut effectuer les traitements d'images </param>
        static void Traitement(MyImage image)
        {
            int pictureNumber = -1;
            Console.WriteLine("Quelle opération voulez vous effectuer sur " + image.Nom +" :\n" +
                "\n1. Nuance de gris" +
                "\n2. Noir et blanc +" +
                "\n3. Image miroir" +
                "\n4. Rotation" +
                "\n5. Agrandir" +
                "\n6. Rétrécir" +
                "\n7. Appliquer un filtre" +
                "\n8. Miroir contenu" +
                "\n9. Insérer une image dans une autre" +
                "\n\n0. Sortir\n");
            do
            {
                Console.Write("> ");
                try { pictureNumber = Convert.ToInt32(Console.ReadLine()); }
                catch { Console.Write("Entrez un numéro valide\n"); }
            } while (pictureNumber != 0 && pictureNumber != 1 && pictureNumber != 2 && pictureNumber != 3 && pictureNumber != 4 && pictureNumber != 5 && pictureNumber != 6 && pictureNumber != 7 && pictureNumber != 8 && pictureNumber != 9);

            switch (pictureNumber)
            {
                case 0:
                    Environment.Exit(0); // Si on ne veut pas aller jusqu'au bout
                    break;
                case 1:
                    image.enGris();
                    break;
                case 2:
                    image.NetB();
                    break;
                case 3:
                    image.miroir();
                    break;
                case 4:
                    image.rotation();
                    break;
                case 5:
                    image.agrandir();
                    break;
                case 6:
                    image.retrecir();
                    break;
                case 7:
                    Console.Clear();
                    image.filtre();
                    break;
                case 8:
                    image.miroirContenu();
                    break;
                case 9:
                    Console.Clear();
                    image.dissimulerImage();
                    break;
            }
        }

        static void Main(string[] args)
        {
            MyImage image = new MyImage(Selection());
            Parametre(image);
            Traitement(image);

            Console.ReadKey();
        }
    }
}