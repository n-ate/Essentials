using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace n_ate.Essentials
{
    public static class Files
    {
        public static string[] FindMatchingAbsoluteDirectoryPaths(string relativeDirectoryPath)
        {
            var results = new string[0];
            relativeDirectoryPath = relativeDirectoryPath.Trim(Path.DirectorySeparatorChar);

            if (relativeDirectoryPath == string.Empty)
            {
                results = Environment.CurrentDirectory.ToSingleItemArray().Concat(Directory.GetDirectories(Environment.CurrentDirectory, "", SearchOption.AllDirectories)).ToArray();
            }
            else
            {
                var parts = relativeDirectoryPath.Split(Path.DirectorySeparatorChar, 2);
                var firstDirectory = parts[0];
                var remaining = parts.Length == 2 ? parts[1] : null;
                var firstDirectoryMatches = new string[0];
                try
                {
                    Debug.WriteLine($"Searching for directory matches for first directory in path: {firstDirectory}");
                    firstDirectoryMatches = Directory.GetDirectories(Environment.CurrentDirectory, firstDirectory, SearchOption.AllDirectories);
                }
                catch (DirectoryNotFoundException) { }
                if (firstDirectoryMatches.Any())
                {
                    var absoluteMatches = new List<string>();
                    Debug.WriteLine("Found the following matching directories:");
                    foreach (var directory in firstDirectoryMatches)
                    {
                        Debug.WriteLine($"    {directory}");
                        var directoryPath = remaining is null ? directory : Path.Combine(directory, remaining);
                        if (Directory.Exists(directoryPath)) absoluteMatches.Add(directoryPath);
                    }
                    if (absoluteMatches.Any())
                    {
                        Debug.WriteLine("Found the following matching file paths:");
                        foreach (var match in absoluteMatches) Debug.WriteLine($"    {match}");
                    }
                    results = absoluteMatches.ToArray();
                }
                else
                {
                    Debug.WriteLine("No matching directories were found.");
                }
            }
            return results;
        }

        /// <summary>
        /// Registers the wwwroot directory wherever it is found for static file use as the static file root.
        /// </summary>
        public static string[] FindMatchingAbsoluteFilePaths(string relativeFilePath)
        {
            string[] results = new string[0];
            if (relativeFilePath is not null)
            {
                relativeFilePath = Path.DirectorySeparatorChar == '\\' ? relativeFilePath.Replace('/', Path.DirectorySeparatorChar) : relativeFilePath.Replace('\\', Path.DirectorySeparatorChar); //converts Windows or Linux pathing to native
                var absoluteFilePath = Path.GetFullPath(relativeFilePath);
                if (File.Exists(absoluteFilePath))
                {
                    results = absoluteFilePath.ToSingleItemArray(); //absolute path was easily constructed
                }
                else //search for path
                {
                    Debug.WriteLine($"Searching for directory matches for relative file path: {relativeFilePath}");
                    var fileName = Path.GetFileName(relativeFilePath);
                    var directoryPath = relativeFilePath.Substring(0, relativeFilePath.Length - fileName.Length);
                    string[] directories = FindMatchingAbsoluteDirectoryPaths(directoryPath);
                    if (directories.Any())
                    {
                        var absoluteMatches = new List<string>();
                        Debug.WriteLine("Found the following matching directories:");
                        foreach (var directory in directories)
                        {
                            Debug.WriteLine($"    {directory}");
                            var filePath = Path.Combine(directory, fileName);
                            if (File.Exists(filePath)) absoluteMatches.Add(filePath);
                        }
                        if (absoluteMatches.Any())
                        {
                            Debug.WriteLine("Found the following matching file paths:");
                            foreach (var match in absoluteMatches) Debug.WriteLine($"    {match}");
                        }
                        results = absoluteMatches.ToArray();
                    }
                    else
                    {
                        Debug.WriteLine("No matching directories were found.");
                    }
                }
            }
            return results;
        }
    }
}