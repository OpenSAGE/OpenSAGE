using System.Collections.Generic;

namespace OpenSage.Logic.Object
{
    public sealed class ObjectSystem : GameSystem
    {
        private readonly List<ObjectComponent> _objects;

        public ObjectSystem(Game game)
            : base(game)
        {
            RegisterComponentList(_objects = new List<ObjectComponent>());
        }


    }
}
