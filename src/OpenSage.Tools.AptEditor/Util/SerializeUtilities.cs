using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OpenSage.FileFormats.Apt;
using OpenSage.FileFormats.Apt.ActionScript;

namespace OpenSage.Tools.AptEditor.Util
{
    class SerializeUtilities
    {
        

        public static string Serialize<T>(T obj)
        {


            if (obj is InstructionStorage)
            {

            }
            else
            {
                try
                {
                    return JsonSerializer.Serialize(obj);
                }
                catch (NotSupportedException)
                {
                    
                }
                catch (JsonException)
                {

                }
            }
            if (obj == null)
                return "null";
            else
            {
                var s = obj.ToString();
                if (s == null)
                    return "null";
                else
                    return s;
            }
        }

        public static T? Deserialize<T>(string ser)
        {
            if (typeof(T) == typeof(InstructionStorage))
            {

            }
            else
            {
                var objRaw = JsonSerializer.Deserialize(ser, typeof(T));
                if (objRaw == null)
                    return default(T);
                else
                    return (T) objRaw;
            }
            throw new NotSupportedException();
        }

    }
}
