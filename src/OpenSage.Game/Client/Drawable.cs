using System;
using System.Collections.Generic;
using System.IO;
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

        public uint DrawableID { get; private set; }

        private ColorFlashHelper _selectionFlashHelper;
        private ColorFlashHelper _scriptedFlashHelper;

        private ObjectDecalType _objectDecalType;

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

            DrawableID = reader.ReadUInt32();

            var modelConditionFlags = reader.ReadBitArray<ModelConditionFlag>();
            CopyModelConditionFlags(modelConditionFlags);

            var transform = reader.ReadMatrix4x3();

            var hasSelectionFlashHelper = reader.ReadBoolean();
            if (hasSelectionFlashHelper)
            {
                _selectionFlashHelper ??= new ColorFlashHelper();
                _selectionFlashHelper.Load(reader);
            }

            var hasScriptedFlashHelper = reader.ReadBoolean();
            if (hasScriptedFlashHelper)
            {
                _scriptedFlashHelper ??= new ColorFlashHelper();
                _scriptedFlashHelper.Load(reader);
            }

            _objectDecalType = reader.ReadEnum<ObjectDecalType>();

            for (var i = 0; i < 6; i++)
            {
                var unknownFloat = reader.ReadSingle();
            }

            var objectId = reader.ReadUInt32();
            if (objectId != GameObject.ID)
            {
                throw new InvalidDataException();
            }

            var unknownInt1 = reader.ReadUInt32();
            if (unknownInt1 != 0 && unknownInt1 != 1 && unknownInt1 != 2 && unknownInt1 != 3)
            {
                throw new InvalidDataException();
            }

            for (var i = 0; i < 5; i++)
            {
                var unknownInt = reader.ReadUInt32();
                if (unknownInt != 0 && unknownInt != 1)
                {
                    throw new InvalidDataException();
                }
            }

            var unknownBool4 = reader.ReadBoolean();
            if (unknownBool4)
            {
                for (var j = 0; j < 19; j++)
                {
                    reader.ReadSingle();
                }
            }

            LoadModules(reader);

            var unknownInt4 = reader.ReadUInt32();
            if (unknownInt4 != 0)
            {
                throw new InvalidDataException();
            }

            var unknownInt5 = reader.ReadUInt32();
            if (unknownInt5 != 0)
            {
                throw new InvalidDataException();
            }

            var unknownInt6 = reader.ReadUInt32();

            var unknownBool5 = reader.ReadBoolean();

            for (var i = 0; i < 5; i++)
            {
                var unknown = reader.ReadByte();
                if (unknown != 0)
                {
                    throw new InvalidDataException();
                }
            }

            var unknownBool6 = reader.ReadBoolean();

            var unknownMatrix = reader.ReadMatrix4x3(false);

            var unknownFloat2 = reader.ReadSingle();
            if (unknownFloat2 != 1)
            {
                throw new InvalidDataException();
            }

            var unknownInt2 = reader.ReadUInt32();
            if (unknownInt2 != 0)
            {
                throw new InvalidDataException();
            }

            var unknownInt3 = reader.ReadUInt32();
            if (unknownInt3 != 0)
            {
                throw new InvalidDataException();
            }

            var unknownBool1 = reader.ReadBoolean();

            if (unknownBool1)
            {
                var animation2DName = reader.ReadAsciiString();

                var unknown = reader.ReadUInt32();
                if (unknown != 0)
                {
                    throw new InvalidDataException();
                }

                var animation2DName2 = reader.ReadAsciiString();
                if (animation2DName2 != animation2DName)
                {
                    throw new InvalidDataException();
                }
            }

            var unknownBool2 = reader.ReadBoolean();
            if (!unknownBool2)
            {
                throw new InvalidDataException();
            }
        }

        private void LoadModules(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var numModuleGroups = reader.ReadUInt16();
            for (var i = 0; i < numModuleGroups; i++)
            {
                var numModules = reader.ReadUInt16();
                for (var moduleIndex = 0; moduleIndex < numModules; moduleIndex++)
                {
                    var moduleTag = reader.ReadAsciiString();

                    reader.BeginSegment();

                    var module = GetModuleByTag(moduleTag);
                    module.Load(reader.Inner);

                    reader.EndSegment();
                }
            }
        }
    }

    public enum ObjectDecalType
    {
        HordeInfantry = 1,
        HordeVehicle = 3,
        Crate = 5,
        None = 6,
    }
}
