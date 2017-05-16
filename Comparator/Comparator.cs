using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImageSearch
{
    public abstract class Comparator
    {
        private static readonly string[] supportedFormat =
        {
            "bmp",
            "jpg",
            //"png",
            //"gif",
        };

        protected readonly IList<Bitmap> Bitmaps = new List<Bitmap>();

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
                    if (!supportedFormat.Contains(ext))
                    {
                        continue;
                    }
                    var img = Image.FromFile(filename);
                    Bitmaps.Add(new Bitmap(img));
                }
                _folder = value;
                Count = Bitmaps.Count;
            }
        }
        private string _folder = null;
        protected int Count;

        protected Comparator(string folder)
        {
            Folder = folder;
        }

        public abstract double Compare(int storedIndex, Bitmap comparing);
        public IList<Bitmap> Search(Bitmap bitmap)
        {
            var scores = new List<double>();
            for (var i = 0; i < Count; i++)
            {
                scores.Add(Compare(i, bitmap));
            }
            var result = Bitmaps.ToArray();
            Array.Sort(scores.ToArray(), result);
            return result;
        }

        protected void DisposeImages()
        {
            foreach (var bitmap in Bitmaps)
            {
                bitmap.Dispose();
            }
            Bitmaps.Clear();
        }

        ~Comparator()
        {
            DisposeImages();
        }
    }

    public static class ComparatorFactory
    {
        public static Comparator NewComparator(string folder)
        {
            return new DefaultComparator(folder);
        }
    }
}
