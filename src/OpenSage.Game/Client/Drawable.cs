using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Client
{
    public sealed class Drawable : Entity, IPersistableObject
    {
        private readonly Dictionary<string, ModuleBase> _tagToModuleLookup = new();

        private void AddDrawModule(string tag, DrawModule module)
        {
            module.Tag = tag;
            _drawModules.Add(module);
            _tagToModuleLookup.Add(tag, module);
        }

        private void AddClientUpdateModule(string tag, ClientUpdateModule module)
        {
            module.Tag = tag;
            _clientUpdateModules.Add(module);
            _tagToModuleLookup.Add(tag, module);
        }

        private ModuleBase GetModuleByTag(string tag)
        {
            return _tagToModuleLookup[tag];
        }

        private readonly GameContext _gameContext;

        private readonly List<string> _hiddenDrawModules;
        private readonly Dictionary<string, bool> _hiddenSubObjects;
        private readonly Dictionary<string, bool> _shownSubObjects;

        public readonly GameObject GameObject;
        public readonly ObjectDefinition Definition;

        public readonly IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates;

        public readonly BitArray<ModelConditionFlag> ModelConditionFlags;

        // Doing this with a field and a property instead of an auto-property allows us to have a read-only public interface,
        // while simultaneously supporting fast (non-allocating) iteration when accessing the list within the class.
        public IReadOnlyList<DrawModule> DrawModules => _drawModules;
        private readonly List<DrawModule> _drawModules;

        private readonly List<ClientUpdateModule> _clientUpdateModules;

        // TODO: Make this a property.
        public uint DrawableID;

        public uint GameObjectID => GameObject?.ID ?? 0u;

        private Matrix4x3 _transformMatrix;

        private ColorFlashHelper _selectionFlashHelper;
        private ColorFlashHelper _scriptedFlashHelper;

        private ObjectDecalType _objectDecalType;

        private float _unknownFloat2;
        private float _unknownFloat3;
        private float _unknownFloat4;
        private float _unknownFloat6;

        private uint _unknownInt1;
        private uint _unknownInt2;
        private uint _unknownInt3;
        private uint _unknownInt4;
        private uint _unknownInt5;
        private uint _unknownInt6;

        private bool _hasUnknownFloats;
        private readonly float[] _unknownFloats = new float[19];

        private uint _unknownInt7;

        private uint _flashFrameCount;
        private ColorRgba _flashColor;

        private bool _unknownBool1;
        private bool _unknownBool2;

        private bool _someMatrixIsIdentity;
        private Matrix4x3 _someMatrix;

        private Animation _animation;

        internal Drawable(ObjectDefinition objectDefinition, GameContext gameContext, GameObject gameObject)
        {
            Definition = objectDefinition;
            _gameContext = gameContext;
            GameObject = gameObject;

            ModelConditionFlags = new BitArray<ModelConditionFlag>();

            _drawModules = new List<DrawModule>();
            foreach (var drawDataContainer in objectDefinition.Draws.Values)
            {
                var drawModuleData = (DrawModuleData) drawDataContainer.Data;
                var drawModule = AddDisposable(drawModuleData.CreateDrawModule(this, gameContext));
                if (drawModule != null)
                {
                    // TODO: This will never be null once we've implemented all the draw modules.
                    AddDrawModule(drawDataContainer.Tag, drawModule);
                }
            }

            ModelConditionStates = _drawModules
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
                    AddClientUpdateModule(clientUpdateModuleDataContainer.Tag, clientUpdateModule);
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
            switch (Definition.Shadow)
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

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(5);

            reader.PersistUInt32(ref DrawableID);

            var modelConditionFlags = ModelConditionFlags.Clone();
            reader.PersistBitArray(ref modelConditionFlags);
            if (reader.Mode == StatePersistMode.Read)
            {
                CopyModelConditionFlags(modelConditionFlags);
            }

            reader.PersistMatrix4x3(ref _transformMatrix);

            var hasSelectionFlashHelper = _selectionFlashHelper != null;
            reader.PersistBoolean(ref hasSelectionFlashHelper);
            if (hasSelectionFlashHelper)
            {
                _selectionFlashHelper ??= new ColorFlashHelper();
                reader.PersistObject(_selectionFlashHelper);
            }

            var hasScriptedFlashHelper = _scriptedFlashHelper != null;
            reader.PersistBoolean(ref hasScriptedFlashHelper);
            if (hasScriptedFlashHelper)
            {
                _scriptedFlashHelper ??= new ColorFlashHelper();
                reader.PersistObject(_scriptedFlashHelper);
            }

            reader.PersistEnum(ref _objectDecalType);

            var unknownFloat1 = 1.0f;
            reader.PersistSingle(ref unknownFloat1);
            if (unknownFloat1 != 1)
            {
                throw new InvalidStateException();
            }

            reader.PersistSingle(ref _unknownFloat2); // 0, 1
            reader.PersistSingle(ref _unknownFloat3); // 0, 1
            reader.PersistSingle(ref _unknownFloat4); // 0, 1

            reader.SkipUnknownBytes(4);

            reader.PersistSingle(ref _unknownFloat6); // 0, 1

            var objectId = GameObjectID;
            reader.PersistUInt32(ref objectId);
            if (objectId != GameObjectID)
            {
                throw new InvalidStateException();
            }

            reader.PersistUInt32(ref _unknownInt1);
            reader.PersistUInt32(ref _unknownInt2); // 0, 1
            reader.PersistUInt32(ref _unknownInt3);
            reader.PersistUInt32(ref _unknownInt4);
            reader.PersistUInt32(ref _unknownInt5);
            reader.PersistUInt32(ref _unknownInt6);

            reader.PersistBoolean(ref _hasUnknownFloats);
            if (_hasUnknownFloats)
            {
                reader.PersistArray(_unknownFloats, static (StatePersister persister, ref float item) =>
                {
                    persister.PersistSingleValue(ref item);
                });
            }

            PersistModules(reader);

            reader.PersistUInt32(ref _unknownInt7);

            reader.PersistUInt32(ref _flashFrameCount);
            reader.PersistColorRgba(ref _flashColor);

            reader.PersistBoolean(ref _unknownBool1);
            reader.PersistBoolean(ref _unknownBool2);

            reader.SkipUnknownBytes(4);

            reader.PersistBoolean(ref _someMatrixIsIdentity);

            reader.PersistMatrix4x3(ref _someMatrix, false);

            var unknownFloat10 = 1.0f;
            reader.PersistSingle(ref unknownFloat10);
            if (unknownFloat10 != 1)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(4);

            var unknownInt8 = 0u;
            reader.PersistUInt32(ref unknownInt8); // 232...frameSomething?

            var hasAnimation2D = _animation != null;
            reader.PersistBoolean(ref hasAnimation2D);
            if (hasAnimation2D)
            {
                var animation2DName = _animation?.Template.Name;
                reader.PersistAsciiString(ref animation2DName);

                reader.SkipUnknownBytes(4);

                var animation2DName2 = animation2DName;
                reader.PersistAsciiString(ref animation2DName2);
                if (animation2DName2 != animation2DName)
                {
                    throw new InvalidStateException();
                }

                if (reader.Mode == StatePersistMode.Read)
                {
                    var animationTemplate = reader.AssetStore.Animations.GetByName(animation2DName);
                    _animation = new Animation(animationTemplate);
                }

                reader.PersistObject(_animation);
            }

            var unknownBool3 = true;
            reader.PersistBoolean(ref unknownBool3);
            if (!unknownBool3)
            {
                throw new InvalidStateException();
            }
        }

        private void PersistModules(StatePersister reader)
        {
            reader.BeginObject("ModuleGroups");

            reader.PersistVersion(1);

            ushort numModuleGroups = 2;
            reader.PersistUInt16(ref numModuleGroups);

            if (numModuleGroups != 2)
            {
                throw new InvalidStateException();
            }

            PersistModuleGroup(reader, "DrawModules", _drawModules);
            PersistModuleGroup(reader, "ClientUpdateModules", _clientUpdateModules);

            reader.EndObject();
        }

        private void PersistModuleGroup<T>(StatePersister reader, string groupName, List<T> modules)
            where T : ModuleBase
        {
            reader.BeginObject(groupName);

            var numModules = (ushort)modules.Count;
            reader.PersistUInt16(ref numModules);

            reader.BeginArray("Modules");
            for (var moduleIndex = 0; moduleIndex < numModules; moduleIndex++)
            {
                reader.BeginObject();

                ModuleBase module;
                if (reader.Mode == StatePersistMode.Read)
                {
                    var moduleTag = "";
                    reader.PersistAsciiString(ref moduleTag);
                    module = GetModuleByTag(moduleTag);
                }
                else
                {
                    module = modules[moduleIndex];
                    var moduleTag = module.Tag;
                    reader.PersistAsciiString(ref moduleTag);
                }

                reader.BeginSegment($"{module.GetType().Name} module in game object {Definition.Name}");

                reader.PersistObject(module);

                reader.EndSegment();

                reader.EndObject();
            }
            reader.EndArray();

            reader.EndObject();
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
