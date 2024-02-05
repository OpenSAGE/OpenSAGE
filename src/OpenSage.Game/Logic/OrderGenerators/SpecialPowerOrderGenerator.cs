using System;
using System.Numerics;
using OpenSage.Input;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;
using OpenSage.Mathematics;
using OpenSage.Terrain;

namespace OpenSage.Logic.OrderGenerators
{
    public enum SpecialPowerTarget
    {
        Location,
        AllyObject,
        EnemyObject,
        NeutralObject,
    };

    public enum SpecialPowerTargetType
    {
        Location,
        Object,
    }

    public record struct SpecialPowerCursorInformation(
        SpecialPower SpecialPower,
        BitArray<SpecialPowerTarget> AllowedTargets,
        string CursorName,
        string InvalidCursorName);

    public sealed class SpecialPowerOrderGenerator : OrderGenerator, IDisposable
    {
        private readonly SpecialPower _specialPower;
        private readonly SpecialPowerCursorInformation _cursorInformation;
        private readonly GameData _config;
        private readonly Player _player;
        private readonly GameContext _gameContext;
        private readonly SpecialPowerTargetType _targetType;
        private readonly Scene3D _scene;

        private readonly DecalHandle _decalHandle;

        private SpecialPowerTarget Target => TargetForGameObject(WorldObject);

        public override bool CanDrag => false;

        internal SpecialPowerOrderGenerator(
           SpecialPowerCursorInformation cursorInformation,
           GameData config,
           Player player,
           GameContext gameContext,
           SpecialPowerTargetType targetType,
           Scene3D scene,
           in TimeInterval time) : base(gameContext.Game)
        {
            _specialPower = cursorInformation.SpecialPower;
            _cursorInformation = cursorInformation;
            _targetType = targetType;
            _scene = scene;
            _config = config;
            _player = player;
            _gameContext = gameContext;

            // TODO: Improve this check.
            var radiusCursors = _scene.GameContext.AssetLoadContext.AssetStore.InGameUI.Current.RadiusCursors;
            var radiusCursorName = _specialPower.Type.ToString();
            if (radiusCursors.TryGetValue(radiusCursorName, out var radiusCursor))
            {
                _decalHandle = scene.Terrain.RadiusCursorDecals.AddDecal(
                    radiusCursor.DecalTemplate,
                    _specialPower.RadiusCursorRadius,
                    time);
            }
        }

        public override OrderGeneratorResult TryActivate(Scene3D scene, KeyModifiers keyModifiers)
        {
            var player = scene.LocalPlayer;
            var playerIdx = scene.GetPlayerIndex(player);
            Order specialPowerOrder;
            if (_targetType == SpecialPowerTargetType.Location)
            {
                specialPowerOrder = Order.CreateSpecialPowerAtLocation(playerIdx, _specialPower.InternalId, WorldPosition);
            }
            else
            {
                specialPowerOrder = Order.CreateSpecialPowerAtObject(playerIdx, _specialPower.InternalId);
            }
            return OrderGeneratorResult.SuccessAndExit(new[] { specialPowerOrder });
        }

        public override void UpdatePosition(Vector2 mousePosition, Vector3 worldPosition)
        {
            base.UpdatePosition(mousePosition, worldPosition);

            if (_decalHandle != null)
            {
                _scene.Terrain.RadiusCursorDecals.SetDecalPosition(_decalHandle, worldPosition.Vector2XY());
            }
        }

        private SpecialPowerTarget TargetForGameObject(GameObject? target)
        {

            if (target == null)
            {
                return SpecialPowerTarget.Location;
            }

            var owner = target.Owner;
            if (_player.Enemies.Contains(owner))
            {
                return SpecialPowerTarget.EnemyObject;
            }

            if (_player == owner || _player.Allies.Contains(owner))
            {
                return SpecialPowerTarget.AllyObject;
            }

            return SpecialPowerTarget.NeutralObject;
        }

        public override string GetCursor(KeyModifiers keyModifiers)
        {
            // todo: this should ultimately probably be decided by the individual special power, as some are specific to specific target types but don't mention it in the inis
            return _cursorInformation.AllowedTargets.Get(Target) ? _cursorInformation.CursorName : _cursorInformation.InvalidCursorName;
        }

        public void Dispose()
        {
            if (_decalHandle != null)
            {
                _scene.Terrain.RadiusCursorDecals.RemoveDecal(_decalHandle);
            }
        }
    }
}
