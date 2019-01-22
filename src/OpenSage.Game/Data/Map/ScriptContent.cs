using System;
using System.IO;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    public abstract class ScriptContent<TDerived, TContentType> : Asset
        where TDerived : ScriptContent<TDerived, TContentType>, new()
        where TContentType : struct
    {
        public TContentType ContentType { get; private set; }
        public AssetPropertyKey InternalName { get; private set; }

        public ScriptArgument[] Arguments { get; private set; }

        public bool Enabled { get; private set; }

        internal static TDerived Parse(
            BinaryReader reader,
            MapParseContext context,
            ushort minimumVersionThatHasInternalName,
            ushort minimumVersionThatHasEnabledFlag,
            Action<ushort, TDerived> derivedParse = null)
        {
            return ParseAsset(reader, context, version =>
            {
                var result = new TDerived();

                // TODO: Need to make game-specific TContentType enums.
                result.ContentType = (TContentType) (object) reader.ReadUInt32();
                //result.ContentType = reader.ReadUInt32AsEnum<TContentType>();

                if (version >= minimumVersionThatHasInternalName)
                {
                    result.InternalName = AssetPropertyKey.Parse(reader, context);
                }

                var numArguments = reader.ReadUInt32();
                result.Arguments = new ScriptArgument[numArguments];

                for (var i = 0; i < numArguments; i++)
                {
                    result.Arguments[i] = ScriptArgument.Parse(reader);
                }

                result.Enabled = true;
                if (version >= minimumVersionThatHasEnabledFlag)
                {
                    result.Enabled = reader.ReadBooleanUInt32Checked();
                }

                derivedParse?.Invoke(version, result);

                return result;
            });
        }

        internal void WriteTo(
            BinaryWriter writer,
            AssetNameCollection assetNames,
            ushort minimumVersionThatHasInternalName,
            ushort minimumVersionThatHasEnabledFlag,
            Action derivedWriteTo = null)
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

                if (Version >= minimumVersionThatHasEnabledFlag)
                {
                    writer.WriteBooleanUInt32(Enabled);
                }

                derivedWriteTo?.Invoke();
            });
        }
    }
}
