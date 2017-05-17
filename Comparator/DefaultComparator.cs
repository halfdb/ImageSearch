using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        private readonly Task _initTask;
        public DefaultComparator(string folder)
            : base(folder)
        {
            Distributions = new Distribution[Count];
            _initTask = Task.Run(() =>
            {
                var time1 = Environment.TickCount;
                for (var i = 0; i < Count; i++)
                {
                    Distributions[i] = new Distribution(Bitmaps[i]);
                }
                var time2 = Environment.TickCount;
                Console.WriteLine($"init finished. time spent: {time2 - time1}ms");
            });
        }

        protected struct Distribution
        {
            private readonly double _pixelCount;

            private readonly int[,] _rgby;

            public Distribution(Bitmap bitmap)
            {
                _rgby = new int[ShadeCount, Y+1];
                var w = bitmap.Width;
                var h = bitmap.Height;
                _pixelCount = w * h;
                for (var j = 0; j < h; j++)
                {
                    for (var i = 0; i < w; i++)
                    {
                        var color = bitmap.GetPixel(i, j);
                        _rgby[color.R, R]++;
                        _rgby[color.G, G]++;
                        _rgby[color.B, B]++;
                        var luminance = (int)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
                        _rgby[luminance, Y]++;
                    }
                }
            }

            public double Probability(int rgby, int shade)
            {
                if (rgby < R || rgby > Y)
                {
                    throw new ArgumentOutOfRangeException(nameof(rgby), rgby, $"[{R}, {Y}]");
                }
                if (shade<0 || shade >= ShadeCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(shade), shade, $"[0, {ShadeCount})");
                }
                return _rgby[shade, rgby] / _pixelCount;
            }
        }

        protected readonly IList<Distribution> Distributions;
        private Tuple<Bitmap, Distribution>_cache;

        protected override double Compare(int storedIndex, Bitmap comparing)
        {
            if (_cache is null || _cache.Item1 != comparing)
            {
                _cache = new Tuple<Bitmap, Distribution>(comparing, new Distribution(comparing));
            }
            while (!_initTask.IsCompleted)
            {
                Thread.Sleep(500);
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