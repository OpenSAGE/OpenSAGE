using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    public sealed class PostEffect
    {
        public string Name { get; private set; }

        // Only applicable to LookupTable in v1.
        // TODO: Could move this to a two hard-coded Parameters.
        public float BlendFactor { get; private set; }
        public string LookupImage { get; private set; }

        [AddedIn(SageGame.Cnc3)]
        public PostEffectParameter[] Parameters { get; private set; }

        internal static PostEffect Parse(BinaryReader reader, ushort version)
        {
            // v1 (BFME II) was hardcoded to store LookupTable's parameters.
            // v2 added a more flexible structure, generically storing the effect name and parameters.

            var result = new PostEffect
            {
                Name = reader.ReadUInt16PrefixedAsciiString()
            };

            if (version >= 2)
            {
                var numParameters = reader.ReadUInt32();
                result.Parameters = new PostEffectParameter[numParameters];
                for (var i = 0; i < numParameters; i++)
                {
                    result.Parameters[i] = PostEffectParameter.Parse(reader);
                }
            }
            else
            {
                result.BlendFactor = reader.ReadSingle();
                result.LookupImage = reader.ReadUInt16PrefixedAsciiString();
            }

            return result;
        }

        internal void WriteTo(BinaryWriter writer, ushort version)
        {
            writer.WriteUInt16PrefixedAsciiString(Name);

            if (version >= 2)
            {
                writer.Write((uint) Parameters.Length);
                foreach (var parameter in Parameters)
                {
                    parameter.WriteTo(writer);
                }
            }
            else
            {
                writer.Write(BlendFactor);
                writer.WriteUInt16PrefixedAsciiString(LookupImage);
            }
        }
    }

    [AddedIn(SageGame.Cnc3)]
    public sealed class PostEffectParameter
    {
        public string Name { get; private set; }
        public string Type { get; private set; }
        public object Data { get; private set; }

        internal static PostEffectParameter Parse(BinaryReader reader)
        {
            var result = new PostEffectParameter
            {
                Name = reader.ReadUInt16PrefixedAsciiString(),
                Type = reader.ReadUInt16PrefixedAsciiString()
            };

            switch (result.Type)
            {
                case "Float":
                    result.Data = reader.ReadSingle();
                    break;

                case "Float4":
                    result.Data = reader.ReadVector4();
                    break;

                case "Int":
                    result.Data = reader.ReadInt32();
                    break;

                case "Texture":
                    result.Data = reader.ReadUInt16PrefixedAsciiString();
                    break;

                default:
                    throw new InvalidDataException($"Unknown effect parameter type '{result.Type}' for parameter name '{result.Name}'.");
            }

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.WriteUInt16PrefixedAsciiString(Name);
            writer.WriteUInt16PrefixedAsciiString(Type);

            switch (Type)
            {
                case "Float":
                    writer.Write((float) Data);
                    break;

                case "Float4":
                    writer.Write((Vector4) Data);
                    break;

                case "Int":
                    writer.Write((int) Data);
                    break;

                case "Texture":
                    writer.WriteUInt16PrefixedAsciiString((string) Data);
                    break;

                default:
                    throw new InvalidDataException();
            }
        }
    }
}
