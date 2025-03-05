#nullable enable

using System;
using System.IO;
using System.Numerics;
using NLog;
using OpenSage.FileFormats;
using OpenSage.Mathematics;
using OpenSage.Terrain;
using Vulkan;

namespace OpenSage.Data.Map;

public sealed class PolygonTrigger : IPersistableObject
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public required string Name { get; init; }
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public string? LayerName { get; private set; }
    public uint TriggerId { get; private set; }
    public bool IsWater { get; private set; }
    public bool IsRiver { get; private set; }

    /// <summary>
    /// For rivers, this is the index into the array of Points
    /// for the point where the river flows from.
    /// </summary>
    public uint RiverStart { get; private set; }

    // Shared water options
    [AddedIn(SageGame.Bfme)]
    public bool UseAdditiveBlending { get; private set; }
    [AddedIn(SageGame.Bfme)]
    public Vector2 UvScrollSpeed { get; private set; }

    // River-specific options
    [AddedIn(SageGame.Bfme)]
    public string? RiverTexture { get; private set; }
    [AddedIn(SageGame.Bfme)]
    public string? NoiseTexture { get; private set; }
    [AddedIn(SageGame.Bfme)]
    public string? AlphaEdgeTexture { get; private set; }
    [AddedIn(SageGame.Bfme)]
    public string? SparkleTexture { get; private set; }
    [AddedIn(SageGame.Bfme)]
    public string? BumpMapTexture { get; private set; }
    [AddedIn(SageGame.Bfme)]
    public string? SkyTexture { get; private set; }
    [AddedIn(SageGame.Bfme)]
    public byte Unknown { get; private set; }
    [AddedIn(SageGame.Bfme)]
    public ColorRgb RiverColor { get; private set; }
    [AddedIn(SageGame.Bfme)]
    public float RiverAlpha { get; private set; }

    public Point3D[] Points { get; private set; } = [];

    private Rectangle _bounds;
    private bool _boundsNeedsUpdate = true;
    private Rectangle Bounds
    {
        get
        {
            if (_boundsNeedsUpdate)
            {
                UpdateBounds();
            }
            return _bounds;
        }
    }

    private float _radius;
    public float Radius
    {
        get
        {
            if (_boundsNeedsUpdate)
            {
                UpdateBounds();
            }
            return _radius;
        }
    }

    internal static PolygonTrigger? Parse(BinaryReader reader, ushort version)
    {
        var result = new PolygonTrigger
        {
            Name = reader.ReadUInt16PrefixedAsciiString()
        };

        if (version >= 4)
        {
            result.LayerName = reader.ReadUInt16PrefixedAsciiString();
        }

        result.TriggerId = reader.ReadUInt32();

        if (version >= 2)
        {
            result.IsWater = reader.ReadBooleanChecked();
        }

        if (version >= 3)
        {
            result.IsRiver = reader.ReadBooleanChecked();
            result.RiverStart = reader.ReadUInt32();
        }

        // BFME+
        if (version >= 5)
        {
            result.RiverTexture = reader.ReadUInt16PrefixedAsciiString();
            result.NoiseTexture = reader.ReadUInt16PrefixedAsciiString();
            result.AlphaEdgeTexture = reader.ReadUInt16PrefixedAsciiString();
            result.SparkleTexture = reader.ReadUInt16PrefixedAsciiString();
            result.BumpMapTexture = reader.ReadUInt16PrefixedAsciiString();
            result.SkyTexture = reader.ReadUInt16PrefixedAsciiString();

            result.UseAdditiveBlending = reader.ReadBooleanChecked();

            result.RiverColor = reader.ReadColorRgb();

            result.Unknown = reader.ReadByte(); // 0
            if (result.Unknown != 0)
            {
                throw new InvalidDataException();
            }

            result.UvScrollSpeed = reader.ReadVector2();

            result.RiverAlpha = reader.ReadSingle();
        }

        var numPoints = reader.ReadUInt32();

        // ZH Compatibility: Original ZH code logs a warning if there are fewer than 2 points, but that's not a valid polygon.
        // Likely an off-by-one error.
        if (numPoints < 3)
        {
            Logger.Warn($"Polygon trigger '{result.Name}' has less than 3 points, ignoring.");
            return null;
        }

        result.Points = new Point3D[numPoints];

        for (var i = 0; i < numPoints; i++)
        {
            result.Points[i] = reader.ReadPoint3D();
        }

        return result;
    }

    public static PolygonTrigger CreateDefaultWaterArea(uint id)
    {
        // TODO: This should be defined in a more central location
        const int MapXYFactor = 10;
        const int DefaultWaterLevel = 7;

        var trigger = new PolygonTrigger()
        {
            Name = "AutoAddedWaterAreaTrigger",
            IsWater = true,
            TriggerId = id,
            Points = [
                new Point3D(-30 * MapXYFactor, -30 * MapXYFactor, DefaultWaterLevel),
                new Point3D(30 * MapXYFactor, -30 * MapXYFactor, DefaultWaterLevel),
                new Point3D(30 * MapXYFactor, 30 * MapXYFactor, DefaultWaterLevel),
                new Point3D(-30 * MapXYFactor, 30 * MapXYFactor, DefaultWaterLevel)
            ]
        };

        return trigger;
    }

    private void UpdateBounds()
    {
        // Magic constant from ZH
        const int BigInt = 0x7ffff0;
        var topLeft = new Point2D(BigInt, BigInt);
        var bottomRight = new Point2D(-BigInt, -BigInt);

        foreach (var point in Points)
        {
            var point2D = point.ToPoint2D();
            topLeft = Point2D.Min(topLeft, point2D);
            bottomRight = Point2D.Max(bottomRight, point2D);
        }

        _bounds = Rectangle.FromCorners(topLeft, bottomRight);
        _boundsNeedsUpdate = false;

        var halfWidth = (bottomRight.X - topLeft.X) * 0.5f;
        // ZH Compatibility: This is wrong, but it matches the original implementation
        // It should actually be (bottomRight.Y - topLeft.Y) * 0.5f
        // As a result the radius is significantly larger than it should be.
        var halfHeight = (bottomRight.Y + topLeft.Y) * 0.5f;
        _radius = MathF.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight);
    }

    internal void WriteTo(BinaryWriter writer, ushort version)
    {
        writer.WriteUInt16PrefixedAsciiString(Name);
        writer.WriteUInt16PrefixedAsciiString(LayerName);
        writer.Write(TriggerId);
        writer.Write(IsWater);
        writer.Write(IsRiver);
        writer.Write(RiverStart);

        // BFME+
        if (version >= 5)
        {
            writer.WriteUInt16PrefixedAsciiString(RiverTexture);
            writer.WriteUInt16PrefixedAsciiString(NoiseTexture);
            writer.WriteUInt16PrefixedAsciiString(AlphaEdgeTexture);
            writer.WriteUInt16PrefixedAsciiString(SparkleTexture);
            writer.WriteUInt16PrefixedAsciiString(BumpMapTexture);
            writer.WriteUInt16PrefixedAsciiString(SkyTexture);

            writer.Write(UseAdditiveBlending);

            writer.Write(RiverColor);

            writer.Write(Unknown);

            writer.Write(UvScrollSpeed);

            writer.Write(RiverAlpha);
        }

        writer.Write((uint)Points.Length);

        foreach (var point in Points)
        {
            writer.Write(point);
        }
    }

    public Vector3? GetCenterPoint(Terrain.Terrain terrain)
    {
        var bounds = Bounds;
        var x = bounds.Left + bounds.Width / 2;
        var y = bounds.Top + bounds.Height / 2;
        var z = terrain.GetGroundHeight(new Vector2(x, y));

        if (z == null)
        {
            return null;
        }

        return new Vector3(x, y, z.Value);
    }

    // Ported from ZH, based on the Ray Casting algorithm
    // https://en.wikipedia.org/wiki/Point_in_polygon#Ray_casting_algorithm
    public bool PointInTrigger(in Point3D point)
    {
        // Coarse test so we can early-out.
        if (!Bounds.Contains(point.ToPoint2D()))
        {
            return false;
        }

        var inside = false;
        for (var i = 0; i < Points.Length; i++)
        {
            var pt1 = Points[i];
            Point3D pt2;
            if (i == Points.Length - 1)
            {
                pt2 = Points[0];
            }
            else
            {
                pt2 = Points[i + 1];
            }

            if (pt1.Y == pt2.Y)
            {
                // ZH Compatibility: "ignore horizontal lines" says the original code
                // No idea why.
                continue;
            }

            if (pt1.Y < point.Y && pt2.Y < point.Y)
            {
                continue;
            }

            if (pt1.Y >= point.Y && pt2.Y >= point.Y)
            {
                continue;
            }

            if (pt1.X < point.X && pt2.X < point.X)
            {
                continue;
            }

            // Line segment crosses ray from point x->infinity
            var dy = pt2.Y - pt1.Y;
            var dx = pt2.X - pt1.X;

            var intersectionX = pt1.X + (dx * (point.Y - pt1.Y) / (float)dy);
            if (intersectionX >= point.X)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistArrayWithUInt32Length(
            Points,
            static (StatePersister persister, ref Point3D item) =>
            {
                persister.PersistPoint3DValue(ref item);
            }
        );


        // ZH Compatibility: this uses potentially outdated values for the bounds and radius
        // It is not clear if that was intentional or not.
        var topLeft = _bounds.TopLeft;
        reader.PersistPoint2D(ref topLeft);
        var bottomRight = _bounds.BottomRight;
        reader.PersistPoint2D(ref bottomRight);
        reader.PersistSingle(ref _radius);
        reader.PersistBoolean(ref _boundsNeedsUpdate);
    }
}
