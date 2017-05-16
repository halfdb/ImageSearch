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

        protected struct Distribution
        {
            private double pixelCount;
            public int PixelCount => (int)pixelCount;

            private int[,] RGBY;

            public Distribution(Bitmap bitmap)
            {
                RGBY = new int[ShadeCount, Y+1];
                var w = bitmap.Width;
                var h = bitmap.Height;
                pixelCount = w * h;
                for (var j = 0; j < h; j++)
                {
                    for (var i = 0; i < w; i++)
                    {
                        var color = bitmap.GetPixel(i, j);
                        RGBY[color.R, R]++;
                        RGBY[color.G, G]++;
                        RGBY[color.B, B]++;
                        var luminance = (int)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
                        RGBY[luminance, Y]++;
                    }
                }
            }

            public double Probability(int rgb, int shade)
            {
                if (rgb < R || rgb > Y)
                {
                    throw new ArgumentOutOfRangeException(nameof(rgb), rgb, $"[{R}, {Y}]");
                }
                if (shade<0 || shade >= ShadeCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(shade), shade, $"[0, {ShadeCount})");
                }
                return RGBY[shade, rgb] / pixelCount;
            }
        }

        protected IList<Distribution> Distributions;
        private Tuple<Bitmap, Distribution>_cache;

        public override double Compare(int storedIndex, Bitmap comparing)
        {
            if (_cache is null || _cache.Item1 != comparing)
            {
                _cache = new Tuple<Bitmap, Distribution>(comparing, new Distribution(comparing));
            }
            var comparingDistribution = _cache.Item2;
            var distances = new double[Y+1];
            for (var color = 0; color < Y+1; color++)
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