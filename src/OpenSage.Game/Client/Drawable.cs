using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Sav;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Client
{
    public sealed class Drawable : Entity
    {
        private readonly ObjectDefinition _definition;
        private readonly GameContext _gameContext;

        private readonly List<string> _hiddenDrawModules;
        private readonly Dictionary<string, bool> _hiddenSubObjects;
        private readonly Dictionary<string, bool> _shownSubObjects;

        public readonly GameObject GameObject;

        public readonly IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates;

        public readonly BitArray<ModelConditionFlag> ModelConditionFlags;

        // Doing this with a field and a property instead of an auto-property allows us to have a read-only public interface,
        // while simultaneously supporting fast (non-allocating) iteration when accessing the list within the class.
        public IReadOnlyList<DrawModule> DrawModules => _drawModules;
        private readonly List<DrawModule> _drawModules;

        private readonly List<ClientUpdateModule> _clientUpdateModules;

        internal Drawable(ObjectDefinition objectDefinition, GameContext gameContext, GameObject gameObject)
        {
            _definition = objectDefinition;
            _gameContext = gameContext;
            GameObject = gameObject;

            ModelConditionFlags = new BitArray<ModelConditionFlag>();

            var drawModules = new List<DrawModule>();
            foreach (var drawDataContainer in objectDefinition.Draws.Values)
            {
                var drawModuleData = (DrawModuleData) drawDataContainer.Data;
                var drawModule = AddDisposable(drawModuleData.CreateDrawModule(this, gameContext));
                if (drawModule != null)
                {
                    // TODO: This will never be null once we've implemented all the draw modules.
                    AddModule(drawDataContainer.Tag, drawModule);
                    drawModules.Add(drawModule);
                }
            }
            _drawModules = drawModules;

            ModelConditionStates = drawModules
                .SelectMany(x => x.ModelConditionStates)
                .Distinct()
                .OrderBy(x => x.NumBitsSet)
                .ToList();

            _hiddenDrawModules = new List<string>();
            _hiddenSubObjects = new Dictionary<string, bool>();
            _shownSubObjects = new Dictionary<string, bool>();

            _clientUpdateModules = new List<ClientUpdateModule>();
            foreach (var clientUpdateModuleDataContainer in objectDefinition.ClientUpdates.Values)
            {
                var clientUpdateModuleData = (ClientUpdateModuleData) clientUpdateModuleDataContainer.Data;
                var clientUpdateModule = AddDisposable(clientUpdateModuleData.CreateModule(this, gameContext));
                if (clientUpdateModule != null)
                {
                    // TODO: This will never be null once we've implemented all the draw modules.
                    AddModule(clientUpdateModuleDataContainer.Tag, clientUpdateModule);
                    _clientUpdateModules.Add(clientUpdateModule);
                }
            }
        }

        internal void CopyModelConditionFlags(BitArray<ModelConditionFlag> newFlags)
        {
            ModelConditionFlags.CopyFrom(newFlags);
        }

        // TODO: This probably shouldn't be here.
        public Matrix4x4? GetWeaponFireFXBoneTransform(WeaponSlot slot, int index)
        {
            foreach (var drawModule in _drawModules)
            {
                var fireFXBone = drawModule.GetWeaponFireFXBone(slot);
                if (fireFXBone != null)
                {
                    var (modelInstance, bone) = drawModule.FindBone(fireFXBone + (index + 1).ToString("D2"));
                    if (bone != null)
                    {
                        return modelInstance.AbsoluteBoneTransforms[bone.Index];
                    }
                    break;
                }
            }

            return null;
        }

        // TODO: This probably shouldn't be here.
        public Matrix4x4? GetWeaponLaunchBoneTransform(WeaponSlot slot, int index)
        {
            foreach (var drawModule in _drawModules)
            {
                var fireFXBone = drawModule.GetWeaponLaunchBone(slot);
                if (fireFXBone != null)
                {
                    var (modelInstance, bone) = drawModule.FindBone(fireFXBone + (index + 1).ToString("D2"));
                    if (bone != null)
                    {
                        return modelInstance.AbsoluteBoneTransforms[bone.Index];
                    }
                    break;
                }
            }

            return null;
        }

        public (ModelInstance modelInstance, ModelBone bone) FindBone(string boneName)
        {
            foreach (var drawModule in _drawModules)
            {
                var (modelInstance, bone) = drawModule.FindBone(boneName);
                if (bone != null)
                {
                    return (modelInstance, bone);
                }
            }

            return (null, null);
        }

        internal void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime, in Matrix4x4 worldMatrix, in MeshShaderResources.RenderItemConstantsPS renderItemConstantsPS)
        {
            var castsShadow = false;
            switch (_definition.Shadow)
            {
                case ObjectShadowType.ShadowVolume:
                case ObjectShadowType.ShadowVolumeNew:
                    castsShadow = true;
                    break;
            }

            // Update all draw modules
            foreach (var drawModule in _drawModules)
            {
                if (_hiddenDrawModules.Contains(drawModule.Tag))
                {
                    continue;
                }

                drawModule.UpdateConditionState(ModelConditionFlags, _gameContext.Random);
                drawModule.Update(gameTime);
                drawModule.SetWorldMatrix(worldMatrix);
                drawModule.BuildRenderList(
                    renderList,
                    camera,
                    castsShadow,
                    renderItemConstantsPS,
                    _shownSubObjects,
                    _hiddenSubObjects);
            }
        }

        public void HideDrawModule(string module)
        {
            if (!_hiddenDrawModules.Contains(module))
            {
                _hiddenDrawModules.Add(module);
            }
        }

        public void ShowDrawModule(string module)
        {
            _hiddenDrawModules.Remove(module);
        }

        public void HideSubObject(string subObject)
        {
            if (subObject == null) return;

            if (!_hiddenSubObjects.ContainsKey(subObject))
            {
                _hiddenSubObjects.Add(subObject, false);
            }
            _shownSubObjects.Remove(subObject);
        }

        public void HideSubObjectPermanently(string subObject)
        {
            if (subObject == null) return;

            if (!_hiddenSubObjects.ContainsKey(subObject))
            {
                _hiddenSubObjects.Add(subObject, true);
            }
            else
            {
                _hiddenSubObjects[subObject] = true;
            }
            _shownSubObjects.Remove(subObject);
        }


        public void ShowSubObject(string subObject)
        {
            if (subObject == null) return;

            if (!_shownSubObjects.ContainsKey(subObject))
            {
                _shownSubObjects.Add(subObject, false);
            }
            _hiddenSubObjects.Remove(subObject);
        }

        public void ShowSubObjectPermanently(string subObject)
        {
            if (subObject == null) return;

            if (!_shownSubObjects.ContainsKey(subObject))
            {
                _shownSubObjects.Add(subObject, true);
            }
            else
            {
                _shownSubObjects[subObject] = true;
            }
            _hiddenSubObjects.Remove(subObject);
        }

        internal void Destroy()
        {
            foreach (var drawModule in _drawModules)
            {
                drawModule.Dispose();
            }
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(5);

            var drawableID = reader.ReadUInt32();

            reader.ReadByte();

            var numModelConditionFlags = reader.ReadUInt32();
            for (var j = 0; j < numModelConditionFlags; j++)
            {
                var modelConditionFlag = reader.ReadAsciiString();
            }

            var transform = reader.ReadMatrix4x3();

            var unknownBool = reader.ReadBoolean();
            var unknownBool2 = reader.ReadBoolean();
            if (unknownBool)
            {
                for (var j = 0; j < 9; j++)
                {
                    reader.ReadSingle();
                }
                reader.__Skip(19);
            }

            reader.__Skip(56);

            var unknownBool3 = reader.ReadBoolean();
            if (unknownBool3)
            {
                for (var j = 0; j < 19; j++)
                {
                    reader.ReadSingle();
                }
            }

            reader.__Skip(3);

            var numDrawModules = reader.ReadUInt16();
            for (var moduleIndex = 0; moduleIndex < numDrawModules; moduleIndex++)
            {
                var moduleTag = reader.ReadAsciiString();

                reader.BeginSegment();

                var module = GetModuleByTag(moduleTag);
                module.Load(reader.Inner);

                reader.EndSegment();
            }

            var numClientUpdates = reader.ReadUInt16();
            for (var moduleIndex = 0; moduleIndex < numClientUpdates; moduleIndex++)
            {
                var moduleTag = reader.ReadAsciiString();

                reader.BeginSegment();

                var module = GetModuleByTag(moduleTag);
                module.Load(reader.Inner);

                reader.EndSegment();
            }

            reader.__Skip(81);
        }
    }
}
