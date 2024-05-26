using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;

namespace n_ate.Essentials
{
    public static class IConfigurationExtensions
    {
        /// <summary>
        /// Cross OS configuration get that ignores the "__" and ":" differences between windows and linux configuration key names.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <returns>The configuration value or null.</returns>
        public static string? Get(this IConfiguration configuration, string key)
        {
            var result = configuration[key];
            if (result is not null) return result;

            var nextKey = key.Replace("__", ":");//in linux containers "__" should map to ":".
            Debug.WriteLine($"Configuration key not found: {key}. Checking for key: {nextKey}.");
            result = configuration[nextKey];
            if (result is not null) return result;

            key = nextKey;
            nextKey = key.Replace(":", "__");
            Debug.WriteLine($"Configuration key not found: {key}. Checking for key: {nextKey}.");
            result = configuration[nextKey];
            if (result is not null) return result;

            Debug.WriteLine($"Configuration key not found: {key}. Returning null.");
            return null;
        }

        /// <summary>
        /// Cross OS configuration get that ignores the "__" and ":" differences between windows and linux configuration key names.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <returns>The configuration value or null.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no such key is found or the value associated with the key is null.</exception>
        public static string GetOrThrow(this IConfiguration configuration, string key)
        {
            var result = configuration.Get(key);
            if (result is not null) return result;
            Debug.WriteLine($"Configuration key not found: {key}. Throwing.");
            throw new InvalidOperationException($"Expected a non-null configuration \"{key}\". Check that the key is spelled correctly and that the value is set.");
        }
    }
}