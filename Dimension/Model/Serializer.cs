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

        public Dictionary<string, ulong> incomingTraffic = new Dictionary<string, ulong>();
        public Dictionary<string, ulong> outgoingTraffic = new Dictionary<string, ulong>();

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
            string t = c.GetType().FullName;
            b.Write(Encoding.UTF8.GetByteCount(t));
            b.Write(Encoding.UTF8.GetBytes(t));

            string d = Newtonsoft.Json.JsonConvert.SerializeObject(c);
            b.Write(Encoding.UTF8.GetByteCount(d));
            b.Write(Encoding.UTF8.GetBytes(d));
            b.Flush();
            byte[] output = m.ToArray();
            b.Dispose();
            m.Dispose();

            try
            {
                lock (outgoingTraffic)
                {
                    if (!outgoingTraffic.ContainsKey(t))
                        outgoingTraffic[t] = 0;
                    outgoingTraffic[t] += (ulong)output.Length;
                }
            }
            catch
            {
                //whatever
            }
            
            return output;
        }
        public string getText(byte[] b)
        {
            System.IO.MemoryStream m = new System.IO.MemoryStream(b);
            System.IO.BinaryReader br = new System.IO.BinaryReader(m);

            int tl = br.ReadInt32();
            string t = Encoding.UTF8.GetString(br.ReadBytes(tl));
            int dl = br.ReadInt32();
            string d = Encoding.UTF8.GetString(br.ReadBytes(dl));

            try
            {
                lock (incomingTraffic)
                {
                    if (!incomingTraffic.ContainsKey(t))
                        incomingTraffic[t] = 0;
                    incomingTraffic[t] += (ulong)b.Length;
                }
            }
            catch
            {
                //whatever
            }

            return d;
        }
        public Commands.Command deserialize(byte[] b)
        {
            System.IO.MemoryStream m = new System.IO.MemoryStream(b);
            System.IO.BinaryReader br = new System.IO.BinaryReader(m);

            int tl = br.ReadInt32();
            string t = Encoding.UTF8.GetString(br.ReadBytes(tl));
            int dl = br.ReadInt32();
            string d = Encoding.UTF8.GetString(br.ReadBytes(dl));

            try
            {
                if (commandTypes.ContainsKey(t))
                    return (Commands.Command)Newtonsoft.Json.JsonConvert.DeserializeObject(d, commandTypes[t]);
            }
            catch (Newtonsoft.Json.JsonSerializationException)
            {
                return null;    //invalid data, ignore
            }
            return null;
        }
    }
}
