using Microsoft.AspNetCore.Http;
using SharedLibrary;

namespace Backend_Pixel_Crawler.Managers
{
    public class TCPSessionManager
    {
        private Dictionary<string, TCPSession> Sessions = new Dictionary<string, TCPSession>();

        public void AddSession(TCPSession session)
        {
            if (!Sessions.ContainsKey(session.SessionId))
             {
                Sessions.Add(session.SessionId, session);
              }

        }

        public TCPSession GetSession(string sessionId)
        {
            if (Sessions.ContainsKey(sessionId))
            {
                Sessions.TryGetValue(sessionId, out TCPSession session);
                return session;
            }

            return null;
        }

        public async Task RemoveSessionAsync(string sessionId)
        {
            if (Sessions.ContainsKey(sessionId))
            {
                await Sessions[sessionId].CloseAsync();
                Sessions.Remove(sessionId);
            }

        }

        public async Task DisconnectInactiveUsersAsync()
        {
            var timedOutSessions = Sessions.Where(keys => (DateTime.Now - keys.Value.LastStreamActivity) > TimeSpan.FromMinutes(15)).ToList();

            foreach(var session in timedOutSessions)
            {
                await RemoveSessionAsync(session.Key);
            }
        }
    }
}
