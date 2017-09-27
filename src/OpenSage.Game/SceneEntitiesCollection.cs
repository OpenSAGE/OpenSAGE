using System;
using System.Collections.ObjectModel;

namespace OpenSage
{
    public sealed class SceneEntitiesCollection : Collection<Entity>
    {
        private readonly Scene _scene;

        public SceneEntitiesCollection(Scene scene)
        {
            _scene = scene;
        }

        private void SetScene(Entity item)
        {
            if (item.SceneDirect != null)
            {
                throw new InvalidOperationException();
            }
            item.SceneDirect = _scene;
        }

        private void RemoveScene(Entity item)
        {
            if (item.SceneDirect != _scene)
            {
                throw new InvalidOperationException();
            }
            item.SceneDirect = null;
        }

        protected override void SetItem(int index, Entity item)
        {
            SetScene(item);
            base.SetItem(index, item);
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                RemoveScene(item);
            }
            base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            RemoveScene(this[index]);
            base.RemoveItem(index);
        }

        protected override void InsertItem(int index, Entity item)
        {
            SetScene(item);
            base.InsertItem(index, item);
        }
    }
}
