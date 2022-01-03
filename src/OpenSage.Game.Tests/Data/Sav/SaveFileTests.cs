using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OpenSage.Data.Sav;
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

            //// Now try writing .sav file out using validating writer,
            //// to make sure we write the same thing as we read.
            //stream.Position = 0;
            //using var comparisonReader = new StateReader(stream, game);
            //using var validatingWriter = new ValidatingStateWriter(comparisonReader);
            //SaveFile.Persist(validatingWriter);

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

            public override void PersistBoolean(string name, ref bool value)
            {
                bool comparisonValue = default;
                _comparisonReader.PersistBoolean(name, ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistByte(string name, ref byte value)
            {
                byte comparisonValue = default;
                _comparisonReader.PersistByte(name, ref comparisonValue);

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

            public override void PersistUnicodeString(string name, ref string value)
            {
                string comparisonValue = default;
                _comparisonReader.PersistUnicodeString(name, ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override byte PersistVersion(byte maximumVersion)
            {
                return _comparisonReader.PersistVersion(maximumVersion);
            }

            public override void PersistSpan(Span<byte> span)
            {
                Span<byte> comparisonValue = new byte[span.Length];
                _comparisonReader.PersistSpan(comparisonValue);

                for (var i = 0; i < span.Length; i++)
                {
                    CheckEquality(span[i], comparisonValue[i]);
                }
            }

            private static void CheckEquality<T>(T value, T comparisonValue)
            {
                if (!value.Equals(comparisonValue))
                {
                    throw new InvalidStateException($"Actual value '{value}' does not match comparison value '{comparisonValue}'");
                }
            }

            public override void SkipUnknownBytes(int numBytes)
            {
                _comparisonReader.SkipUnknownBytes(numBytes);
            }
        }

        //private sealed class JsonSaveWriter : StatePersister
        //{
        //    private readonly Utf8JsonWriter _writer;

        //    public JsonSaveWriter(Game game, string filePath)
        //        : base(game, StatePersistMode.Write)
        //    {
        //        var stream = AddDisposable(File.OpenRead(filePath);

        //        _writer = AddDisposable(new Utf8JsonWriter(stream, new JsonWriterOptions
        //        {
        //            Indented = true,
        //            Validating = true,
        //        }));
        //    }

        //    public override uint BeginSegment(string segmentName)
        //    {
        //        return 0;
        //    }

        //    public override void EndSegment()
        //    {
                
        //    }

        //    public override void PersistAsciiString(ref string value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override void PersistBoolean(ref bool value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override void PersistByte(ref byte value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override void PersistEnum<TEnum>(ref TEnum value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override void PersistEnumByte<TEnum>(ref TEnum value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override void PersistEnumByteFlags<TEnum>(ref TEnum value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override void PersistEnumFlags<TEnum>(ref TEnum value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override void PersistInt16(ref short value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override void PersistInt32(ref int value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override void PersistSingle(ref float value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override void PersistSpan(Span<byte> span)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override void PersistUInt16(ref ushort value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override void PersistUInt32(ref uint value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override void PersistUnicodeString(ref string value)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override byte PersistVersion(byte maximumVersion)
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
    }
}
