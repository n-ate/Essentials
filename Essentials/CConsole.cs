using System;

namespace n_ate.Essentials
{
    /// <summary>
    /// Console helper class
    /// </summary>
    public abstract class CConsole(ConsoleColor textColor)
    {
        public ConsoleColor TextColor { get; } = textColor;

        public void Write(string? value) => Output(value);

        public void WriteLine(string? value) => Output(value, true);

        public void WriteMemberErred(string sourceName, string sourceValue, string memberName) => Output($"Erred {memberName}. {sourceName}: {sourceValue}\n");

        private void Output(string? value, bool newLine = false)
        {
            if (value == null)
            {
                if (newLine) Console.WriteLine();
                return;
            }
            var color = Console.ForegroundColor;
            Console.ForegroundColor = TextColor;
            if (newLine) Console.WriteLine(value);
            else Console.Write(value);
            Console.ForegroundColor = color;
        }
    }
}