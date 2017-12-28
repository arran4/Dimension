using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class Serializer
    {
        Dictionary<string, Type> commandTypes = new Dictionary<string, Type>();

        public Serializer()
        {
            foreach (Type t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
                if (t.IsSubclassOf(typeof(Commands.Command)))
                    commandTypes.Add(t.FullName, t);
        }
        public byte[] serialize(Commands.Command c)
        {
            System.IO.MemoryStream m = new System.IO.MemoryStream();
            System.IO.BinaryWriter b = new System.IO.BinaryWriter(m);
            b.Write(c.GetType().FullName);
            b.Write(Newtonsoft.Json.JsonConvert.SerializeObject(c));
            byte[] output = m.ToArray();
            b.Dispose();
            m.Dispose();
            return output;
        }
        public Commands.Command deserialize(byte[] b)
        {

            System.IO.MemoryStream m = new System.IO.MemoryStream(b);
            System.IO.BinaryReader br = new System.IO.BinaryReader(m);

            string t = br.ReadString();
            string d = br.ReadString();

            if (commandTypes.ContainsKey(t))
                return (Commands.Command)Newtonsoft.Json.JsonConvert.DeserializeObject(d, commandTypes[t]);
            return null;
        }
        }
}
