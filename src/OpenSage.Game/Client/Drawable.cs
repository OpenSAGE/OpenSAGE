#nullable enable

using System;
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

        private ModuleBase GetModuleByTag(string tag)
        {
            return _tagToModuleLookup[tag];
        }

        private readonly GameContext _gameContext;

        private readonly List<string> _hiddenDrawModules;
        private readonly Dictionary<string, bool> _hiddenSubObjects;
        private readonly Dictionary<string, bool> _shownSubObjects;

        public Dictionary<string, bool> ShownSubObjects => _shownSubObjects;
        public Dictionary<string, bool> HiddenSubObjects => _hiddenSubObjects;

        public readonly GameObject GameObject;
        public readonly ObjectDefinition Definition;

        public readonly IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates;

        public readonly BitArray<ModelConditionFlag> ModelConditionFlags;

        public ReadOnlySpan<DrawModule> DrawModules
        {
            get
            {
                if (ModelConditionFlags.BitsChanged)
                {
                    foreach (var drawModule in _drawModules)
                    {
                        drawModule.UpdateConditionState(ModelConditionFlags, _gameContext.Random);
                    }
                    ModelConditionFlags.BitsChanged = false;
                }
                return _drawModules;
            }
        }

        private readonly DrawModule[] _drawModules;

        private readonly ClientUpdateModule[] _clientUpdateModules;

        private uint _id;

        public uint ID
        {
            get => _id;
            internal set
            {
                var oldDrawableId = _id;

                _id = value;

                _gameContext.GameClient.OnDrawableIdChanged(this, oldDrawableId);
            }
        }

        public uint GameObjectID => GameObject?.ID ?? 0u;

        private Matrix4x3 _transformMatrix;

        private ColorFlashHelper? _selectionFlashHelper;
        private ColorFlashHelper? _scriptedFlashHelper;

        public ObjectDecalType ObjectDecalType;

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

        private List<Animation> _animations = [];
        public IReadOnlyList<Animation> Animations => _animations;
        private readonly Dictionary<AnimationType, Animation> _animationMap = [];

        private Vector3? _selectionFlashColor;
        private Vector3 SelectionFlashColor => _selectionFlashColor ??=
            _gameContext.AssetLoadContext.AssetStore.GameData.Current.SelectionFlashHouseColor
                ? GameObject.Owner.Color.ToVector3()
                : new Vector3( // the ini comments say "zero leaves color unaffected, 4.0 is purely saturated", however the value in the ini is 0.5 and value in sav files is 0.25, so either it's hardcoded or the comments are wrong and I choose configurability
                    Math.Clamp(_gameContext.AssetLoadContext.AssetStore.GameData.Current.SelectionFlashSaturationFactor / 2f, 0, 1),
                    Math.Clamp(_gameContext.AssetLoadContext.AssetStore.GameData.Current.SelectionFlashSaturationFactor / 2f, 0, 1),
                    Math.Clamp(_gameContext.AssetLoadContext.AssetStore.GameData.Current.SelectionFlashSaturationFactor / 2f, 0, 1));

        internal Drawable(ObjectDefinition objectDefinition, GameContext gameContext, GameObject gameObject)
        {
            Definition = objectDefinition;
            _gameContext = gameContext;
            GameObject = gameObject;

            ModelConditionFlags = new BitArray<ModelConditionFlag>();

            // the casing on the object names doesn't always match
            _hiddenSubObjects = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
            _shownSubObjects = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);

            var drawModules = new List<DrawModule>();
            foreach (var drawDataContainer in objectDefinition.Draws.Values)
            {
                var drawModuleData = (DrawModuleData) drawDataContainer.Data;
                var drawModule = AddDisposable(drawModuleData.CreateDrawModule(this, gameContext));
                if (drawModule != null)
                {
                    // TODO: This will never be null once we've implemented all the draw modules.
                    drawModule.Tag = drawDataContainer.Tag;
                    drawModules.Add(drawModule);
                    _tagToModuleLookup.Add(drawDataContainer.Tag, drawModule);
                    AddDisposable(drawModule);
                }
            }
            _drawModules = drawModules.ToArray();

            ModelConditionStates = _drawModules
                .SelectMany(x => x.ModelConditionStates)
                .Distinct()
                .OrderBy(x => x.NumBitsSet)
                .ToList();

            _hiddenDrawModules = new List<string>();

            var clientUpdateModules = new List<ClientUpdateModule>();
            foreach (var clientUpdateModuleDataContainer in objectDefinition.ClientUpdates.Values)
            {
                var clientUpdateModuleData = (ClientUpdateModuleData) clientUpdateModuleDataContainer.Data;
                var clientUpdateModule = AddDisposable(clientUpdateModuleData.CreateModule(this, gameContext));
                if (clientUpdateModule != null)
                {
                    // TODO: This will never be null once we've implemented all the draw modules.
                    clientUpdateModule.Tag = clientUpdateModuleDataContainer.Tag;
                    clientUpdateModules.Add(clientUpdateModule);
                    _tagToModuleLookup.Add(clientUpdateModuleDataContainer.Tag, clientUpdateModule);
                }
            }
            _clientUpdateModules = clientUpdateModules.ToArray();
        }

        // as far as I can tell nothing like this is stored in the actual game, so I'm not sure how this was handled for network games where logic ticks weren't linked to fps (if at all - you can't save a network game, after all)
        private LogicFrame _lastSelectionFlashFrame;

        public void LogicTick()
        {
            var currentFrame = _gameContext.GameLogic.CurrentFrame;
            if (currentFrame > _lastSelectionFlashFrame)
            {
                _lastSelectionFlashFrame = currentFrame;
                _selectionFlashHelper?.StepFrame();
            }
        }

        public void TriggerSelection()
        {
            _selectionFlashHelper ??= new ColorFlashHelper(); // this is dynamically created in generals - units don't get one until they have been selected
            _selectionFlashHelper.StartSelection(SelectionFlashColor);
        }

        public void AddAnimation(AnimationType animationName)
        {
            if (_animationMap.ContainsKey(animationName))
            {
                return;
            }

            var animationTemplate = _gameContext.Game.AssetStore.Animations.GetByName(Animation.AnimationTypeToName(animationName));
            var animation = new Animation(animationTemplate);

            _animations.Add(animation);
            _animationMap[animationName] = animation;
        }

        public void RemoveAnimation(AnimationType animationType)
        {
            if (_animationMap.Remove(animationType))
            {
                // todo: this can result in a lot of allocations
                _animations = _animations.Where(a => a.AnimationType != animationType).ToList();
            }
        }

        internal void CopyModelConditionFlags(BitArray<ModelConditionFlag> newFlags)
        {
            ModelConditionFlags.CopyFrom(newFlags);
        }

        // TODO: This probably shouldn't be here.
        public Matrix4x4? GetWeaponFireFXBoneTransform(WeaponSlot slot, int index)
        {
            foreach (var drawModule in DrawModules)
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
            foreach (var drawModule in DrawModules)
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

        public (ModelInstance? modelInstance, ModelBone? bone) FindBone(string boneName)
        {
            foreach (var drawModule in DrawModules)
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
            foreach (var drawModule in DrawModules)
            {
                if (_hiddenDrawModules.Contains(drawModule.Tag))
                {
                    continue;
                }

                var pitchShift = renderItemConstantsPS;
                if (_selectionFlashHelper?.IsActive == true)
                {
                    pitchShift = pitchShift with
                    {
                        TintColor = pitchShift.TintColor + _selectionFlashHelper.CurrentColor,
                    };
                }

                drawModule.Update(gameTime);
                drawModule.SetWorldMatrix(worldMatrix);
                drawModule.BuildRenderList(
                    renderList,
                    camera,
                    castsShadow,
                    pitchShift,
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

        public void SetAnimationDuration(LogicFrameSpan frames)
        {
            foreach (var drawModule in DrawModules)
            {
                drawModule.SetAnimationDuration(frames);
            }
        }

        public void SetSupplyBoxesRemaining(float boxPercentage)
        {
            foreach (var drawModule in DrawModules)
            {
                drawModule.SetSupplyBoxesRemaining(boxPercentage);
            }
        }

        internal void Destroy()
        {
            foreach (var drawModule in DrawModules)
            {
                drawModule.Dispose();
            }
        }

        public void Persist(StatePersister reader)
        {
            var version = reader.PersistVersion(7);

            var id = ID;
            reader.PersistUInt32(ref id);
            if (reader.Mode == StatePersistMode.Read)
            {
                ID = id;
            }

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

            reader.PersistEnum(ref ObjectDecalType);

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

            var animation2DCount = (byte) _animations.Count;
            reader.PersistByte(ref animation2DCount);

            reader.BeginArray("Animations");

            if (reader.Mode == StatePersistMode.Read)
            {
                _animations = [];
            }

            for (var i = 0; i < animation2DCount; i++)
            {
                reader.BeginObject();
                var animation = reader.Mode == StatePersistMode.Read ? null : _animations[i];
                var animation2DName = animation?.Template.Name;
                reader.PersistAsciiString(ref animation2DName);

                var unknownInt = 0;
                reader.PersistInt32(ref unknownInt); // was non-zero for bombtimed - potentially a frame?

                var animation2DName2 = animation2DName;
                reader.PersistAsciiString(ref animation2DName2);
                if (animation2DName2 != animation2DName)
                {
                    throw new InvalidStateException();
                }

                if (reader.Mode == StatePersistMode.Read)
                {
                    var animationTemplate = reader.AssetStore.Animations.GetByName(animation2DName);
                    animation = new Animation(animationTemplate);
                    _animations.Add(animation);
                }

                reader.PersistObject(animation);

                reader.EndObject();
            }

            reader.EndArray();

            var unknownBool3 = true;
            reader.PersistBoolean(ref unknownBool3);
            if (!unknownBool3)
            {
                throw new InvalidStateException();
            }

            if (version >= 7)
            {
                var unknownBool4 = true;
                reader.PersistBoolean(ref unknownBool4);
                if (!unknownBool4)
                {
                    throw new InvalidStateException();
                }

                reader.SkipUnknownBytes(1);
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

            PersistModuleGroup(reader, "DrawModules", DrawModules);
            PersistModuleGroup(reader, "ClientUpdateModules", new ReadOnlySpan<ClientUpdateModule>(_clientUpdateModules));

            reader.EndObject();
        }

        private void PersistModuleGroup<T>(StatePersister reader, string groupName, ReadOnlySpan<T> modules)
            where T : ModuleBase
        {
            reader.BeginObject(groupName);

            var numModules = (ushort)modules.Length;
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
        HordeInfantry = 1, // exhorde.dds
        NationalismInfantry = 2, // exhorde_up.dds
        HordeVehicle = 3, // exhordeb.dds
        NationalismVehicle = 4, // exhordeb_up.dds
        Crate = 5, // exjunkcrate.dds
        None = 6,
        Unknown1 = 7,
        Unknown2 = 8,
    }
}
