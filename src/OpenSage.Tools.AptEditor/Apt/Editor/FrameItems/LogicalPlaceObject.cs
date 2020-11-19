using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Mathematics;

namespace OpenSage.Tools.AptEditor.Apt.Editor.FrameItems
{
    internal class LogicalPlaceObject
    {
        private readonly Action<IEditAction> _edit;
        private readonly PlaceObject _linkedPlaceObject;

        public bool ModifyingExisting { get; private set; }
        public int? Character { get; private set; }
        public bool HasTransfrom => _linkedPlaceObject.Flags.HasFlag(PlaceObjectFlags.HasMatrix);
        public float Rotation { get; private set; }
        public float Skew { get; private set; }
        public Vector2 Scale { get; private set; }
        public Vector2 Translation { get; private set; }
        public float MatrixError { get; private set; }
        public Vector4? ColorTransform { get; private set; }
        public float? Ratio { get; private set; }
        public string? Name { get; private set; }
        public List<ClipEvent>? ClipEvents { get; private set; }

        public LogicalPlaceObject(Action<IEditAction> edit, PlaceObject po)
        {
            _edit = edit;
            _linkedPlaceObject = po;

            ModifyingExisting = po.Flags.HasFlag(PlaceObjectFlags.Move);

            if (po.Flags.HasFlag(PlaceObjectFlags.HasCharacter))
            {
                Character = po.Character;
            }

            if (po.Flags.HasFlag(PlaceObjectFlags.HasMatrix))
            {
                GetRotationSkewAndScale(GetMatrix3x2(po.RotScale));
                Translation = po.Translation;
            }

            if (po.Flags.HasFlag(PlaceObjectFlags.HasColorTransform))
            {
                ColorTransform = po.Color.ToVector4();
            }

            if (po.Flags.HasFlag(PlaceObjectFlags.HasRatio))
            {
                Ratio = po.Ratio;
            }

            if (po.Flags.HasFlag(PlaceObjectFlags.HasName))
            {
                Name = po.Name;
            }

            if (po.Flags.HasFlag(PlaceObjectFlags.HasClipAction))
            {
                ClipEvents = po.ClipEvents;
            }
        }

        public void SetCharacter(int? character)
        {
            MakeEdit("Set PlaceObject Character", character, value =>
            {
                var previous = Character;
                Character = value;
                _linkedPlaceObject.SetCharacter(value);
                return previous;
            });
        }

        public void SetRotation(float rotation)
        {
            CreateTransform("PlaceObject Rotation", () => Rotation = rotation);
        }

        public void SetSkew(float skew)
        {
            CreateTransform("PlaceObject Scale", () => Skew = skew);
        }

        public void SetScale(Vector2 scale)
        {
            CreateTransform("PlaceObject Scale", () => Scale = scale);
        }

        public void SetTranslation(Vector2 translation)
        {
            CreateTransform("PlaceObject Translation", () => Translation = translation);
        }

        public void SetColor(in Vector4? color)
        {
            var desc = $"{(color.HasValue ? string.Empty : "Disable ")}PlaceObject Color";
            MakeEdit(desc, color, value =>
            {
                var previous = ColorTransform;
                ColorTransform = value;
                ColorRgba? rgba = value is Vector4 v
                    ? new ColorRgbaF(v.X, v.Y, v.Z, v.W).ToColorRgba()
                    : null;
                _linkedPlaceObject.SetColorTransform(rgba);
                return previous;
            });
        }

        public void InitTransform()
        {
            CreateTransform("Init PlaceObject Transform", () => { });
        }

        public void DisableTransform()
        {
            EditTransform("Disable PlaceObject Transform", () => null);
        }

        public void SetName(string? name)
        {
            MakeEdit("PlaceObject Name", name, value =>
            {
                var previous = Name;
                Name = value;
                _linkedPlaceObject.SetName(value);
                return previous;
            });
        }

        private void MakeEdit<T>(string description, in T state, Func<T, T> edit)
        {
            _edit(new EditAction<T>(edit, state, description));
        }

        private void CreateTransform(string description, System.Action edit)
        {
            EditTransform(description, () =>
            {
                if (!HasTransfrom)
                {
                    Rotation = 0;
                    Skew = 0;
                    Scale = Vector2.One;
                }
                edit();
                MatrixError = 0;
                var transform = CalculateTransformation();
                transform.Translation = Translation;
                return transform;
            });
        }

        private void EditTransform(string description, Func<Matrix3x2?> edit)
        {
            var currentError = MatrixError;
            Matrix3x2? currentTransform = HasTransfrom
                ? GetMatrix3x2(_linkedPlaceObject.RotScale, _linkedPlaceObject.Translation)
                : null;
            void Edit()
            {
                MatrixError = 0;
                _linkedPlaceObject.SetTransform(edit());
            }
            void Restore()
            {
                MatrixError = currentError;
                _linkedPlaceObject.SetTransform(currentTransform);
            }
            var id = _linkedPlaceObject.Character + description + _linkedPlaceObject.GetHashCode();
            _edit(new MergeableEdit(TimeSpan.FromSeconds(1), id, Edit, Restore, description));
        }

        private Matrix3x2 CalculateTransformation()
        {
            return Matrix3x2.CreateRotation(Rotation) * Matrix3x2.CreateSkew(0, Skew) * Matrix3x2.CreateScale(Scale);
        }

        private void GetRotationSkewAndScale(in Matrix3x2 matrix)
        {
            // QR Decomposition
            var c1 = new Vector2(matrix.M11, matrix.M21);
            var c2 = new Vector2(matrix.M12, matrix.M22);

            // Columns of Q Matrix
            var e1 = Vector2.Normalize(c1);
            var e2 = Vector2.Normalize(c2 - Projection(c2, c1));

            var xSign = 1;
            var qMatrix = new Matrix3x2(e1.X, e2.X, e1.Y, e2.Y, 0, 0);
            if (qMatrix.GetDeterminant() == -1)
            {
                qMatrix *= new Matrix3x2(-1, 0, 0, 1, 0, 0);
                xSign = -1;
            }
            var rMatrix = new Matrix3x2(Vector2.Dot(c1, e1), Vector2.Dot(c2, e1), 0, Vector2.Dot(c2, e2), 0, 0);

            Rotation = MathF.Atan2(qMatrix.M12, qMatrix.M11);
            Skew = rMatrix.M12;
            Scale = new Vector2(rMatrix.M11 * xSign, rMatrix.M22);
            var calculated = CalculateTransformation();
            MatrixError = MatrixAbsDifference(calculated, matrix);
        }

        private static Matrix3x2 GetMatrix3x2(Matrix2x2 m, Vector2 t = default)
        {
            return new Matrix3x2(m.M11, m.M12, m.M21, m.M22, t.X, t.Y);
        }

        private static Vector2 Projection(Vector2 vector, Vector2 projectOn)
        {
            return Vector2.Dot(projectOn, vector) * projectOn / projectOn.LengthSquared();
        }

        private static float MatrixAbsDifference(in Matrix3x2 m1, in Matrix3x2 m2)
        {
            var diff = m1 - m2;
            var diffs = new[] { diff.M11, diff.M12, diff.M21, diff.M22 };
            return diffs.Select(value => MathF.Abs(value)).Sum();
        }
    }
}
