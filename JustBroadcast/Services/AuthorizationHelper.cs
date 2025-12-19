using JustBroadcast.Models;
using System.Security.Claims;

namespace JustBroadcast.Services
{
    public static class AuthorizationHelper
    {
        public static bool HasRole(this ClaimsPrincipal user, UserRole role)
        {
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userRole))
                return false;

            return Enum.TryParse<UserRole>(userRole, out var parsedRole) && parsedRole == role;
        }

        public static bool HasAnyRole(this ClaimsPrincipal user, params UserRole[] roles)
        {
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userRole))
                return false;

            if (Enum.TryParse<UserRole>(userRole, out var parsedRole))
            {
                return roles.Contains(parsedRole);
            }

            return false;
        }

        public static UserRole? GetUserRole(this ClaimsPrincipal user)
        {
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userRole))
                return null;

            return Enum.TryParse<UserRole>(userRole, out var parsedRole) ? parsedRole : null;
        }

        public static bool CanManagePlayouts(this ClaimsPrincipal user)
        {
            return user.HasAnyRole(UserRole.Supervisor, UserRole.Administrator, UserRole.Operator);
        }

        public static bool CanManageUsers(this ClaimsPrincipal user)
        {
            return user.HasAnyRole(UserRole.Supervisor, UserRole.Administrator);
        }

        public static bool CanManageSettings(this ClaimsPrincipal user)
        {
            return user.HasAnyRole(UserRole.Supervisor, UserRole.Administrator);
        }

        public static bool CanViewOnly(this ClaimsPrincipal user)
        {
            return user.HasRole(UserRole.Viewer);
        }

        public static bool IsSupervisor(this ClaimsPrincipal user)
        {
            return user.HasRole(UserRole.Supervisor);
        }

        public static bool IsAdministrator(this ClaimsPrincipal user)
        {
            return user.HasRole(UserRole.Administrator);
        }

        public static bool IsOperator(this ClaimsPrincipal user)
        {
            return user.HasRole(UserRole.Operator);
        }
    }
}
