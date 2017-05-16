using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearch
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.Write("initializing folder:");
            var folder = Console.ReadLine();
            var time1 = Environment.TickCount;
            var comparator = ComparatorFactory.NewComparator(folder);
            var time2 = Environment.TickCount;
            Console.WriteLine($"time spent: {time2 - time1}ms");
            while (true)
            {
                Console.Write("test picture:");
                var imagePath = Console.ReadLine();
                if (!(imagePath == "" || imagePath is null))
                {
                    try
                    {
                        using (var bitmap = new Bitmap(Image.FromFile(imagePath)))
                        {
                            var filenames = comparator.SearchFilenames(bitmap, out var distances);
                            Console.WriteLine("results:");
                            foreach (var pair in filenames.Zip(distances, (s, d) => new Tuple<string, double>(s, d)))
                            {
                                var filename = pair.Item1;
                                var distance = pair.Item2;
                                var idx = filename.LastIndexOf(@"\") + 1;
                                Console.WriteLine($"filname: {filename.Substring(idx)}, distance: {distance}");
                            }
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        Console.WriteLine("file not found.");
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
}
