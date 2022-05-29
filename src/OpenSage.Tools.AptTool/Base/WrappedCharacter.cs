using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenSage.FileFormats.Apt.Characters;

namespace OpenSage.Tools.AptTool.Base
{
    public class WrappedCharacterConverter : JsonConverter
    {
        private static readonly Dictionary<Type, string> CharacterTypes;
        private static readonly Dictionary<string, Type> CharacterTypesInverse;
        private static readonly string TypeStr = "$TYPE";

        static WrappedCharacterConverter() {
            CharacterTypes = new()
            {
                [typeof(WrappedSprite)] = "Sprite",
                [typeof(WrappedButton)] = "Button", 
                [typeof(WrappedCharacter<Text>)] = "Text",
                [typeof(WrappedCharacter<Shape>)] = "Shape",
                [typeof(WrappedCharacter<Image>)] = "Image",
            };
            CharacterTypesInverse = new(CharacterTypes.Select(x => new KeyValuePair<string, Type>(x.Value, x.Key)));
        }

        public override bool CanConvert(Type objectType)
        {
            return CharacterTypes.ContainsKey(objectType) || typeof(WrappedCharacterBase).IsAssignableFrom(objectType);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var obj = JToken.ReadFrom(reader) as JObject;
            if (obj == null)
                throw new InvalidDataException();
            var t = (string?)obj.Property(TypeStr);
            if (t == null)
                throw new InvalidDataException();

            var type = CharacterTypesInverse[t];
            obj.Remove(TypeStr);
            return type.GetMethod("FromJsonObject")!.Invoke(null, new object[] { reader, obj, serializer });
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            var t = value.GetType();

            // judge if type is valid
            if (CharacterTypes.ContainsKey(t))
            {
                var tg = t.GetGenericArguments()[0];
                typeof(WrappedCharacterConverter).GetMethod("WriteJson2")!.MakeGenericMethod(new[] { tg }).Invoke(this, new[] { writer, value, serializer });
                return;
            }
            else
            {
                throw new InvalidDataException();
            }
        }

        public void WriteJson2<T>(JsonWriter writer, WrappedCharacter<T> value, JsonSerializer serializer) where T: Character
        {
            var pairs = value.ToJsonObject();
            if (pairs == null) // TODO Is it necessary or correct?
            {
                writer.WriteNull();
                return;
            }
            pairs.Add(TypeStr, CharacterTypes[value.GetType()]);
            serializer.Serialize(writer, pairs);
        }
    }

    [JsonConverter(typeof(WrappedCharacterConverter))]
    public abstract record WrappedCharacterBase
    {
        // just for convenience when creating an array etc.
        public abstract Character? getItem();
    }

    [JsonConverter(typeof(WrappedCharacterConverter))]
    public record WrappedCharacter<T> : WrappedCharacterBase where T: Character
    {
        public T? Item { get; set; }
        public override Character? getItem()
        {
            return Item;
        }

        public virtual JObject? ToJsonObject()
        {
            var ret = Item != null ? JObject.FromObject(Item) : null;
            return ret;
        }

        public static WrappedCharacter<T> FromJsonObject(JsonReader reader, JObject existingValue, JsonSerializer serializer)
        {
            var target = Activator.CreateInstance(typeof(T));
            serializer.Populate(existingValue.CreateReader(), target!);
            return new() { Item = (T)target! };
        }
    }

}
