using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model.Commands
{
    public class FileListing : Command
    {
        public string path;
        public FSListing[] files = new FSListing[] { };
        public FSListing[] folders = new FSListing[] { };
    }
}
