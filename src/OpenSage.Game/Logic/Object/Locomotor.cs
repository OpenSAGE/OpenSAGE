using System;
using System.Numerics;
using OpenSage.Terrain;

namespace OpenSage.Logic.Object
{
    internal sealed class Locomotor
    {
        private readonly GameObject _gameObject;
        private readonly LocomotorSet _locomotorSet;
        private readonly LocomotorTemplate _locomotorTemplate;

        public Locomotor(GameObject gameObject, LocomotorSet locomotorSet)
        {
            _gameObject = gameObject;
            _locomotorSet = locomotorSet;
            _locomotorTemplate = locomotorSet.Locomotor.Value;
        }

        public void LocalLogicTick(in TimeInterval gameTime, in Vector3 targetPoint, HeightMap heightMap)
        {
            var deltaTime = (float) gameTime.DeltaTime.TotalSeconds;

            var transform = _gameObject.Transform;

            var x = transform.Translation.X;
            var y = transform.Translation.Y;
            var trans = transform.Translation;

            // This locomotor speed is distance/second
            var delta = targetPoint - transform.Translation;
            var distance = GetLocomotorValue(l => l.Speed) * deltaTime;
            if (delta.Length() < distance)
            {
                distance = delta.Length();
            }

            // TODO: Do this properly. Needs to be negative for reverse?
            _gameObject.Speed = GetLocomotorValue(l => l.Speed);

            //var currentAngle = -transform.EulerAngles.Z;
            //var angleDelta = TargetAngle - currentAngle;

            //var d = GetLocomotorValue(l => l.TurnRate) * deltaTime * 0.1f;
            //var newAngle = currentAngle + (angleDelta * d);
            ////var newAngle = currentAngle + d;

            //if (Math.Abs(angleDelta) > 0.1f)
            //{
            //    var pitch = 0.0f;
            //    if (_locomotorTemplate.Appearance == LocomotorAppearance.FourWheels) // TODO
            //    {
            //        var normal = heightMap?.GetNormal(x, y) ?? new Vector3();
            //        pitch = (float) Math.Atan2(Vector3.UnitZ.Y - normal.Y, Vector3.UnitZ.X - normal.X);
            //    }
            //    transform.Rotation = Quaternion.CreateFromYawPitchRoll(pitch, 0.0f, newAngle);
            //}

            var direction = Vector3.Normalize(delta);
            trans += direction * distance;
            trans.Z = heightMap.GetHeight(x, y);
            transform.Translation = trans;
        }

        private float GetLocomotorValue(Func<LocomotorTemplate, float> getValue)
        {
            return (_locomotorSet.Speed / 100.0f) * getValue(_locomotorTemplate);
        }
    }
}
