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

        public uint DrawableID { get; internal set; }

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

            _transformMatrix = reader.ReadMatrix4x3();

            var hasSelectionFlashHelper = _selectionFlashHelper != null;
            reader.ReadBoolean(ref hasSelectionFlashHelper);
            if (hasSelectionFlashHelper)
            {
                _selectionFlashHelper ??= new ColorFlashHelper();
                _selectionFlashHelper.Load(reader);
            }

            var hasScriptedFlashHelper = _scriptedFlashHelper != null;
            reader.ReadBoolean(ref hasScriptedFlashHelper);
            if (hasScriptedFlashHelper)
            {
                _scriptedFlashHelper ??= new ColorFlashHelper();
                _scriptedFlashHelper.Load(reader);
            }

            reader.ReadEnum(ref _objectDecalType);

            var unknownFloat1 = reader.ReadSingle();
            if (unknownFloat1 != 1)
            {
                throw new InvalidStateException();
            }

            _unknownFloat2 = reader.ReadSingle(); // 0, 1

            _unknownFloat3 = reader.ReadSingle(); // 0, 1

            _unknownFloat4 = reader.ReadSingle(); // 0, 1

            var unknownFloat5 = reader.ReadSingle();
            if (unknownFloat5 != 0)
            {
                throw new InvalidStateException();
            }

            _unknownFloat6 = reader.ReadSingle(); // 0, 1

            var objectId = reader.ReadUInt32();
            if (objectId != GameObject.ID)
            {
                throw new InvalidStateException();
            }

            _unknownInt1 = reader.ReadUInt32();

            _unknownInt2 = reader.ReadUInt32(); // 0, 1

            _unknownInt3 = reader.ReadUInt32();

            _unknownInt4 = reader.ReadUInt32();

            _unknownInt5 = reader.ReadUInt32();

            _unknownInt6 = reader.ReadUInt32();

            reader.ReadBoolean(ref _hasUnknownFloats);
            if (_hasUnknownFloats)
            {
                for (var j = 0; j < 19; j++)
                {
                    _unknownFloats[j] = reader.ReadSingle();
                }
            }

            LoadModules(reader);

            _unknownInt7 = reader.ReadUInt32();

            _flashFrameCount = reader.ReadUInt32();
            _flashColor = reader.ReadColorRgba();

            reader.ReadBoolean(ref _unknownBool1);
            reader.ReadBoolean(ref _unknownBool2);

            reader.SkipUnknownBytes(4);

            reader.ReadBoolean(ref _someMatrixIsIdentity);

            _someMatrix = reader.ReadMatrix4x3(false);

            var unknownFloat10 = reader.ReadSingle();
            if (unknownFloat10 != 1)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(8);

            var hasAnimation2D = _animation != null;
            reader.ReadBoolean(ref hasAnimation2D);
            if (hasAnimation2D)
            {
                var animation2DName = _animation?.Template.Name;
                reader.ReadAsciiString(ref animation2DName);

                reader.SkipUnknownBytes(4);

                var animation2DName2 = animation2DName;
                reader.ReadAsciiString(ref animation2DName2);
                if (animation2DName2 != animation2DName)
                {
                    throw new InvalidStateException();
                }

                var animationTemplate = reader.AssetStore.Animations.GetByName(animation2DName);

                _animation = new Animation(animationTemplate);
                _animation.Load(reader);
            }

            var unknownBool2 = true;
            reader.ReadBoolean(ref unknownBool2);
            if (!unknownBool2)
            {
                throw new InvalidStateException();
            }
        }

        private void LoadModules(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            ushort numModuleGroups = 0;
            reader.ReadUInt16(ref numModuleGroups);

            for (var i = 0; i < numModuleGroups; i++)
            {
                ushort numModules = 0;
                reader.ReadUInt16(ref numModules);

                for (var moduleIndex = 0; moduleIndex < numModules; moduleIndex++)
                {
                    var moduleTag = "";
                    reader.ReadAsciiString(ref moduleTag);
                    var module = GetModuleByTag(moduleTag);

                    reader.BeginSegment($"{module.GetType().Name} module in game object {GameObject.Definition.Name}");

                    module.Load(reader);

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
