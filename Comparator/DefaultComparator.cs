using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using static System.Math;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearch
{
    public class DefaultComparator : Comparator
    {
        public const int SHADE_COUNT = 256;
        public const int R = 0;
        public const int G = R + 1;
        public const int B = G + 1;
        public const int Y = B + 1; // luminance

        public DefaultComparator(string folder)
            : base(folder)
        {
            Distributions = new Distribution[Count];
            for (int i = 0; i < Count; i++)
            {
                Distributions[i] = new Distribution(Bitmaps[i]);
            }
        }

        protected class Distribution
        {
            private double pixelCount;
            public int PixelCount { get => (int)pixelCount; }

            private int[][] RGBY = { new int[SHADE_COUNT], new int[SHADE_COUNT], new int[SHADE_COUNT], new int[SHADE_COUNT] };

            public Distribution(Bitmap bitmap)
            {
                int w = bitmap.Width;
                int h = bitmap.Height;
                pixelCount = w * h;
                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        var color = bitmap.GetPixel(i, j);
                        RGBY[R][color.R]++;
                        RGBY[G][color.G]++;
                        RGBY[B][color.B]++;
                        int y = (int)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
                        RGBY[Y][y]++;
                    }
                }
            }

            public double Probability(int rgb, int shade)
            {
                if (rgb < R || rgb > Y)
                {
                    throw new ArgumentOutOfRangeException("rgb", rgb, $"[{R}, {Y}]");
                }
                else if (shade<0 || shade >= SHADE_COUNT)
                {
                    throw new ArgumentOutOfRangeException("shade", shade, $"[0, {SHADE_COUNT})");
                }
                return RGBY[rgb][shade] / PixelCount;
            }
        }

        protected IList<Distribution> Distributions;

        public override double Compare(int storedIndex, Bitmap comparing)
        {
            var comparingDistribution = new Distribution(comparing);
            var distances = new double[Y];
            for (int color = 0; color < Y; color++)
            {
                double sum = 0;
                for (int shade = 0; shade < SHADE_COUNT; shade++)
                {
                    sum += Sqrt(comparingDistribution.Probability(color, shade) *
                        Distributions[storedIndex].Probability(color, shade));
                }
                distances[color] = -Log(sum);
            }
            return distances.Sum();
        }
    }

}