using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model.Commands
{
    public class RequestChunks:Command
    {
        public long startingByte;
        public bool allChunks;
        public string path;
    }
}
