using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public abstract class ScriptContent<TDerived, TContentType> : Asset
        where TDerived : ScriptContent<TDerived, TContentType>, new()
        where TContentType : struct
    {
        public TContentType ContentType { get; private set; }
        public AssetPropertyKey InternalName { get; private set; }

        public ScriptArgument[] Arguments { get; private set; }

        internal static TDerived Parse(BinaryReader reader, MapParseContext context, ushort minimumVersionThatHasInternalName)
        {
            return ParseAsset(reader, context, version =>
            {
                var contentType = reader.ReadUInt32AsEnum<TContentType>();

                AssetPropertyKey internalName = null;
                if (version >= minimumVersionThatHasInternalName)
                {
                    internalName = AssetPropertyKey.Parse(reader, context);
                }

                var numArguments = reader.ReadUInt32();
                var arguments = new ScriptArgument[numArguments];

                for (var i = 0; i < numArguments; i++)
                {
                    arguments[i] = ScriptArgument.Parse(reader);
                }

                return new TDerived
                {
                    ContentType = contentType,
                    InternalName = internalName,
                    Arguments = arguments
                };
            });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames, ushort minimumVersionThatHasInternalName)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((uint) (object) ContentType);

                if (Version >= minimumVersionThatHasInternalName)
                {
                    InternalName.WriteTo(writer, assetNames);
                }

                writer.Write((uint) Arguments.Length);

                foreach (var argument in Arguments)
                {
                    argument.WriteTo(writer);
                }
            });
        }
    }
}
