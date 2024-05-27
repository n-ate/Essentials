using System;
using System.Text.Json;

namespace n_ate.Essentials
{
    public static class JsonElementExtensions
    {
        public static JsonElement? GetProperty(this JsonElement objValueKind, string propertyName)
        {
            if (objValueKind.ValueKind == JsonValueKind.Object)
            {
                var enumerator = objValueKind.EnumerateObject();
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Name == propertyName)
                    {
                        return enumerator.Current.Value;
                    }
                }
                return null;
            }
            else throw new ArgumentException();
        }

        public static string GetStringProperty(this JsonElement objValueKind, string propertyName)
        {
            var element = objValueKind.GetProperty(propertyName);
            if (element.ValueKind == JsonValueKind.String)
            {
                return element.GetString()!;
            }
            else throw new NotImplementedException($"{propertyName} must be string.");
        }
    }
}