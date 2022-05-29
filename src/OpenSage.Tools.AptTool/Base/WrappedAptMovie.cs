using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenSage.Tools.AptTool.Base
{
    public class AptMovieConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(WrappedAptMovie).IsAssignableFrom(objectType);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    [JsonObject()]
    [JsonConverter(typeof(AptMovieConverter))]
    public record WrappedAptMovie
    {
        [JsonProperty("metadata")]
        public AptMovieMetadata Metadata { get; private set; }

        [JsonProperty("characters")]
        public List<WrappedCharacterBase> Characters { get; private set; }


    }
}
