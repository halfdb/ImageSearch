using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageSearch
{
    public abstract class Comparator
    {
        private static readonly string[] SupportedFormat =
        {
            "bmp",
            "jpg",
            //"png",
            //"gif",
        };

        protected readonly IList<Bitmap> Bitmaps = new List<Bitmap>();
        protected readonly IList<string> Filenames = new List<string>();

        public string Folder
        {
            get => _folder;
            set
            {
                DisposeImages();
                var directory = Directory.EnumerateFiles(value);
                foreach (var filename in directory)
                {
                    var splittedFilename = filename.Split('.');
                    var ext = splittedFilename[splittedFilename.Length - 1].ToLower();
                    if (!SupportedFormat.Contains(ext))
                    {
                        continue;
                    }
                    var img = Image.FromFile(filename);
                    Bitmaps.Add(new Bitmap(img));
                    Filenames.Add(filename);
                }
                _folder = value;
                Count = Bitmaps.Count;
            }
        }
        private string _folder;
        protected int Count;

        protected Comparator(string folder)
        {
            Folder = folder;
        }

        protected abstract double Compare(int storedIndex, Bitmap comparing);

        protected IList<int> SearchIndices(Bitmap bitmap, out IList<double> distances)
        {
            distances = new double[Count];
            var indices = new int[Count];
            for (var i = 0; i < Count; i++)
            {
                indices[i] = i;
                distances[i] = Compare(i, bitmap);
            }
            Array.Sort(distances as Array, indices);
            return indices;
        }

        public IList<Bitmap> Search(Bitmap bitmap)
        {
            return Search(bitmap, out _);
        }

        public IList<Bitmap> Search(Bitmap bitmap, out IList<double> distances)
        {
            var indices = SearchIndices(bitmap, out distances);
            var result = new Bitmap[Count];
            for (var i = 0; i < indices.Count; i++)
            {
                result[i] = Bitmaps[indices[i]];
            }
            return result;
        }

        public IList<string> SearchFilenames(Bitmap bitmap)
        {
            return SearchFilenames(bitmap, out _);
        }

        public IList<string> SearchFilenames(Bitmap bitmap, out IList<double> distances)
        {
            var indices = SearchIndices(bitmap, out distances);
            var result = new string[Count];
            for (var i = 0; i < indices.Count; i++)
            {
                result[i] = Filenames[indices[i]];
            }
            return result;
        }

        protected void DisposeImages()
        {
            foreach (var bitmap in Bitmaps)
            {
                bitmap.Dispose();
            }
            Bitmaps.Clear();
            Filenames.Clear();
        }

        ~Comparator()
        {
            DisposeImages();
        }
    }

    public static class ComparatorFactory
    {
        public static Comparator NewComparator(string folder) => new DefaultComparator(folder);

        public delegate double ComparingDelegate(Bitmap picture1, Bitmap picture2);

        private sealed class DelegateComparator: Comparator
        {
            internal DelegateComparator(string folder, ComparingDelegate compare) : base(folder)
            {
                _compare = compare;
            }

            private readonly ComparingDelegate _compare;

            protected override double Compare(int storedIndex, Bitmap comparing)
            {
                return _compare(Bitmaps[storedIndex], comparing);
            }
        }

        public static Comparator FromDelegate(string folder, ComparingDelegate compare)
        {
            return new DelegateComparator(folder, compare);
        }
    }
}
