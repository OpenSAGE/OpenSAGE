using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using OpenSage.FileFormats.Apt;
using OpenSage.FileFormats.Apt.Characters;
using OpenSage.FileFormats.Apt.FrameItems;
using OpenSage.Tools.AptEditor.Util;
using OpenSage.Mathematics;
using System.Text.Json;

namespace OpenSage.Tools.AptEditor
{
    internal static class SampleApt
    {
        public static async Task<AptFile> Test1Async(string path)
        {
            var getter = new StandardStreamGetter(path);
            var file = Create(getter.GetMovieName());
            await file.WriteAsync(getter);
            var file2 = AptFile.Parse(getter);
            return file2;
        }

        public static AptFile Test2(string path)
        {
            var getter = new StandardStreamGetter(path);
            var file = AptFile.Parse(getter);
            file.Write(getter);
            var file2 = AptFile.Parse(getter);
            return file2;
        }

        public static void Test3()
        {
            TreeList tl = new(Create("Gan Si Huang Xu Dong", new ColorRgbaF(0.2f, 0.4f, 0.6f, 0.8f).ToColorRgba()));
            Queue<(int, string)> q = new();
            q.Enqueue((1, ""));
            while (q.Count > 0)
            {
                var (nid, nstr) = q.Dequeue();
                Console.WriteLine(nstr + $"#{nid} {tl.GetType(nid).Info}");

                var f = tl.GetFields(nid).Info;
                Console.WriteLine(nstr + " Fields: " + f);
                var farr = JsonSerializer.Deserialize<List<string>>(f);
                foreach (var ff in farr)
                    Console.WriteLine(nstr + $"  {ff}: {tl.Get(nid, ff)}");

                var c = tl.GetChildren(nid).Info;
                Console.WriteLine(nstr + " Children: " + c);
                var carr = JsonSerializer.Deserialize<List<int>>(c);
                foreach (var cc in carr)
                    q.Enqueue((cc, nstr + "   "));
            }
        }

        public static AptFile Create(string name) { return Create(name, new ColorRgba(0, 255, 0, 255)); }
        public static AptFile Create(string name, in ColorRgba color)
        {
            const int width = 100;
            const int height = 100;
            var aptFile = AptFile.CreateEmpty(name, width, height, 33);

            var backgroundShape = CreateShape(aptFile, new[]
            {
                "c",
                $"s s:{color.R}:{color.G}:{color.B}:{color.A}",
                $"t 0:0:{width}:{height}:0:{height}",
                $"t 0:0:{width}:{height}:{width}:0"
            });

            var backgroundShape2 = CreateShape(aptFile, new[]
            {
                "c",
                $"s s:{255 - color.R}:{255 - color.G}:{255 - color.B}:{color.A}",
                $"t 0:0:{width}:{height}:0:{height}",
                $"t 0:0:{width}:{height}:{width}:0"
            });

            var white = ColorRgba.White;
            var triSize = 10;
            var triangle = CreateShape(aptFile, new[]
            {
                "c",
                $"s s:{white.R}:{white.G}:{white.B}:{white.A}",
                $"t 0:{triSize / 2}:{triSize / 2}:0:{triSize}:{triSize / 2}",
            });
            var translatedPlace = PlaceObject.Create(1, triangle);
            translatedPlace.SetTransform(
                Matrix3x2.CreateTranslation(20, 0),
                ColorRgbaF.Red.ToColorRgba()
            );
            var sprite = CreateSprite(aptFile, new[]
            {
                PlaceObject.Create(0, triangle),
                translatedPlace
            });

            var rotatedSprite = PlaceObject.Create(1, sprite);
            rotatedSprite.SetTransform(
                Matrix3x2.CreateRotation(MathF.PI / 2) *
                    Matrix3x2.CreateTranslation(20, 20),
                ColorRgbaF.White.ToColorRgba()
            );

            var example1 = CreateSprite(aptFile, new[]
            {
                PlaceObject.Create(0, backgroundShape),
                rotatedSprite
            });
            var example2 = CreateSprite(aptFile, new[]
            {
                PlaceObject.Create(0, backgroundShape2),
                rotatedSprite
            });
            var frame = Frame.Create(new List<FrameItem>
            {
                PlaceObject.Create(0, example1),
                PlaceObject.Create(1, example2),
            });
            aptFile.Movie.Frames.Add(frame);

            return aptFile;
        }

        public static int CreateSprite(AptFile apt, IEnumerable<FrameItem> items)
        {
            var characters = apt.Movie.Characters;
            var spriteIndex = characters.Count;
            var sprite = Sprite.Create(apt);
            characters.Add(sprite);
            sprite.Frames[0].FrameItems.AddRange(items);
            return spriteIndex;
        }

        public static int CreateShape(AptFile apt, string[] geometryCommands)
        {
            using var reader = new StringReader(string.Join('\n', geometryCommands));
            var geometry = Geometry.Parse(apt, reader);
            var characters = apt.Movie.Characters;
            var shapeIndex = (uint) characters.Count;
            apt.GeometryMap.Add(shapeIndex, geometry);
            var shape = Shape.Create(apt, shapeIndex);
            characters.Add(shape);
            return (int) shapeIndex;
        }
    }
}
