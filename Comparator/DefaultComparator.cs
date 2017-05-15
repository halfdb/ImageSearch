using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearch
{
    public class DefaultComparator: Comparator
    {
        public DefaultComparator(string folder)
            :base(folder)
        {
        }

        public override double Compare(Bitmap picture1, Bitmap picture2)
        {
            throw new NotImplementedException();
        }
    }
}
