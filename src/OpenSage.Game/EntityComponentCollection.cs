using System.Collections.ObjectModel;

namespace OpenSage
{
    public sealed class EntityComponentCollection : Collection<EntityComponent>
    {
        private readonly Entity _entity;

        public EntityComponentCollection(Entity entity)
        {
            _entity = entity;
        }

        private void AttachComponent(EntityComponent component)
        {
            if (component.Entity != null)
            {
                throw new System.InvalidOperationException();
            }
            component.Entity = _entity;
            _entity.Scene?.Game?.OnEntityComponentsAdded(new[] { component });
        }

        private void DetachComponent(EntityComponent component)
        {
            if (component.Entity != _entity)
            {
                throw new System.InvalidOperationException();
            }
            component.Entity = null;
            _entity.Scene?.Game?.OnEntityComponentsRemoved(new[] { component });
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                DetachComponent(item);
            }
            base.ClearItems();
        }

        protected override void InsertItem(int index, EntityComponent item)
        {
            AttachComponent(item);
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            DetachComponent(this[index]);
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, EntityComponent item)
        {
            AttachComponent(item);
            base.SetItem(index, item);
        }
    }
}
