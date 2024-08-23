﻿using System;
using System.IO;
using System.Text.Json;
using OpenSage.Logic.Object;

namespace OpenSage.Tools.Sav2Json;

internal sealed class JsonSaveWriter : StatePersister
{
    private readonly Utf8JsonWriter _writer;

    public JsonSaveWriter(IGame game, string filePath)
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

    protected override void OnPersistObjectValue<T>(T value)
    {
        _writer.WriteString("_Type", value.GetType().Name);
    }

    protected override void OnPersistObjectValue<T>(ref T value)
    {
        _writer.WriteString("_Type", value.GetType().Name);
    }

    public override void PersistFieldName(string name)
    {
        var normalized = name.TrimStart('_');
        if (normalized.Length > 0 && char.IsLower(normalized[0]))
        {
            var array = normalized.ToCharArray();
            array[0] = char.ToUpper(array[0]);
            normalized = new string(array);
        }

        _writer.WritePropertyName(normalized);

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

    public override void PersistUpdateFrameValue(ref UpdateFrame value)
    {
        var frame = value.Frame;
        PersistUInt32(ref frame);

        var something = value.Something;
        PersistByte(ref something);
    }

    public override void SkipUnknownBytes(int numBytes)
    {

    }
}
