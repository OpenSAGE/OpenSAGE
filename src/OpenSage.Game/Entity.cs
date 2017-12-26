using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Logic.Object;

namespace OpenSage
{
    /// <summary>
    /// Entities are the building blocks of scenes. A scene contains a hierarchy of entities.
    /// Each entity contains at least a <see cref="TransformComponent"/>, and may contain multiple
    /// other components.
    /// </summary>
    public sealed class Entity
    {
        // TODO: Move to ObjectFactory.
        public static Entity FromObjectDefinition(ObjectDefinition objectDefinition)
        {
            var result = new Entity();
            result.Name = objectDefinition.Name;

            result.AddComponent(new ObjectComponent());

            foreach (var draw in objectDefinition.Draws)
            {
                var drawEntity = new Entity();
                drawEntity.Name = draw.Tag;
                result.AddChild(drawEntity);

                switch (draw)
                {
                    case W3dModelDrawModuleData modelDrawData:
                        drawEntity.Components.Add(new W3dModelDraw(modelDrawData));
                        break;

                    // TODO
                }
            }

            return result;
        }

        public string Name { get; set; }

        // TODO: Cache this property in ancestors and descendants.
        private bool _visible = true;
        public bool Visible
        {
            get { return _visible; }
            set
            {
                _visible = value;

                ClearCachedVisibleInHierarchyRecursive();
            }
        }

        private void ClearCachedVisibleInHierarchyRecursive()
        {
            _visibleInHierarchy = null;

            foreach (var child in GetChildren())
            {
                child.ClearCachedVisibleInHierarchyRecursive();
            }
        }

        private bool? _visibleInHierarchy;
        internal bool VisibleInHierarchy
        {
            get
            {
                if (_visibleInHierarchy == null)
                {
                    _visibleInHierarchy = true;

                    var parent = this;
                    while (parent != null)
                    {
                        if (!parent.Visible)
                        {
                            _visibleInHierarchy = false;
                            break;
                        }
                        parent = parent.GetParent();
                    }
                }
                return _visibleInHierarchy.Value;
            }
        }

        private Scene _scene;

        internal Scene SceneDirect
        {
            get { return _scene; }
            set { _scene = value; }
        }

        public Scene Scene
        {
            get { return FindRoot()._scene; }
            set
            {
                if (GetParent() != null)
                {
                    throw new InvalidOperationException();
                }

                var oldScene = _scene;
                if (oldScene == value)
                {
                    return;
                }

                oldScene?.Entities.Remove(this);
                value?.Entities.Add(this);
            }
        }

        public EntityComponentCollection Components { get; }

        public TransformComponent Transform { get; }

        public Entity()
        {
            Components = new EntityComponentCollection(this);
            Components.Add(Transform = new TransformComponent());
        }

        public Entity GetParent()
        {
            return Transform.Parent?.Entity;
        }

        public IEnumerable<Entity> GetChildren()
        {
            return Transform.Children.Select(x => x.Entity).ToList();
        }

        public Entity FindRoot()
        {
            var root = this;
            Entity parent;
            while ((parent = root.GetParent()) != null)
            {
                root = parent;
            }
            return root;
        }

        public void AddChild(Entity child)
        {
            Transform.Children.Add(child.Transform);
        }

        public void AddComponent(EntityComponent component)
        {
            Components.Add(component);
        }

        public T GetComponent<T>()
            where T : EntityComponent
        {
            return Components.OfType<T>().FirstOrDefault();
        }

        public IEnumerable<T> GetComponents<T>()
            where T : EntityComponent
        {
            return Components.OfType<T>();
        }

        public IEnumerable<Entity> GetSelfAndDescendants()
        {
            yield return this;

            foreach (var child in GetChildren())
            {
                foreach (var descendant in child.GetSelfAndDescendants())
                {
                    yield return descendant;
                }
            }
        }

        public IEnumerable<T> GetComponentsInSelfAndDescendants<T>()
            where T : EntityComponent
        {
            foreach (var entity in GetSelfAndDescendants())
            {
                foreach (var component in entity.GetComponents<T>())
                {
                    yield return component;
                }
            }
        }
    }
}
