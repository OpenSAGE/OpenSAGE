using System.Collections.Generic;
using System.IO;

namespace OpenZH.Data.Map
{
    public abstract class ScriptHierarchyNode
    {
        public static ScriptHierarchyNode[] ParseNodes(BinaryReader reader, long endPosition, string[] assetStrings)
        {
            var scriptHierarchyNodes = new List<ScriptHierarchyNode>();
            while (reader.BaseStream.Position < endPosition)
            {
                var nodeType = reader.ReadUInt32();
                var nodeName = assetStrings[nodeType - 1];

                switch (nodeName)
                {
                    case "ScriptGroup":
                        scriptHierarchyNodes.Add(ScriptGroup.Parse(reader, assetStrings));
                        break;

                    case "Script":
                        scriptHierarchyNodes.Add(Script.Parse(reader, assetStrings));
                        break;

                    default:
                        throw new InvalidDataException($"Unexpected script hierarchy node name: {nodeName}");
                }
            }
            return scriptHierarchyNodes.ToArray();
        }
    }
}
