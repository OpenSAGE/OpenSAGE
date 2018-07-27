using System.Collections.Generic;
using OpenSage.Content;

namespace OpenSage.Logic.Object
{
    public sealed class GameObjectCollection : DisposableBase
    {
        private readonly ContentManager _contentManager;
        private readonly List<GameObject> _items;

        public IReadOnlyList<GameObject> Items => _items;

        public GameObjectCollection(ContentManager contentManager)
        {
            _contentManager = contentManager;
            _items = new List<GameObject>();
        }

        public GameObject Add(ObjectDefinition objectDefinition, Player player)
        {
            var gameObject = AddDisposable(new GameObject(objectDefinition, _contentManager, player));
            _items.Add(gameObject);
            return gameObject;
        }

        public GameObject Add(string typeName)
        {
            var gameObject = AddDisposable(_contentManager.InstantiateObject(typeName));

            if (gameObject != null)
            {
                _items.Add(gameObject);
            }

            return gameObject;
        }

        public GameObject Add(GameObject gameObject)
        {
            AddDisposable(gameObject);
            _items.Add(gameObject);
            return gameObject;
        }
    }
}
