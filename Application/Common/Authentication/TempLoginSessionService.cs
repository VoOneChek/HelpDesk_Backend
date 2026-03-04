using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Authentication
{
    public class TempLoginSessionService
    {
        private readonly Dictionary<string, TempLoginSession> _sessions = new();

        public string CreateSession(Guid userId, string code, TimeSpan lifetime)
        {
            var sessionId = Guid.NewGuid().ToString();
            _sessions[sessionId] = new TempLoginSession
            {
                UserId = userId,
                Code = code,
                ExpiresAt = DateTime.UtcNow.Add(lifetime)
            };
            return sessionId;
        }

        public TempLoginSession? GetSession(string sessionId)
        {
            _sessions.TryGetValue(sessionId, out var session);
            return session;
        }

        public void RemoveSession(string sessionId)
        {
            _sessions.Remove(sessionId);
        }
    }
}
