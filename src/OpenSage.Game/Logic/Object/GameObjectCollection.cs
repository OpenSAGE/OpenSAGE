using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Logic.Object
{
    public sealed class GameObjectCollection : DisposableBase
    {
        private readonly GameContext _gameContext;
        private readonly Dictionary<uint, GameObject> _items;
        private readonly Dictionary<string, GameObject> _nameLookup;
        private readonly List<GameObject> _createList;
        private readonly List<uint> _destroyList;
        private readonly Player _civilianPlayer;
        private readonly Navigation.Navigation _navigation;

        private uint _nextObjectId;

        public IEnumerable<GameObject> Items => _items.Values;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal GameObjectCollection(
            GameContext gameContext,
            Player civilianPlayer,
            Navigation.Navigation navigation)
        {
            _gameContext = gameContext;
            _items = new Dictionary<uint, GameObject>();
            _nameLookup = new Dictionary<string, GameObject>();
            _civilianPlayer = civilianPlayer;
            _destroyList = new List<uint>();
            _createList = new List<GameObject>();
            _navigation = navigation;
            _nextObjectId = 0;
        }

        public GameObject Add(string typeName)
        {
            return Add(typeName, _civilianPlayer);
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

        public GameObject Add(ObjectDefinition objectDefinition)
        {
            return Add(objectDefinition, _civilianPlayer);
        }

        public GameObject Add(ObjectDefinition objectDefinition, Player player)
        {
            var gameObject = AddDisposable(new GameObject(objectDefinition, _gameContext, player, this));
            _createList.Add(gameObject);
            return gameObject;
        }

        // TODO: This is probably not how real SAGE works.
        public uint GetObjectId(GameObject gameObject)
        {
            return _items.FirstOrDefault(x => x.Value == gameObject).Key;
        }

        public List<uint> GetObjectIds(IEnumerable<GameObject> gameObjects)
        {
            var objIds = new List<uint>();
            foreach (var gameObject in gameObjects)
            {
                objIds.Add(GetObjectId(gameObject));
            }

            return objIds;
        }

        public GameObject GetObjectById(uint objectId)
        {
            return _items[objectId];
        }

        public bool TryGetObjectByName(string name, out GameObject gameObject)
        {
            return _nameLookup.TryGetValue(name, out gameObject);
        }

        public List<GameObject> GetObjectsByKindOf(ObjectKinds kindOf)
        {
            var result = new List<GameObject>();
            foreach (var match in _items.Where(x => x.Value.Definition.KindOf.Get(kindOf)))
            {
                result.Add(match.Value);
            }
            return result;
        }

        public void AddNameLookup(GameObject gameObject)
        {
            _nameLookup[gameObject.Name ?? throw new ArgumentException("Cannot add lookup for unnamed object.")] = gameObject;
        }

        public void InsertCreated()
        {
            foreach (var gameObject in _createList)
            {
                _gameContext.Quadtree.Insert(gameObject);
                _gameContext.Radar.AddGameObject(gameObject, _nextObjectId);
                _items.Add(_nextObjectId++, gameObject);
            }
            _createList.Clear();
        }

        public void DeleteDestroyed()
        {
            _destroyList.Clear();
            foreach (var (objectId, gameObject) in _items)
            {
                if (!gameObject.Destroyed)
                {
                    continue;
                }

                _gameContext.Quadtree.Remove(gameObject);
                _gameContext.Radar.RemoveGameObject(gameObject);
                _destroyList.Add(objectId);
            }

            foreach (var objectId in _destroyList)
            {
                _items.Remove(objectId, out var _);
            }
        }
    }
}
