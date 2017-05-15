using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearch
{
    class Program
    {
        public static void Main(string[] args)
        {
            var folder = Console.ReadLine();
            var comparator = ComparatorFactory.NewComparator(folder);
        }
    }
}
