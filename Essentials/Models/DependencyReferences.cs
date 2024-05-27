using System.Collections.Generic;

namespace n_ate.Essentials.Models
{
    public class CompilationOptions : Dictionary<string, string>
    {
    }

    public class Dependencies : Dictionary<string, string>
    {
    }

    public class DependencyReferences
    {
        public CompilationOptions? CompilationOptions { get; set; }
        public Libraries? Libraries { get; set; }
        public RuntimeTarget? RuntimeTarget { get; set; }
        public Targets? Targets { get; set; }
    }

    public class Libraries : Dictionary<string, Library>
    {
    }

    public class Library
    {
        public string? HashPath { get; set; }
        public string? Path { get; set; }
        public bool Serviceable { get; set; }
        public string? Sha512 { get; set; }
        public string? Type { get; set; }
    }

    public class Project
    {
        public Dependencies? Dependencies { get; set; }
        public Runtime? Runtime { get; set; }
    }

    public class Projects : Dictionary<string, Project>
    {
    }

    public class Runtime : Dictionary<string, object>
    {
    }

    public class RuntimeTarget
    {
        public string? Name { get; set; }
        public string? Signature { get; set; }
    }

    public class Targets : Dictionary<string, Projects>
    {
    }
}