using System.Threading.Tasks;

namespace n_ate.Essentials
{
    public static class TaskExtensions
    {
        public static object? GetValueOrNull(this Task task)
        {
            task.Wait();
            if (task.TryGetValue(nameof(Task<object>.Result), out object? value)) return value;
            return null;
        }
    }
}