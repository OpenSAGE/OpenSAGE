using System.IO;
using System.Numerics;
using OpenSage.Data.Sav;
using OpenSage.Graphics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic
{
    public sealed class GhostObject
    {
        private GameObject _gameObject;

        private ObjectGeometry _geometryType;
        private bool _geometryIsSmall;
        private float _geometryMajorRadius;
        private float _geometryMinorRadius;

        private float _angle;
        private Vector3 _position;

        internal void Load(SaveFileReader reader, GameLogic gameLogic, Game game)
        {
            reader.ReadVersion(1);
            reader.ReadVersion(1);

            var objectId = reader.ReadObjectID();
            _gameObject = gameLogic.GetObjectById(objectId);

            _geometryType = reader.ReadEnum<ObjectGeometry>();

            // Sometimes there's a 0xC0, when it should be 0x0.
            _geometryIsSmall = reader.ReadByte() == 1;

            _geometryMajorRadius = reader.ReadSingle();
            _geometryMinorRadius = reader.ReadSingle();

            _angle = reader.ReadSingle();
            _position = reader.ReadVector3();

            var unknown6 = reader.ReadUInt32();
            if (unknown6 != 0)
            {
                throw new InvalidDataException();
            }

            var unknown7 = reader.ReadUInt32();
            if (unknown7 != 0)
            {
                throw new InvalidDataException();
            }

            var unknown8 = reader.ReadUInt32();
            if (unknown8 != 0)
            {
                throw new InvalidDataException();
            }

            for (var i = 0; i < 16; i++)
            {
                var numModels = reader.ReadByte();

                if (numModels > 0 && i != 2)
                {
                    throw new InvalidDataException();
                }

                var modelInstances = new ModelInstance[numModels];

                for (var j = 0; j < numModels; j++)
                {
                    var modelName = reader.ReadAsciiString();

                    var model = game.AssetStore.Models.GetByName(modelName);
                    var modelInstance = model.CreateInstance(game.AssetStore.LoadContext);
                    modelInstances[j] = modelInstance;

                    var scale = reader.ReadSingle();
                    if (scale != 1.0f)
                    {
                        throw new InvalidDataException();
                    }

                    var houseColor = reader.ReadColorRgba();
                    // TODO: Use house color.

                    reader.ReadVersion(1);

                    var modelTransform = reader.ReadMatrix4x3Transposed();

                    modelInstance.SetWorldMatrix(modelTransform.ToMatrix4x4());

                    var numMeshes = reader.ReadUInt32();
                    if (numMeshes > 0 && numMeshes != model.SubObjects.Length)
                    {
                        throw new InvalidDataException();
                    }

                    for (var k = 0; k < numMeshes; k++)
                    {
                        var meshName = reader.ReadAsciiString();
                        var meshBool = reader.ReadBoolean();
                        var meshTransform = reader.ReadMatrix4x3Transposed();

                        if (meshName != model.SubObjects[k].FullName)
                        {
                            throw new InvalidDataException();
                        }

                        // TODO: meshTransform is actually absolute, not relative.
                        modelInstance.RelativeBoneTransforms[model.SubObjects[k].Bone.Index] = meshTransform.ToMatrix4x4();
                    }
                }
            }

            var unknown = reader.ReadBoolean();
            if (unknown)
            {
                reader.ReadByte();
                reader.ReadUInt32();
            }
        }
    }
}
