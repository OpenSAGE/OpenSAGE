using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using OpenSage.Data.Sav;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Mathematics;
using Xunit;

namespace OpenSage.Tests.Data.Sav
{
    public class SaveFileTests : IClassFixture<GameFixture>
    {
        private static readonly string RootFolder = Path.Combine(Environment.CurrentDirectory, "Data", "Sav", "Assets");

        private readonly GameFixture _fixture;

        public SaveFileTests(GameFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory(Skip = "Doesn't work yet")]
        //[Theory]
        [MemberData(nameof(GetSaveFiles))]
        public void CanLoadSaveFiles(string relativePath)
        {
            var fullPath = Path.Combine(RootFolder, relativePath);

            using var stream = File.OpenRead(fullPath);

            var sageGame = Enum.Parse<SageGame>(relativePath.Substring(0, relativePath.IndexOf(Path.DirectorySeparatorChar)));

            var game = _fixture.GetGame(sageGame);

            SaveFile.LoadFromStream(stream, game);

            // Now try writing .sav file out using validating writer,
            // to make sure we write the same thing as we read.
            stream.Position = 0;
            using var comparisonReader = new StateReader(stream, game);
            using var validatingWriter = new ValidatingStateWriter(comparisonReader);
            SaveFile.Persist(validatingWriter);

            game.EndGame();
        }

        public static IEnumerable<object[]> GetSaveFiles()
        {
            foreach (var file in Directory.GetFiles(RootFolder, "*.sav", SearchOption.AllDirectories))
            {
                var relativePath = file.Substring(RootFolder.Length + 1);
                yield return new object[] { relativePath };
            }
        }

        private sealed class ValidatingStateWriter : StatePersister
        {
            private readonly StateReader _comparisonReader;

            public ValidatingStateWriter(StateReader comparisonReader)
                : base(comparisonReader.Game, StatePersistMode.Write)
            {
                _comparisonReader = comparisonReader;
            }

            public override uint BeginSegment(string segmentName)
            {
                return _comparisonReader.BeginSegment(segmentName);
            }

            public override void EndSegment()
            {
                _comparisonReader.EndSegment();
            }

            public override void PersistAsciiString(ref string value)
            {
                string comparisonValue = default;
                _comparisonReader.PersistAsciiString(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistBitArray<TEnum>(ref BitArray<TEnum> value)
            {
                BitArray<TEnum> comparisonValue = new();
                _comparisonReader.PersistBitArray(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistBoolean(ref bool value)
            {
                bool comparisonValue = default;
                _comparisonReader.PersistBoolean(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistByte(ref byte value)
            {
                byte comparisonValue = default;
                _comparisonReader.PersistByte(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistColorRgba(ref ColorRgba value)
            {
                ColorRgba comparisonValue = default;
                _comparisonReader.PersistColorRgba(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistColorRgbaInt(ref ColorRgba value)
            {
                ColorRgba comparisonValue = default;
                _comparisonReader.PersistColorRgbaInt(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistDateTime(ref DateTime value)
            {
                DateTime comparisonValue = default;
                _comparisonReader.PersistDateTime(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistEnum<TEnum>(ref TEnum value)
            {
                TEnum comparisonValue = default;
                _comparisonReader.PersistEnum(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistEnumByte<TEnum>(ref TEnum value)
            {
                TEnum comparisonValue = default;
                _comparisonReader.PersistEnum(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistEnumByteFlags<TEnum>(ref TEnum value)
            {
                TEnum comparisonValue = default;
                _comparisonReader.PersistEnum(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistEnumFlags<TEnum>(ref TEnum value)
            {
                TEnum comparisonValue = default;
                _comparisonReader.PersistEnum(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistInt16(ref short value)
            {
                short comparisonValue = default;
                _comparisonReader.PersistInt16(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistInt32(ref int value)
            {
                int comparisonValue = default;
                _comparisonReader.PersistInt32(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistList<T>(List<T> value, PersistListItemCallback<T> callback)
            {
                throw new NotImplementedException();
            }

            public override void PersistMatrix4x3(ref Matrix4x3 value, bool readVersion = true)
            {
                Matrix4x3 comparisonValue = default;
                _comparisonReader.PersistMatrix4x3(ref comparisonValue, readVersion);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistPoint2D(ref Point2D value)
            {
                Point2D comparisonValue = default;
                _comparisonReader.PersistPoint2D(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistPoint3D(ref Point3D value)
            {
                Point3D comparisonValue = default;
                _comparisonReader.PersistPoint3D(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistRandomAlphaKeyframe(ref RandomAlphaKeyframe value)
            {
                RandomAlphaKeyframe comparisonValue = default;
                _comparisonReader.PersistRandomAlphaKeyframe(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistRandomVariable(ref RandomVariable value)
            {
                RandomVariable comparisonValue = default;
                _comparisonReader.PersistRandomVariable(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistRgbColorKeyframe(ref RgbColorKeyframe value)
            {
                RgbColorKeyframe comparisonValue = default;
                _comparisonReader.PersistRgbColorKeyframe(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistSingle(ref float value)
            {
                float comparisonValue = default;
                _comparisonReader.PersistSingle(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistUInt16(ref ushort value)
            {
                ushort comparisonValue = default;
                _comparisonReader.PersistUInt16(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistUInt32(ref uint value)
            {
                uint comparisonValue = default;
                _comparisonReader.PersistUInt32(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistUnicodeString(ref string value)
            {
                string comparisonValue = default;
                _comparisonReader.PersistUnicodeString(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistVector3(ref Vector3 value)
            {
                Vector3 comparisonValue = default;
                _comparisonReader.PersistVector3(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override byte PersistVersion(byte maximumVersion)
            {
                return _comparisonReader.PersistVersion(maximumVersion);
            }

            public override void ReadBytesIntoStream(Stream destination, int numBytes)
            {
                throw new NotSupportedException();
            }

            public override void SkipUnknownBytes(int numBytes)
            {
                _comparisonReader.SkipUnknownBytes(numBytes);
            }

            private static void CheckEquality<T>(T value, T comparisonValue)
            {
                if (!value.Equals(comparisonValue))
                {
                    throw new InvalidStateException($"Actual value '{value}' does not match comparison value '{comparisonValue}'");
                }
            }
        }
    }
}
