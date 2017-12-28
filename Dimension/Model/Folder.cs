using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class Folder : FSListing
    {
        public ulong[] folderIds = new ulong[0];
        public ulong[] fileIds = new ulong[0];
    }
}
