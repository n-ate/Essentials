using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace n_ate.Essentials
{
    public static class HttpRequestExtensions
    {
        public static ClaimsPrincipal GetPrincipalWithSubjectClaim(this HttpRequest request, ClaimsPrincipal principal)
        {
            var bearer = request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", string.Empty) ?? string.Empty;
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadToken(bearer) as JwtSecurityToken;
            (principal.Identity as ClaimsIdentity)?.AddClaim(new Claim("sub", jwt?.Subject ?? "", "Subject", jwt?.Issuer));
            return principal;
        }
    }
}