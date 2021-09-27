using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using OpenSage.FileFormats;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.Apt
{
    public enum GeometryStyle
    {
        Undefined,
        TexturedTri,
        SolidTri,
        Line
    }

    public interface IGeometryEntry
    {
        GeometryStyle Type { get; }
    }

    public sealed class Geometry : IDataStorage
    {
        public string RawText { get; private set; }
        public List<IGeometryEntry> Entries { get; private set; }
        public RectangleF BoundingBox { get; private set; }
        public AptFile Container { get; private set; }

        private Geometry(AptFile container)
        {
            Entries = new List<IGeometryEntry>();
            Container = container;
        }

        public static Geometry Parse(AptFile container, TextReader reader)
        {
            var geometry = new Geometry(container);

            string line;
            int image = 0;
            float thickness = 0;
            Matrix2x2 rotMat = new Matrix2x2();
            Vector2 translation = new Vector2();
            ColorRgba color = new ColorRgba(0, 0, 0, 0);
            GeometryStyle style = GeometryStyle.Undefined;
            var tris = new List<Triangle2D>();
            var lines = new List<Line2D>();

            Action ApplyStyle = () =>
            {
                switch (style)
                {
                    //no data parsed yet
                    case GeometryStyle.Undefined:
                        break;
                    case GeometryStyle.TexturedTri:
                        geometry.Entries.Add(new GeometryTexturedTriangles(new List<Triangle2D>(tris), color, image, rotMat, translation));
                        tris.Clear();
                        break;
                    case GeometryStyle.SolidTri:
                        geometry.Entries.Add(new GeometrySolidTriangles(new List<Triangle2D>(tris), color));
                        tris.Clear();
                        break;
                    case GeometryStyle.Line:
                        geometry.Entries.Add(new GeometryLines(new List<Line2D>(lines), color, thickness));
                        lines.Clear();
                        break;
                }

                style = GeometryStyle.Undefined;
            };

            var rawText = new StringBuilder();

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                rawText.AppendLine(line);

                var lineMode = line.First();
                line = line.TrimStart('c', 's', 'l', 't');
                var paramList = line.Split(':');

                //trim each entry of the param list
                for (var i = 0; i < paramList.Length; ++i)
                {
                    paramList[i] = paramList[i].Trim();
                }

                switch (lineMode)
                {
                    //Clear - Finish the last geometry
                    case 'c':
                        ApplyStyle();
                        break;
                    //Style - Header for the following data
                    case 's':
                        //Check that we have atleast 1 param
                        if (paramList.Length < 1)
                            throw new InvalidDataException();

                        //Check which style we are using
                        switch (paramList.First())
                        {
                            //this is the solid triangle style
                            case "s":
                                if (paramList.Length != 5)
                                    throw new InvalidDataException();

                                style = GeometryStyle.SolidTri;
                                color = new ColorRgba(
                                    Convert.ToByte(paramList[1]),
                                    Convert.ToByte(paramList[2]),
                                    Convert.ToByte(paramList[3]),
                                    Convert.ToByte(paramList[4]));
                                break;
                            //this is the line style
                            case "l":
                                if (paramList.Length != 6)
                                    throw new InvalidDataException();

                                style = GeometryStyle.Line;
                                thickness = ParseUtility.ParseFloat(paramList[1]);
                                color = new ColorRgba(
                                    Convert.ToByte(paramList[2]),
                                    Convert.ToByte(paramList[3]),
                                    Convert.ToByte(paramList[4]),
                                    Convert.ToByte(paramList[5]));
                                break;
                            //this is the textured triangle style
                            case "tc":
                                if (paramList.Length != 12)
                                    throw new InvalidDataException();

                                style = GeometryStyle.TexturedTri;
                                color = new ColorRgba(
                                    Convert.ToByte(paramList[1]),
                                    Convert.ToByte(paramList[2]),
                                    Convert.ToByte(paramList[3]),
                                    Convert.ToByte(paramList[4]));
                                //image id used
                                image = Convert.ToInt32(paramList[5]);
                                //transformation parameters, to map the geometry above the texture
                                rotMat = new Matrix2x2(
                                    ParseUtility.ParseFloat(paramList[6]),
                                    ParseUtility.ParseFloat(paramList[7]),
                                    ParseUtility.ParseFloat(paramList[8]),
                                    ParseUtility.ParseFloat(paramList[9]));
                                translation.X = ParseUtility.ParseFloat(paramList[10]);
                                translation.Y = ParseUtility.ParseFloat(paramList[11]);
                                break;
                        }
                        break;
                    //A line
                    case 'l':
                        lines.Add(new Line2D(
                            new Vector2(ParseUtility.ParseFloat(paramList[0]), ParseUtility.ParseFloat(paramList[1])),
                            new Vector2(ParseUtility.ParseFloat(paramList[2]), ParseUtility.ParseFloat(paramList[3]))));
                        break;
                    //A triangle
                    case 't':
                        tris.Add(new Triangle2D(
                            new Vector2(ParseUtility.ParseFloat(paramList[0]), ParseUtility.ParseFloat(paramList[1])),
                            new Vector2(ParseUtility.ParseFloat(paramList[2]), ParseUtility.ParseFloat(paramList[3])),
                            new Vector2(ParseUtility.ParseFloat(paramList[4]), ParseUtility.ParseFloat(paramList[5]))));
                        break;
                }

            }

            ApplyStyle();
            geometry.RawText = rawText.ToString();
            geometry.CalculateBoundings();

            return geometry;
        }

        private void CalculateBoundings()
        {
            //find topleft and botright point
            var topLeft = new Vector2 { X = float.MaxValue, Y = float.MaxValue };
            var botRight = new Vector2 { X = float.MinValue, Y = float.MinValue };

            foreach (var entry in Entries)
            {
                switch (entry)
                {
                    case GeometryLines gl:
                        foreach (var line in gl.Lines)
                        {
                            if (line.V0.X < topLeft.X) topLeft.X = line.V0.X;
                            if (line.V1.X < topLeft.X) topLeft.X = line.V1.X;
                            if (line.V0.Y < topLeft.Y) topLeft.Y = line.V0.Y;
                            if (line.V1.Y < topLeft.Y) topLeft.Y = line.V1.Y;

                            if (line.V0.X > botRight.X) botRight.X = line.V0.X;
                            if (line.V1.X > botRight.X) botRight.X = line.V1.X;
                            if (line.V0.Y > botRight.Y) botRight.Y = line.V0.Y;
                            if (line.V1.Y > botRight.Y) botRight.Y = line.V1.Y;
                        }
                        break;
                    case GeometrySolidTriangles gst:
                        foreach (var tri in gst.Triangles)
                        {
                            if (tri.V0.X < topLeft.X) topLeft.X = tri.V0.X;
                            if (tri.V1.X < topLeft.X) topLeft.X = tri.V1.X;
                            if (tri.V2.X < topLeft.X) topLeft.X = tri.V2.X;
                            if (tri.V0.Y < topLeft.Y) topLeft.Y = tri.V0.Y;
                            if (tri.V1.Y < topLeft.Y) topLeft.Y = tri.V1.Y;
                            if (tri.V2.Y < topLeft.Y) topLeft.Y = tri.V2.Y;

                            if (tri.V0.X > botRight.X) botRight.X = tri.V0.X;
                            if (tri.V1.X > botRight.X) botRight.X = tri.V1.X;
                            if (tri.V2.X > botRight.X) botRight.X = tri.V2.X;
                            if (tri.V0.Y > botRight.Y) botRight.Y = tri.V0.Y;
                            if (tri.V1.Y > botRight.Y) botRight.Y = tri.V1.Y;
                            if (tri.V2.Y > botRight.Y) botRight.Y = tri.V2.Y;
                        }
                        break;
                    case GeometryTexturedTriangles gtt:
                        foreach (var tri in gtt.Triangles)
                        {
                            if (tri.V0.X < topLeft.X) topLeft.X = tri.V0.X;
                            if (tri.V1.X < topLeft.X) topLeft.X = tri.V1.X;
                            if (tri.V2.X < topLeft.X) topLeft.X = tri.V2.X;
                            if (tri.V0.Y < topLeft.Y) topLeft.Y = tri.V0.Y;
                            if (tri.V1.Y < topLeft.Y) topLeft.Y = tri.V1.Y;
                            if (tri.V2.Y < topLeft.Y) topLeft.Y = tri.V2.Y;

                            if (tri.V0.X > botRight.X) botRight.X = tri.V0.X;
                            if (tri.V1.X > botRight.X) botRight.X = tri.V1.X;
                            if (tri.V2.X > botRight.X) botRight.X = tri.V2.X;
                            if (tri.V0.Y > botRight.Y) botRight.Y = tri.V0.Y;
                            if (tri.V1.Y > botRight.Y) botRight.Y = tri.V1.Y;
                            if (tri.V2.Y > botRight.Y) botRight.Y = tri.V2.Y;
                        }
                        break;
                }
            }

            var size = botRight - topLeft;
            BoundingBox = new RectangleF(topLeft.X, topLeft.Y, size.X, size.Y);
        }

        public bool Contains(Vector2 point)
        {
            foreach (var entry in Entries)
            {
                switch (entry)
                {
                    case GeometryLines gl:
                        foreach (var line in gl.Lines)
                            if (line.Contains(point))
                                return true;
                        break;
                    case GeometrySolidTriangles gst:
                        foreach (var tri in gst.Triangles)
                            if (TriangleUtility.IsPointInside(tri.V0, tri.V1, tri.V2, point))
                                return true;
                        break;
                    case GeometryTexturedTriangles gtt:
                        foreach (var tri in gtt.Triangles)
                            if (TriangleUtility.IsPointInside(tri.V0, tri.V1, tri.V2, point))
                                return true;
                        break;
                }
            }
            return false;
        }

        public void Write(BinaryWriter writer, MemoryPool pool)
        {
            throw new InvalidOperationException();
        }
    }

    public class GeometrySolidTriangles : IGeometryEntry
    {
        public ColorRgba Color { get; }
        public List<Triangle2D> Triangles { get; }
        public GeometryStyle Type => GeometryStyle.SolidTri;

        public GeometrySolidTriangles(List<Triangle2D> triangles, in ColorRgba color)
        {
            Color = color;
            Triangles = triangles;
        }
    }

    public class GeometryLines : IGeometryEntry
    {
        public ColorRgba Color { get; }
        public float Thickness { get; }
        public List<Line2D> Lines { get; }
        public GeometryStyle Type => GeometryStyle.Line;

        public GeometryLines(List<Line2D> lines, in ColorRgba color, float thickness)
        {
            Color = color;
            Lines = lines;
            Thickness = thickness;
        }
    }

    public class GeometryTexturedTriangles : IGeometryEntry
    {
        public ColorRgba Color { get; }
        public List<Triangle2D> Triangles { get; }
        public int Image { get; }
        public Matrix2x2 RotScale { get; }
        public Vector2 Translation { get; }
        public GeometryStyle Type => GeometryStyle.TexturedTri;

        public GeometryTexturedTriangles(List<Triangle2D> triangles, in ColorRgba color, int image, Matrix2x2 rot, Vector2 translation)
        {
            Color = color;
            Triangles = triangles;
            Image = image;
            RotScale = rot;
            Translation = translation;
        }
    }
}
