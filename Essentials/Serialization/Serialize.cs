using n_ate.Essentials;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace n_ate.Essentials.Serialization
{
    public static class Serialize
    {
        /// <summary>
        /// Outputs Json without $ notation and with friendly spacing
        /// </summary>
        public static readonly JsonSerializerOptions FormattedOptions = new JsonSerializerOptions(Internal.BaseOptions)
        {
            WriteIndented = true //formatted
        };

        /// <summary>
        /// Outputs Json without $ notation and without friendly spacing
        /// </summary>
        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions(Internal.BaseOptions)
        {
            WriteIndented = false //formatted
        };

        /// <summary>
        /// Outputs Json with $ notation and with friendly spacing
        /// </summary>
        public static JsonSerializerOptions CircularFormattedOptions => new JsonSerializerOptions(FormattedOptions)
        {
            ReferenceHandler = new PreserveReferenceHandler() //circular; a new handler must be added for each serialization
        };

        /// <summary>
        /// Outputs Json with $ notation and without friendly spacing
        /// </summary>
        public static JsonSerializerOptions CircularOptions => new JsonSerializerOptions(Options)
        {
            ReferenceHandler = new PreserveReferenceHandler() //circular; a new handler must be added for each serialization
        };

        private static class Internal
        {
            internal static readonly JsonSerializerOptions BaseOptions = new JsonSerializerOptions()
            {
                Converters =
                {
                    new EnumDescriptionConverter(),
                    new KnownTypeConverter(typeof(StringExtensions).Assembly),
                    new StringDictionaryConverter<string>()
                },
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles, //not circular
                WriteIndented = false //not formatted
            };
        }
    }
}