using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ResourceType
    {
        public static readonly ResourceType Texture2D = new ResourceType("texture2D", 0);

        public static readonly ResourceType Float = new ResourceType("float", 4);
        public static readonly ResourceType Int = new ResourceType("int", 4);
        public static readonly ResourceType UInt = new ResourceType("uint", 4);

        public static readonly ResourceType Vec2 = new ResourceType("vec2", 8);
        public static readonly ResourceType Vec3 = new ResourceType("vec3", 12);
        public static readonly ResourceType Vec4 = new ResourceType("vec4", 16);

        public static readonly ResourceType Mat4 = new ResourceType("mat4", 64);

        private readonly Dictionary<string, ResourceTypeMember> _members;

        public readonly string Name;
        public readonly uint Size;

        public ResourceType(string name, uint size, params ResourceTypeMember[] members)
        {
            Name = name;
            _members = members.ToDictionary(x => x.Name);
            Size = size;
        }

        public ResourceTypeMember GetMember(string name)
        {
            if (!_members.TryGetValue(name, out var result))
            {
                throw new InvalidOperationException($"Unknown member '{name}' in type '{Name}'.");
            }
            return result;
        }
    }
}
