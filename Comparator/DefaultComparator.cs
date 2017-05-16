using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static System.Math;

namespace ImageSearch
{
    public class DefaultComparator : Comparator
    {
        public const int ShadeCount = 256;
        public const int R = 0;
        public const int G = R + 1;
        public const int B = G + 1;
        public const int Y = B + 1; // luminance

        public DefaultComparator(string folder)
            : base(folder)
        {
            Distributions = new Distribution[Count];
            for (var i = 0; i < Count; i++)
            {
                Distributions[i] = new Distribution(Bitmaps[i]);
            }
        }

        protected class Distribution
        {
            private double pixelCount;
            public int PixelCount => (int)pixelCount;

            private int[][] RGBY = { new int[ShadeCount], new int[ShadeCount], new int[ShadeCount], new int[ShadeCount] };

            public Distribution(Bitmap bitmap)
            {
                var w = bitmap.Width;
                var h = bitmap.Height;
                pixelCount = w * h;
                for (var i = 0; i < w; i++)
                {
                    for (var j = 0; j < h; j++)
                    {
                        var color = bitmap.GetPixel(i, j);
                        RGBY[R][color.R]++;
                        RGBY[G][color.G]++;
                        RGBY[B][color.B]++;
                        var y = (int)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
                        RGBY[Y][y]++;
                    }
                }
            }

            public double Probability(int rgb, int shade)
            {
                if (rgb < R || rgb > Y)
                {
                    throw new ArgumentOutOfRangeException(nameof(rgb), rgb, $"[{R}, {Y}]");
                }
                else if (shade<0 || shade >= ShadeCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(shade), shade, $"[0, {ShadeCount})");
                }
                return RGBY[rgb][shade] / pixelCount;
            }
        }

        protected IList<Distribution> Distributions;

        public override double Compare(int storedIndex, Bitmap comparing)
        {
            var comparingDistribution = new Distribution(comparing);
            var distances = new double[Y];
            for (var color = 0; color < Y; color++)
            {
                double sum = 0;
                for (var shade = 0; shade < ShadeCount; shade++)
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