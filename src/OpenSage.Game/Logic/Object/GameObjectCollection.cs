using System;
using System.Collections.Generic;

namespace OpenSage.Logic.Object
{
    public sealed class GameObjectCollection : DisposableBase
    {
        private readonly GameContext _gameContext;
        private readonly List<GameObject> _items;
        private readonly Dictionary<string, GameObject> _nameLookup;
        private readonly Player _civilianPlayer;
        private readonly Navigation.Navigation _navigation;

        public IReadOnlyList<GameObject> Items => _items;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal GameObjectCollection(
            GameContext gameContext,
            Player civilianPlayer,
            Navigation.Navigation navigation)
        {
            _gameContext = gameContext;
            _items = new List<GameObject>();
            _nameLookup = new Dictionary<string, GameObject>();
            _civilianPlayer = civilianPlayer;
            _navigation = navigation;
        }

        public GameObject Add(string typeName, Player player)
        {
            var definition = _gameContext.AssetLoadContext.AssetStore.ObjectDefinitions.GetByName(typeName);

            if (definition == null)
            {
                Logger.Warn($"Skipping unknown GameObject \"{typeName}\"");
                return null;
            }

            return Add(definition, player);
        }

        public GameObject Add(string typeName)
        {
            return Add(typeName, _civilianPlayer);
        }

        public GameObject Add(ObjectDefinition objectDefinition, Player player)
        {
            var gameObject = AddDisposable(new GameObject(objectDefinition, _gameContext, player, this));
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

        public List<int> GetObjectIds(IEnumerable<GameObject> gameObjects)
        {
            var objIds = new List<int>();
            foreach (var gameObject in gameObjects)
            {
                objIds.Add(GetObjectId(gameObject));
            }

            return objIds;
        }

        public GameObject GetObjectById(int objectId)
        {
            return _items[objectId - 1];
        }

        public bool TryGetObjectByName(string name, out GameObject gameObject)
        {
            return _nameLookup.TryGetValue(name, out gameObject);
        }

        public void AddNameLookup(GameObject gameObject)
        {
            _nameLookup[gameObject.Name ?? throw new ArgumentException("Cannot add lookup for unnamed object.")] = gameObject;
        }
    }
}
