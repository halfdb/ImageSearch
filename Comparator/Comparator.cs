﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearch
{
    public abstract class Comparator
    {
        private static string[] supportedFormat =
        {
            "bmp",
            "jpg",
            //"png",
            //"gif",
        };

        protected IList<Bitmap> Bitmaps = new List<Bitmap>();

        public string Folder
        {
            get => folder;
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
            }
        }
        private string folder = null;

        protected Comparator(string folder)
        {
            Folder = folder;
        }

        public abstract double Compare(Bitmap picture1, Bitmap picture2);
        public IList<Bitmap> Search(Bitmap bitmap)
        {
            throw new NotImplementedException();
        }

        protected void DisposeImages()
        {
            foreach (var bitmap in Bitmaps)
            {
                bitmap.Dispose();
            }
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
