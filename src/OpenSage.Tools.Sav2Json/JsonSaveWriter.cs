using System;
using System.IO;
using System.Text.Json;

namespace OpenSage.Tools.Sav2Json;

internal sealed class JsonSaveWriter : StatePersister
{
    private readonly Utf8JsonWriter _writer;

    public JsonSaveWriter(Game game, string filePath)
        : base(game, StatePersistMode.Write)
    {
        var stream = AddDisposable(File.OpenWrite(filePath));

        _writer = AddDisposable(new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Indented = true,
            SkipValidation = false,
        }));
    }

    public override uint BeginSegment(string segmentName)
    {
        return 0;
    }

    public override void EndSegment()
    {
        
    }

    public override void BeginArray()
    {
        _writer.WriteStartArray();
    }

    public override void EndArray()
    {
        _writer.WriteEndArray();
    }

    public override void BeginObject()
    {
        _writer.WriteStartObject();
    }

    public override void EndObject()
    {
        _writer.WriteEndObject();
    }

    public override void PersistFieldName(string name)
    {
        _writer.WritePropertyName(name);
    }

    public override void PersistAsciiStringValue(ref string value)
    {
        _writer.WriteStringValue(value);
    }

    public override void PersistBooleanValue(ref bool value)
    {
        _writer.WriteBooleanValue(value);
    }

    public override void PersistByteValue(ref byte value)
    {
        _writer.WriteNumberValue(value);
    }

    public override void PersistEnumByteFlagsValue<TEnum>(ref TEnum value)
    {
        _writer.WriteStringValue(value.ToString());
    }

    public override void PersistEnumByteValue<TEnum>(ref TEnum value)
    {
        _writer.WriteStringValue(value.ToString());
    }

    public override void PersistEnumFlagsValue<TEnum>(ref TEnum value)
    {
        _writer.WriteStringValue(value.ToString());
    }

    public override void PersistEnumValue<TEnum>(ref TEnum value)
    {
        _writer.WriteStringValue(value.ToString());
    }

    public override void PersistInt16Value(ref short value)
    {
        _writer.WriteNumberValue(value);
    }

    public override void PersistInt32Value(ref int value)
    {
        _writer.WriteNumberValue(value);
    }

    public override void PersistSingleValue(ref float value)
    {
        _writer.WriteNumberValue(value);
    }

    public override void PersistSpan(Span<byte> span)
    {
        
    }

    public override void PersistUInt16Value(ref ushort value)
    {
        _writer.WriteNumberValue(value);
    }

    public override void PersistUInt32Value(ref uint value)
    {
        _writer.WriteNumberValue(value);
    }

    public override void PersistUnicodeStringValue(ref string value)
    {
        _writer.WriteStringValue(value);
    }

    public override void SkipUnknownBytes(int numBytes)
    {
        
    }
}
