﻿using System.Collections.Generic;
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
            var gameObject = AddDisposable(new GameObject(objectDefinition, _contentManager, player, this));
            _items.Add(gameObject);
            return gameObject;
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
