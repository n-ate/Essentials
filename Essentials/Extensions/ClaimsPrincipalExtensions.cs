using System.Linq;
using System.Security.Claims;

namespace n_ate.Essentials
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetNameOrNull(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        }

        public static string? GetSubjectOrNull(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        }
    }
}