using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model.Commands
{
    class GetFileListing : Command
    {
        public string path = "/";
        public GetFileListing(string path)
        {
            this.path = path;
        }
        public GetFileListing()
        {
            path = "/";
        }
    }
}
