using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model.Commands
{
    public class FileChunk : DataCommand
    {
        public long start;
        public long totalSize;
        public string path;
        public string originalPath;
    }
}
