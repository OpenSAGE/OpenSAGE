using System.Collections.Generic;
using System.Numerics;
using OpenSage.Graphics;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Logic
{
    public sealed class GhostObject : IPersistableObject
    {
        public uint OriginalObjectId;

        private GameObject _gameObject;

        private ObjectGeometry _geometryType;
        private bool _geometryIsSmall;
        private float _geometryMajorRadius;
        private float _geometryMinorRadius;

        private float _angle;
        private Vector3 _position;

        private readonly List<ModelInstance>[] _modelsPerPlayer;

        private bool _hasUnknownThing;
        private byte _unknownByte;
        private uint _unknownInt;

        public GhostObject()
        {
            _modelsPerPlayer = new List<ModelInstance>[Player.MaxPlayers];
            for (var i = 0; i < _modelsPerPlayer.Length; i++)
            {
                _modelsPerPlayer[i] = new List<ModelInstance>();
            }
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            reader.PersistVersion(1);
            reader.EndObject();

            uint objectId = _gameObject?.ID ?? 0u;
            reader.PersistObjectID("ObjectId", ref objectId);
            if (reader.Mode == StatePersistMode.Read)
            {
                _gameObject = reader.Game.Scene3D.GameLogic.GetObjectById(objectId);
            }

            reader.PersistEnum("GeometryType", ref _geometryType);

            // Sometimes there's a 0xC, which is probably uninitialized data.
            byte geometryIsSmall = _geometryIsSmall ? (byte)1 : (byte)0;
            reader.PersistByte("GeometryIsSmall", ref geometryIsSmall);
            _geometryIsSmall = geometryIsSmall == 1;

            reader.PersistSingle("GeometryMajorRadius", ref _geometryMajorRadius);
            reader.PersistSingle("GeometryMinorRadius", ref _geometryMinorRadius);
            reader.PersistSingle("Angle", ref _angle);
            reader.PersistVector3("Position", ref _position);

            reader.SkipUnknownBytes(12);

            reader.BeginArray("ModelsPerPlayer");
            for (var i = 0; i < Player.MaxPlayers; i++)
            {
                reader.BeginObject();

                byte numModels = (byte)_modelsPerPlayer[i].Count;
                reader.PersistByte("NumModels", ref numModels);

                reader.BeginArray("Models");
                for (var j = 0; j < numModels; j++)
                {
                    reader.BeginObject();

                    var modelName = reader.Mode == StatePersistMode.Write
                        ? _modelsPerPlayer[i][j].Model.Name
                        : "";
                    reader.PersistAsciiString("ModelName", ref modelName);

                    Model model;
                    ModelInstance modelInstance;
                    if (reader.Mode == StatePersistMode.Read)
                    {
                        model = reader.AssetStore.Models.GetByName(modelName);
                        modelInstance = model.CreateInstance(reader.AssetStore.LoadContext);

                        _modelsPerPlayer[i].Add(modelInstance);
                    }
                    else
                    {
                        modelInstance = _modelsPerPlayer[i][j];
                        model = modelInstance.Model;
                    }

                    var scale = 1.0f;
                    reader.PersistSingle("Scale", ref scale);
                    if (scale != 1.0f)
                    {
                        throw new InvalidStateException();
                    }

                    reader.PersistColorRgba("HouseColor", ref modelInstance.HouseColor);

                    reader.PersistVersion(1);

                    var modelTransform = reader.Mode == StatePersistMode.Write
                        ? Matrix4x3.FromMatrix4x4(modelInstance.GetWorldMatrix())
                        : Matrix4x3.Identity;
                    reader.PersistMatrix4x3("ModelTransform", ref modelTransform, readVersion: false);

                    if (reader.Mode == StatePersistMode.Read)
                    {
                        modelInstance.SetWorldMatrix(modelTransform.ToMatrix4x4());
                    }

                    var numMeshes = (uint)model.SubObjects.Length;
                    reader.PersistUInt32("NumMeshes", ref numMeshes);
                    if (numMeshes > 0 && numMeshes != model.SubObjects.Length)
                    {
                        throw new InvalidStateException();
                    }

                    reader.BeginArray("Meshes");
                    for (var k = 0; k < numMeshes; k++)
                    {
                        reader.BeginObject();

                        var meshName = model.SubObjects[k].FullName;
                        reader.PersistAsciiString("MeshName", ref meshName);

                        if (meshName != model.SubObjects[k].FullName)
                        {
                            throw new InvalidStateException();
                        }

                        reader.PersistBoolean("UnknownBool", ref modelInstance.UnknownBools[k]);

                        var meshTransform = reader.Mode == StatePersistMode.Write
                            ? Matrix4x3.FromMatrix4x4(modelInstance.RelativeBoneTransforms[model.SubObjects[k].Bone.Index])
                            : Matrix4x3.Identity;
                        reader.PersistMatrix4x3("MeshTransform", ref meshTransform, readVersion: false);

                        if (reader.Mode == StatePersistMode.Read)
                        {
                            // TODO: meshTransform is actually absolute, not relative.
                            modelInstance.RelativeBoneTransforms[model.SubObjects[k].Bone.Index] = meshTransform.ToMatrix4x4();
                        }

                        reader.EndObject();
                    }
                    reader.EndArray();

                    reader.EndObject();
                }
                reader.EndArray();

                reader.EndObject();
            }
            reader.EndArray();

            reader.PersistBoolean("HasUnknownThing", ref _hasUnknownThing);
            if (_hasUnknownThing)
            {
                reader.PersistByte("UnknownByte", ref _unknownByte);
                reader.PersistUInt32("UnknownInt", ref _unknownInt);
            }
        }
    }
}
