using System.Collections.Generic;

namespace OpenSage.Logic.Object
{
    public sealed class GameObjectCollection : DisposableBase
    {
        private readonly GameContext _gameContext;

        private GameLogic GameLogic => _gameContext.GameLogic;

        public IEnumerable<GameObject> Items => GameLogic.Objects;

        internal GameObjectCollection(GameContext gameContext)
        {
            _gameContext = gameContext;
        }

        public GameObject Add(ObjectDefinition objectDefinition, Player player) => GameLogic.CreateObject(objectDefinition, player);

        public GameObject GetObjectById(uint objectId) => GameLogic.GetObjectById(objectId);

        public bool TryGetObjectByName(string name, out GameObject gameObject) => GameLogic.TryGetObjectByName(name, out gameObject);

        public void AddNameLookup(GameObject gameObject) => GameLogic.AddNameLookup(gameObject);

        public void DestroyObject(GameObject gameObject) => GameLogic.DestroyObject(gameObject);

        public void DeleteDestroyed() => GameLogic.DeleteDestroyed();
    }
}
