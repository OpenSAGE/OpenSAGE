using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Diagnostics.Util;

namespace OpenSage.Logic.Object
{
    public sealed class GameObjectCollection : DisposableBase
    {
        private readonly GameContext _gameContext;
        private readonly Dictionary<uint, GameObject> _items;
        private readonly Dictionary<string, GameObject> _nameLookup;
        private readonly List<GameObject> _destroyList;

        private uint _nextObjectId;

        public IEnumerable<GameObject> Items => _items.Values;

        private static readonly DistinctLogger Logger = new(NLog.LogManager.GetCurrentClassLogger());

        internal GameObjectCollection(GameContext gameContext)
        {
            _gameContext = gameContext;
            _items = new Dictionary<uint, GameObject>();
            _nameLookup = new Dictionary<string, GameObject>();
            _destroyList = new List<GameObject>();
            _nextObjectId = 1;
        }

        public GameObject Add(ObjectDefinition objectDefinition, Player player)
        {
            if (objectDefinition == null)
            {
                Logger.Warn($"Skipping unknown GameObject");
                return null;
            }

            var gameObject = AddDisposable(new GameObject(objectDefinition, _gameContext, player, this));

            _gameContext.Quadtree?.Insert(gameObject);
            _gameContext.Radar?.AddGameObject(gameObject, _nextObjectId);
            _items.Add(_nextObjectId++, gameObject);

            return gameObject;
        }

        // TODO: This is probably not how real SAGE works.
        public uint GetObjectId(GameObject gameObject)
        {
            return _items.FirstOrDefault(x => x.Value == gameObject).Key;
        }

        public GameObject GetObjectById(uint objectId)
        {
            return _items[objectId];
        }

        public bool TryGetObjectByName(string name, out GameObject gameObject)
        {
            return _nameLookup.TryGetValue(name, out gameObject);
        }

        public void AddNameLookup(GameObject gameObject)
        {
            _nameLookup[gameObject.Name ?? throw new ArgumentException("Cannot add lookup for unnamed object.")] = gameObject;
        }

        public void DestroyObject(GameObject gameObject)
        {
            _destroyList.Add(gameObject);
        }

        public void DeleteDestroyed()
        {
            foreach (var gameObject in _destroyList)
            {
                _gameContext.Quadtree.Remove(gameObject);
                _gameContext.Radar.RemoveGameObject(gameObject);

                gameObject.Drawable.Destroy();

                if (gameObject.Name != null)
                {
                    _nameLookup.Remove(gameObject.Name);
                }

                var objectId = GetObjectId(gameObject);
                _items[objectId] = null;
            }

            _destroyList.Clear();
        }
    }
}
