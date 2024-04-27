using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBitMap
{
    public class Pixel
    {
        int red;
        int green;
        int blue;

        public Pixel(int Blue, int Green, int Red)
        {
            if (Red >= 0 && Red < 256 && Blue >= 0 && Blue < 256 && Green >= 0 && Green < 256)
            {
                blue = Blue;
                green = Green;
                red = Red;
            }
        }
                
        public int Red
        {
            set { red = value; }
            get { return red; }
        }
        public int Green
        {
            set { green = value; }
            get { return green; }
        }
        public int Blue
        {
            set { blue = value; }
            get { return blue; }
        }

        public void GrayScale()
        {
            int averageColor = (red + green + blue) / 3;
            red = averageColor;
            green = averageColor;
            blue = averageColor;
        }

        public void BinarizeColor()
        {
            int color = (this.red + this.blue + this.green) / 3;
            switch (color)
            {
                case int n when (n <= 128):
                    this.red = 0;
                    this.green = 0;
                    this.blue = 0;
                    break;
                case int n when (n >= 128):
                    this.red = 255;
                    this.green = 255;
                    this.blue = 255;
                    break;
            }
        }
    }
}