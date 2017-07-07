using System.IO;

namespace OpenZH.Data.Map
{
    public class PlayerScriptsList
    {
        public ScriptList[] ScriptLists { get; private set; }

        public static PlayerScriptsList Parse(BinaryReader reader, string[] assetStrings)
        {
            var header = reader.ReadUInt32();
            if (assetStrings[header - 1] != "PlayerScriptsList")
            {
                throw new InvalidDataException();
            }

            var numScriptLists = reader.ReadUInt16();
            var scriptLists = new ScriptList[numScriptLists];

            var dataSize = reader.ReadUInt32();
            var startPosition = reader.BaseStream.Position;

            for (var i = 0; i < numScriptLists; i++)
            {
                scriptLists[i] = ScriptList.Parse(reader, assetStrings);
            }

            if (startPosition + dataSize != reader.BaseStream.Position)
            {
                throw new InvalidDataException();
            }

            return new PlayerScriptsList
            {
                ScriptLists = scriptLists
            };
        }
    }
}
