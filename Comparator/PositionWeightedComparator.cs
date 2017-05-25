using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using static System.Math;

namespace ImageSearch
{
    public class PositionWeightedComparator: DefaultComparator
    {
        public PositionWeightedComparator(string folder)
        {
            Folder = folder;
            Distributions = new IDistribution[Count];
            InitTask = Task.Run(() =>
            {
                var time1 = Environment.TickCount;
                for (var i = 0; i < Count; i++)
                {
                    Distributions[i] = new PositionWeightedDistribution(Bitmaps[i]);
                }
                var time2 = Environment.TickCount;
                Trace.WriteLine($"init finished. time spent: {time2 - time1}ms");
            });
        }

        protected struct PositionWeightedDistribution : IDistribution
        {
            private double _totalWeight;

            private readonly int[,] _rgby;

            private const int MaxWeight = 100;

            public PositionWeightedDistribution(Bitmap bitmap)
            {
                _rgby = new int[ShadeCount / BucketSize, Y + 1];
                _totalWeight = 0;
                Initialize(bitmap);
            }

            public void Initialize(Bitmap bitmap)
            {
                var w = bitmap.Width;
                var halfW = w / 2.0;
                var h = bitmap.Height;
                var halfH = h / 2.0;
                for (var j = 0; j < h; j++)
                {
                    for (var i = 0; i < w; i++)
                    {
                        var dx = Max(0, Abs(i - halfW) / halfW - 0.1);
                        var dy = Max(0, Abs(j - halfH) / halfH - 0.1);
                        var weight = (int) (MaxWeight * Max(0, 1 - Sqrt(dx * dx + dy * dy)));
                        _totalWeight += weight;
                        var color = bitmap.GetPixel(i, j);
                        _rgby[color.R / BucketSize, R] += weight;
                        _rgby[color.G / BucketSize, G] += weight;
                        _rgby[color.B / BucketSize, B] += weight;
                        var luminance = (int)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
                        _rgby[luminance / BucketSize, Y] += weight;
                    }
                }
            }

            public double Probability(int rgby, int shade)
            {
                if (rgby < R || rgby > Y)
                {
                    throw new ArgumentOutOfRangeException(nameof(rgby), rgby, $"[{R}, {Y}]");
                }
                if (shade < 0 || shade >= ShadeCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(shade), shade, $"[0, {ShadeCount})");
                }
                return _rgby[shade / BucketSize, rgby] / _totalWeight;
            }
        }

        protected override double Compare(int storedIndex, Bitmap comparing)
        {
            if (Cache?.Item1 != comparing)
            {
                Cache = new Tuple<Bitmap, IDistribution>(comparing, new PositionWeightedDistribution(comparing));
            }
            return base.Compare(storedIndex, comparing);
        }
    }
}
