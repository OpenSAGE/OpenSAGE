#nullable enable

using System;
using System.IO;
using OpenSage.Data.Sav;
using OpenSage.Logic.Object;
using Xunit;

namespace OpenSage.Tests.Data.Sav
{
    public class SaveFileTests : IClassFixture<GameFixture>
    {
        // swap to the null skip message in order to run locally
        private const string Skip = "game files required";
        // private const string? Skip = null;

        private static readonly string RootFolder = Path.Combine(Environment.CurrentDirectory, "Data", "Sav", "Assets");

        private readonly GameFixture _fixture;

        public SaveFileTests(GameFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory(Skip = Skip)]
        [MemberData(nameof(GeneralsSaveFiles))]
        public void CanLoadGeneralsSaveFiles(string fullPath)
        {
            LoadSaveFile(fullPath, SageGame.CncGenerals);
        }

        [Theory(Skip = Skip)]
        [MemberData(nameof(ZeroHourSaveFiles))]
        public void CanLoadZeroHourSaveFiles(string fullPath)
        {
            LoadSaveFile(fullPath, SageGame.CncGeneralsZeroHour);
        }

        private void LoadSaveFile(string fullPath, SageGame sageGame)
        {
            using var stream = File.OpenRead(fullPath);

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

        public static TheoryData<string> GeneralsSaveFiles() => GetSaveFiles(SageGame.CncGenerals);
        public static TheoryData<string> ZeroHourSaveFiles() => GetSaveFiles(SageGame.CncGeneralsZeroHour);

        private static TheoryData<string> GetSaveFiles(SageGame sageGame)
        {
            var data = new TheoryData<string>();
            data.AddRange(Directory.GetFiles(Path.Combine(RootFolder, sageGame.ToString()), "*.sav", SearchOption.AllDirectories));
            return data;
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

            public override void PersistAsciiStringValue(ref string value)
            {
                string? comparisonValue = default;
                _comparisonReader.PersistAsciiStringValue(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistBooleanValue(ref bool value)
            {
                bool comparisonValue = default;
                _comparisonReader.PersistBooleanValue(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistByteValue(ref byte value)
            {
                byte comparisonValue = default;
                _comparisonReader.PersistByteValue(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistEnumValue<TEnum>(ref TEnum value)
            {
                TEnum comparisonValue = default;
                _comparisonReader.PersistEnumValue(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistEnumByteValue<TEnum>(ref TEnum value)
            {
                TEnum comparisonValue = default;
                _comparisonReader.PersistEnumValue(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistEnumByteFlagsValue<TEnum>(ref TEnum value)
            {
                TEnum comparisonValue = default;
                _comparisonReader.PersistEnumValue(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistEnumFlagsValue<TEnum>(ref TEnum value)
            {
                TEnum comparisonValue = default;
                _comparisonReader.PersistEnumValue(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistInt16Value(ref short value)
            {
                short comparisonValue = default;
                _comparisonReader.PersistInt16Value(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistInt32Value(ref int value)
            {
                int comparisonValue = default;
                _comparisonReader.PersistInt32Value(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistSingleValue(ref float value)
            {
                float comparisonValue = default;
                _comparisonReader.PersistSingleValue(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistUInt16Value(ref ushort value)
            {
                ushort comparisonValue = default;
                _comparisonReader.PersistUInt16Value(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistUInt32Value(ref uint value)
            {
                uint comparisonValue = default;
                _comparisonReader.PersistUInt32Value(ref comparisonValue);

                CheckEquality(value, comparisonValue);
            }

            public override void PersistUnicodeStringValue(ref string value)
            {
                string? comparisonValue = default;
                _comparisonReader.PersistUnicodeStringValue(ref comparisonValue);

                CheckEquality(value, comparisonValue);
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

            public override void PersistUpdateFrameValue(ref UpdateFrame value)
            {
                PersistUInt32Value(ref value.RawValue);
            }

            private static void CheckEquality<T>(T? value, T? comparisonValue)
            {
                if (Equals(value, comparisonValue))
                {
                    throw new InvalidStateException($"Actual value '{value}' does not match comparison value '{comparisonValue}'");
                }
            }

            public override void SkipUnknownBytes(int numBytes)
            {
                _comparisonReader.SkipUnknownBytes(numBytes);
            }
        }
    }
}
