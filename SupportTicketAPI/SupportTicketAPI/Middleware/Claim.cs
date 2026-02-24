using SupportTicketAPI.Models;
using System.Security.Claims;

namespace SupportTicketAPI.Middleware
{
    public static class Claim
    {
        public static int GetUserId(this ClaimsPrincipal user)
        => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public static RoleName GetRole(this ClaimsPrincipal user)
            => Enum.Parse<RoleName>(user.FindFirstValue(ClaimTypes.Role)!);
    }
}
