using System.Security.Claims;

namespace HelpDesk.Controllers.Common
{
    public class GetCurrentUser
    {
        public Guid GetCurrentUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("Токен не содержит идентификатора пользователя");
            }

            return Guid.Parse(userIdClaim);
        }
    }
}
