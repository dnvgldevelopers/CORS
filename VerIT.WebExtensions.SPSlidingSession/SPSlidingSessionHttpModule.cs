using Microsoft.IdentityModel.Web;
using Microsoft.SharePoint;
using System;
using System.Web;

namespace VerIT.WebExtensions.SPSlidingSession
{
    /// <summary>
    /// Custom Http Module for extending the current user's session in SharePoint.
    /// </summary>
    public class SPSlidingSessionHttpModule : IHttpModule
    {
        private const string EVENT_SOURCE = "VerIT.WebExtensions.SPSlidingSession.SPSlidingSessionHttpModule";

        /// <summary>
        /// Initialize the Http Module to use the windows event log and start handle SessionSecurityTokenReceived events.
        /// </summary>
        /// <param name="context">the current web application</param>
        public void Init(HttpApplication context)
        {
            try
            {
                Utils.EventLog.EnsureSource(EVENT_SOURCE);
                Utils.EventLog.Info("SPSlidingSessionHttpModule.Init(..) Start", EVENT_SOURCE);
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    FederatedAuthentication.SessionAuthenticationModule.SessionSecurityTokenReceived += SessionAuthenticationModule_SessionSecurityTokenReceived;
                });
                Utils.EventLog.Info("SPSlidingSessionHttpModule.Init(..) Completed", EVENT_SOURCE);
            }
            catch (Exception e)
            {
                Utils.EventLog.Error(e, EVENT_SOURCE);
                throw;
            }
        }

        /// <summary>
        /// Extend the lifetime of current user's session token (SharePoint FEDAUTH cookie) if the token is still valid and half of the lifetime has passed.
        /// </summary>
        /// <param name="sender">the event source</param>
        /// <param name="e">the event arguments</param>
        void SessionAuthenticationModule_SessionSecurityTokenReceived(object sender, SessionSecurityTokenReceivedEventArgs e)
        {
            double sessionLifetimeInMinutes = (e.SessionToken.ValidTo - e.SessionToken.ValidFrom).TotalMinutes;
            var logonTokenCacheExpirationWindow = TimeSpan.FromSeconds(1);

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                logonTokenCacheExpirationWindow = Microsoft.SharePoint.Administration.Claims.SPSecurityTokenServiceManager.Local.LogonTokenCacheExpirationWindow;
            });

            DateTime now = DateTime.UtcNow;
            DateTime validTo = e.SessionToken.ValidTo - logonTokenCacheExpirationWindow;
            DateTime validFrom = e.SessionToken.ValidFrom;

            if ((now < validTo) && (now > validFrom.AddMinutes((validTo - validFrom).TotalMinutes / 2)))
            {
                SessionAuthenticationModule sam = FederatedAuthentication.SessionAuthenticationModule;
                e.SessionToken = sam.CreateSessionSecurityToken(e.SessionToken.ClaimsPrincipal,
                    e.SessionToken.Context, now, now.AddMinutes(sessionLifetimeInMinutes), e.SessionToken.IsPersistent);

                e.ReissueCookie = true;
            }
        }

        /// <summary>
        /// Dispose the http module (currently not implemented)
        /// </summary>
        public void Dispose() { }
    }
}
