﻿using System;
using System.IO;
using System.Numerics;

namespace OpenSage.Data.Map;

public sealed class HeightMapData : Asset
{
    public const string AssetName = "HeightMapData";

    public bool ElevationsAre16Bit => Version >= 5;

    // Gathered from heights in World Builder's status bar compared to heightmap data.
    // In Generals, heights are stored as uint8 (max 255), and scaled by 0.625 (max 159.375)
    // In BFME, BFME2, C&C3, heights are stored in uint16 (max 65536), and scaled by 0.0390625 (max 2560)
    // Corresponds to MAP_HEIGHT_SCALE in ZH
    public float VerticalScale => ElevationsAre16Bit ? 0.0390625f : 0.625f;

    public uint Width { get; private set; }
    public uint Height { get; private set; }

    public uint BorderWidth { get; private set; }

    /// <summary>
    /// Relative to BorderWidth.
    /// </summary>
    public HeightMapBorder[] Borders { get; private set; }

    public uint Area { get; private set; }
    public ushort[,] Elevations { get; private set; }

    public float MinZ { get; private set; }
    public float MaxZ { get; private set; }

    public static HeightMapData Create(uint borderWidth, ushort[,] elevations)
    {
        var width = (uint)elevations.GetLength(0);
        var height = (uint)elevations.GetLength(1);

        return new HeightMapData
        {
            Width = width,
            Height = height,
            BorderWidth = borderWidth,
            Area = width * height,
            Elevations = elevations,
        };
    }

    internal static HeightMapData Parse(BinaryReader reader, MapParseContext context)
    {
        return ParseAsset(reader, context, version =>
        {
            var result = new HeightMapData
            {
                Width = reader.ReadUInt32(),
                Height = reader.ReadUInt32(),

                BorderWidth = reader.ReadUInt32()
            };

            var borderCount = reader.ReadUInt32();
            result.Borders = new HeightMapBorder[borderCount];

            for (var i = 0; i < borderCount; i++)
            {
                result.Borders[i] = HeightMapBorder.Parse(reader, version);
            }

            result.Area = reader.ReadUInt32();
            if (result.Width * result.Height != result.Area)
            {
                throw new InvalidDataException();
            }

            var minZ = ushort.MaxValue;
            var maxZ = ushort.MinValue;

            result.Elevations = new ushort[result.Width, result.Height];
            for (var y = 0; y < result.Height; y++)
            {
                for (var x = 0; x < result.Width; x++)
                {
                    var elevation = version >= 5
                        ? reader.ReadUInt16()
                        : reader.ReadByte();
                    minZ = Math.Min(minZ, elevation);
                    maxZ = Math.Max(maxZ, elevation);
                    result.Elevations[x, y] = elevation;
                }
            }

            result.MinZ = minZ * result.VerticalScale;
            result.MaxZ = maxZ * result.VerticalScale;

            return result;
        });
    }

    internal void WriteTo(BinaryWriter writer)
    {
        WriteAssetTo(writer, () =>
        {
            writer.Write(Width);
            writer.Write(Height);

            writer.Write(BorderWidth);

            writer.Write((uint)Borders.Length);

            foreach (var border in Borders)
            {
                border.WriteTo(writer, Version);
            }

            writer.Write(Area);

            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    if (Version >= 5)
                    {
                        writer.Write(Elevations[x, y]);
                    }
                    else
                    {
                        writer.Write((byte)Elevations[x, y]);
                    }
                }
            }
        });
    }
}

public struct HeightMapBorder
{
    [AddedIn(SageGame.Cnc3)]
    public uint Corner1X;

    [AddedIn(SageGame.Cnc3)]
    public uint Corner1Y;

    // Previous name Corner2X
    public uint X;
    // Previous name Corner2Y
    public uint Y;

    public Vector2 ToVector2()
    {
        return new Vector2(X, Y);
    }

    internal static HeightMapBorder Parse(BinaryReader reader, ushort version)
    {
        if (version >= 6)
        {
            return new HeightMapBorder
            {
                Corner1X = reader.ReadUInt32(),
                Corner1Y = reader.ReadUInt32(),
                X = reader.ReadUInt32(),
                Y = reader.ReadUInt32()
            };
        }
        else
        {
            return new HeightMapBorder
            {
                Corner1X = 0,
                Corner1Y = 0,
                X = reader.ReadUInt32(),
                Y = reader.ReadUInt32()
            };
        }
    }

    internal void WriteTo(BinaryWriter writer, ushort version)
    {
        if (version >= 6)
        {
            writer.Write(Corner1X);
            writer.Write(Corner1Y);
            writer.Write(X);
            writer.Write(Y);
        }
        else
        {
            writer.Write(X);
            writer.Write(Y);
        }
    }
}
