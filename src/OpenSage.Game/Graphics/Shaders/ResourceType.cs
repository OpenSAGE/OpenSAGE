using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ResourceType
    {
        public static readonly ResourceType[] DefaultTypes =
        {
            new ResourceType("texture2D", 0),
            new ResourceType("texture2DArray", 0),
            new ResourceType("utexture2D", 0),
            new ResourceType("sampler", 0),

            new ResourceType("float", 4),
            new ResourceType("uint", 4),

            new ResourceType("vec2", 8),

            new ResourceType("vec3", 12),

            new ResourceType("vec4", 16),

            new ResourceType("mat4", 64)
        };

        public string Name { get; }
        public ResourceTypeMember[] Members { get; }
        public uint Size { get; }

        public ResourceType(string name, uint size, params ResourceTypeMember[] members)
        {
            Name = name;
            Members = members;
            Size = size;
        }

        public ResourceTypeMember GetMember(string name)
        {
            var result = Members.FirstOrDefault(x => x.Name == name);
            if (result == null)
            {
                throw new InvalidOperationException($"Unknown member '{name}' in type '{Name}'.");
            }
            return result;
        }

        public static ResourceType FromReflectionType(string typeName, Dictionary<string, ShaderReflectionType> allTypes, List<ResourceType> typeCache)
        {
            var existingType = typeCache.FirstOrDefault(x => x.Name == typeName);
            if (existingType != null)
            {
                return existingType;
            }

            if (!allTypes.TryGetValue(typeName, out var type))
            {
                throw new InvalidOperationException($"Data type '{typeName}' not found.");
            }

            var members = new ResourceTypeMember[type.Members.Length];

            var size = 0u;
            for (var i = 0; i < type.Members.Length; i++)
            {
                var typeMember = type.Members[i];
                var memberType = FromReflectionType(typeMember.Type, allTypes, typeCache);
                members[i] = new ResourceTypeMember(
                    typeMember.Name,
                    memberType,
                    typeMember.Offset);
                size += members[i].Size;
            }

            var result = new ResourceType(type.Name, size, members);

            typeCache.Add(result);

            return result;
        }
    }

    
}
