using System;
using System.Collections.Generic;

namespace OpenSage.Logic.Object
{
    public sealed class LocomotorSet : IPersistableObject
    {
        private readonly GameObject _gameObject;
        private readonly List<Locomotor> _locomotors;
        private Surfaces _surfaces;

        public LocomotorSet(GameObject gameObject)
        {
            _gameObject = gameObject;
            _locomotors = new List<Locomotor>();
        }

        public void Initialize(LocomotorSetTemplate locomotorSetTemplate)
        {
            _surfaces = Surfaces.None;

            foreach (var locomotorTemplateReference in locomotorSetTemplate.Locomotors)
            {
                var locomotorTemplate = locomotorTemplateReference.Value;

                _locomotors.Add(new Locomotor(
                    _gameObject,
                    locomotorTemplate,
                    locomotorSetTemplate.Speed));

                _surfaces |= locomotorTemplate.Surfaces;
            }
        }

        public void Reset()
        {
            _locomotors.Clear();
        }

        public Locomotor GetLocomotorForSurfaces(Surfaces surfaces)
        {
            foreach (var locomotor in _locomotors)
            {
                if ((locomotor.LocomotorTemplate.Surfaces & surfaces) != 0)
                {
                    return locomotor;
                }
            }
            return _locomotors[0];
        }

        public Locomotor GetLocomotor(string locomotorTemplateName)
        {
            foreach (var locomotor in _locomotors)
            {
                if (locomotor.LocomotorTemplate.Name == locomotorTemplateName)
                {
                    return locomotor;
                }
            }

            throw new InvalidOperationException();
        }

        public void Persist(StatePersister reader)
        {
            if (reader.Mode == StatePersistMode.Read)
            {
                _locomotors.Clear();
            }

            reader.PersistVersion(1);

            var numLocomotorTemplates = (ushort) _locomotors.Count;
            reader.PersistUInt16(ref numLocomotorTemplates, "NumLocomotors");

            reader.BeginArray("Locomotors");
            if (reader.Mode == StatePersistMode.Read)
            {
                for (var i = 0; i < numLocomotorTemplates; i++)
                {
                    reader.BeginObject();

                    var locomotorTemplateName = "";
                    reader.PersistAsciiString(ref locomotorTemplateName, "TemplateName");

                    var locomotorTemplate = reader.AssetStore.LocomotorTemplates.GetByName(locomotorTemplateName);

                    var locomotor = new Locomotor(_gameObject, locomotorTemplate, 100);

                    reader.PersistObject(locomotor);

                    _locomotors.Add(locomotor);

                    reader.EndArray();
                }
            }
            else
            {
                foreach (var locomotor in _locomotors)
                {
                    reader.BeginObject();

                    var templateName = locomotor.LocomotorTemplate.Name;
                    reader.PersistAsciiString(ref templateName);

                    reader.PersistObject(locomotor);

                    reader.EndObject();
                }
            }
            reader.EndArray();

            reader.PersistEnumFlags(ref _surfaces);

            reader.SkipUnknownBytes(1);
        }
    }
}
