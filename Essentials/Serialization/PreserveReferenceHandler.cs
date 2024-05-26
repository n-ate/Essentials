using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace n_ate.Essentials.Serialization
{
    /// <summary>
    /// Allows custom object converters to preserve references ($ref, $id) the same way the native converters do.
    /// </summary>
    public class PreserveReferenceHandler : ReferenceHandler
    {
        private ReferenceResolver _resolver;

        public PreserveReferenceHandler()
        {
            _resolver = new PreserveReferenceResolver();
        }

        public override ReferenceResolver CreateResolver() => _resolver;

        public string GetOrCreateReferenceId(object value, out bool extant)
        {
            return _resolver.GetReference(value, out extant);
        }

        public void WriteReferenceIdProperty(Utf8JsonWriter writer, string referenceId)
        {
            writer.WriteString("$id", referenceId);
        }

        public void WriteReferenceObject(Utf8JsonWriter writer, string referenceId)
        {
            writer.WriteStartObject();
            writer.WriteString("$ref", referenceId);
            writer.WriteEndObject();
        }

        private class PreserveReferenceResolver : ReferenceResolver
        {
            private readonly Dictionary<object, string> _objectToReferenceIdMap = new Dictionary<object, string>(ReferenceEqualityComparer.Instance);
            private readonly Dictionary<string, object> _referenceIdToObjectMap = new Dictionary<string, object>();
            private uint _referenceCount;

            public override void AddReference(string referenceId, object value)
            {
                if (!_referenceIdToObjectMap.TryAdd(referenceId, value))
                {
                    throw new JsonException();
                }
            }

            public override string GetReference(object value, out bool alreadyExists)
            {
                if (_objectToReferenceIdMap.TryGetValue(value, out string? referenceId))
                {
                    alreadyExists = true;
                }
                else
                {
                    _referenceCount++;
                    referenceId = _referenceCount.ToString();
                    _objectToReferenceIdMap.Add(value, referenceId);
                    alreadyExists = false;
                }

                return referenceId!;
            }

            public override object ResolveReference(string referenceId)
            {
                if (!_referenceIdToObjectMap.TryGetValue(referenceId, out object? value))
                {
                    throw new JsonException();
                }

                return value!;
            }
        }
    }
}