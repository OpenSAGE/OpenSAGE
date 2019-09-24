using System.Collections.Generic;
using OpenSage.Content.Loaders;

namespace OpenSage.Logic.Object
{
    public sealed class GameObjectCollection : DisposableBase
    {
        private readonly AssetLoadContext _loadContext;
        private readonly List<GameObject> _items;
        private readonly Player _civilianPlayer;

        public IReadOnlyList<GameObject> Items => _items;

        internal GameObjectCollection(AssetLoadContext loadContext, Player civilianPlayer)
        {
            _loadContext = loadContext;
            _items = new List<GameObject>();
            _civilianPlayer = civilianPlayer;
        }

        public GameObject Add(string typeName, Player player)
        {
            return Add(_loadContext.AssetStore.ObjectDefinitions.GetByName(typeName), player);
        }

        public GameObject Add(string typeName)
        {
            return Add(typeName, _civilianPlayer);
        }

        public GameObject Add(ObjectDefinition objectDefinition, Player player)
        {
            var gameObject = AddDisposable(new GameObject(objectDefinition, _loadContext, player, this));
            _items.Add(gameObject);
            return gameObject;
        }

        public GameObject Add(ObjectDefinition objectDefinition)
        {
            return Add(objectDefinition, _civilianPlayer);
        }

        public GameObject Add(GameObject gameObject)
        {
            AddDisposable(gameObject);
            _items.Add(gameObject);
            return gameObject;
        }

        // TODO: This is probably not how real SAGE works.
        public int GetObjectId(GameObject gameObject)
        {
            return _items.IndexOf(gameObject) + 1;
        }

        public GameObject GetObjectById(int objectId)
        {
            return _items[objectId - 1];
        }

    }
}
