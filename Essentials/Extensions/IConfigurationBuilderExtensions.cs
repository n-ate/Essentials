using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace n_ate.Essentials
{
    public static class IConfigurationBuilderExtensions
    {
        public const string ENV_DOCKERFILE_NAME = "DockerFile.env";

        public static IConfigurationBuilder AddEnvDockerFile(this IConfigurationBuilder builder, ILogger logger, string? path = null)
        {
            if (path == null) path = FindEnvDockerFiles(logger).FirstOrDefault();
            if (path == null) LogDebug(logger, $"A {ENV_DOCKERFILE_NAME} file was not found. The {ENV_DOCKERFILE_NAME} configurations will not be loaded.");
            else
            {
                LogDebug(logger, $"A {ENV_DOCKERFILE_NAME} file was found.");
                var keyValues = new Dictionary<string, string>();
                var lines = File.ReadLines(path);
                foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)))
                {
                    var keyValue = line.Split('=', 2);
                    keyValues[keyValue![0]] = keyValue?[1] ?? "";
                    keyValues[keyValue![0].Replace(":", "__")] = keyValue?[1] ?? "";
                    keyValues[keyValue![0].Replace("__", ":")] = keyValue?[1] ?? ""; //in linux containers "__" should map to ":". Not everyone knows this so add all possible combinations.
                }
                builder.AddInMemoryCollection(keyValues!);
                LogDebug(logger, $"The {ENV_DOCKERFILE_NAME} configurations were successfully loaded.");
            }
            return builder;
        }

        private static string[] FindEnvDockerFiles(ILogger logger)
        {
            var searchLocations = new Dictionary<string, string>();
            searchLocations[Environment.CurrentDirectory] = $"Searching for {ENV_DOCKERFILE_NAME} file in executing directory.";
            searchLocations[Environment.CurrentDirectory.Split($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")[0]] = $"Searching for {ENV_DOCKERFILE_NAME} file in project directory.";
            searchLocations[Path.GetFullPath(Path.Combine(Environment.CurrentDirectory.Split($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")[0], $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}"))] = $"Searching for {ENV_DOCKERFILE_NAME} file in solution directory.";

            var files = new string[0];
            while (searchLocations.Any() && !files.Any())
            {
                var search = searchLocations.First();
                searchLocations.Remove(search.Key);
                LogDebug(logger, search.Value);
                try
                {
                    files = Directory.GetFiles(search.Key, ENV_DOCKERFILE_NAME, SearchOption.AllDirectories);
                }
                catch
                {
                    LogDebug(logger, $"Error trying to find {ENV_DOCKERFILE_NAME} in {search.Key}.");
                }
                if (files.Any())
                {
                    LogDebug(logger, $"Found the following {ENV_DOCKERFILE_NAME} files:");
                    foreach (var file in files) LogDebug(logger, file.PadLeft(3));
                }
                else LogError(logger, $"No {ENV_DOCKERFILE_NAME} file was found.");
            }
            return files;
        }

        private static void LogDebug(ILogger logger, string message)
        {
            logger.LogDebug(message);
            Debug.WriteLine(message);
        }

        private static void LogError(ILogger logger, string message)
        {
            logger.LogError(message);
            Debug.WriteLine(message);
        }
    }
}