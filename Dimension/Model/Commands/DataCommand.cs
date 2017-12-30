using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model.Commands
{
    public class DataCommand : Command
    {
        [Newtonsoft.Json.JsonIgnore]
        public byte[] data;

        public int dataLength;
    }
}
