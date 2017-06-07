using System;
using System.Net;
using System.Web;
using VerIT.WebExtensions.CORS.Configuration;
using System.Web.Configuration;
using System.IO;

namespace VerIT.WebExtensions.CORS
{
    /// <summary>
    /// Custom Http Module for enabling CORS on white-listed origins
    /// </summary>
    public class CorsHttpModule : IHttpModule
    {
        private const string EVENT_SOURCE = "VerIT.WebExtensions.CORS.CorsHttpModule";

        private const string HEADER_OPTIONS = "OPTIONS";
        private const string HEADER_ORIGIN = "ORIGIN";
        private const string HEADER_ALLOW_ORIGIN = "Access-Control-Allow-Origin";
        private const string HEADER_ALLOW_CREDENTIALS = "Access-Control-Allow-Credentials";
        private const string Allow_Framming = "AllowFraming";
        private const string Cors_Config_File = "CorsConfigFile";

        private bool _initialized = false;

        private IConfiguration _configuration;

        /// <summary>
        ///  Dispose the http module (currently not implemented)
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Initialize the http module using an external CORS configuration cached in memory with 1-hour expiration.
        /// </summary>
        /// <param name="context">the current web application</param>
        public void Init(HttpApplication context)
        {
            if (_initialized) return; // Init has run

            try
            {
                Utils.EventLog.EnsureSource(EVENT_SOURCE);
            }
            catch (Exception ex)
            {
                Utils.EventLog.LogError(ex, EVENT_SOURCE);
                throw;
            }

            try
            {
                // Initiate CORS configuration from Web.Config
                string filename = WebConfigurationManager.AppSettings[Cors_Config_File];
                if (filename.StartsWith("App_Data"))
                {
                    if (HttpRuntime.AppDomainAppPath != null)
                    {
                        filename = Path.Combine(HttpRuntime.AppDomainAppPath, filename);
                    }
                    _configuration = new FileConfiguration(filename, 60 * 5); // 5 minute cache
                    context.PreSendRequestHeaders += HandleCORSHeaders;
                }
                else
                {
                    throw new ApplicationException("Cannot read configuration file, as it is not inside App_Data");
                }
            }
            catch (Exception ex)
            {
                Utils.EventLog.LogError(ex, EVENT_SOURCE);
                // Continue
            }

            // Initiate framing configuration from Web.Config
            var allowFraming = "true".Equals(WebConfigurationManager.AppSettings[Allow_Framming]);
            if (allowFraming)
            {
                context.BeginRequest += SetFramingFlags;
                context.EndRequest += RemoveFramingHeader;
            }

            _initialized = true;
        }

        /// <summary>
        /// The CORS event handler evaluates the request method and headers and modifies the response status and headers accordingly.
        /// </summary>
        /// <param name="sender">the event source</param>
        /// <param name="e">the event arguments</param>
        private void HandleCORSHeaders(object sender, EventArgs e)
        {
            try
            {
                // Only intercept when cors is needed
                string origin = HttpContext.Current.Request.Headers[HEADER_ORIGIN];
                if (string.IsNullOrEmpty(origin)) return;

                // Only allow CORS for white-listed origins for enabled host names
                if (!_configuration.IsAllowed(HttpContext.Current.Request.Url.Host, origin)) return;

                HttpContext.Current.Response.Headers[HEADER_ALLOW_ORIGIN] = origin;
                HttpContext.Current.Response.Headers[HEADER_ALLOW_CREDENTIALS] = "true"; // Required by Chrome for CORS to work

                // Handle the preflight requests
                //
                // "It turns out that at the time of writing, all the browsers are not yet compatible with CORS 
                // nor all the servers such as IIS if the targeted domain requires to be authenticated which is 
                // usually the case in SharePoint. Because of that, preflight requests get a 401 (Unauthorized) 
                // answer from the server while it expects a 200." (http://www.silver-it.com/node/152)
                if (HEADER_OPTIONS.Equals(HttpContext.Current.Request.HttpMethod.ToUpperInvariant()))
                {
                    HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                Utils.EventLog.LogError(ex, EVENT_SOURCE);
            }
        }

        /// <summary>
        /// Set 'AllowFraming' and 'FrameOptionsHeaderSet' flags on the HttpContext to avoid 
        /// the Microsoft.SharePoint.ApplicationRuntime.SPRequestModule setting the header.
        /// 
        /// See http://sharepoint-community.net/profiles/blogs/sharepoint-2013-cross-domain-sp-modal-dialog-sameorigin-policy
        /// 
        /// Code from PermissiveXFrameHeaderModule on 
        /// https://ventigrate.codeplex.com/wikipage?title=Permissive%20XFrame%20Header
        /// </summary>
        /// <param name="sender">the event source<</param>
        /// <param name="e">the event arguments</param>
        private void SetFramingFlags(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;

            // General requests
            if (!context.Items.Contains("AllowFraming"))
                context.Items.Add("AllowFraming", String.Empty);

            // IPFS requests
            if (!context.Items.Contains("FrameOptionsHeaderSet"))
                context.Items.Add("FrameOptionsHeaderSet", String.Empty);
        }

        /// <summary>
        /// Remove the restrictive iFrame header for resources other than web pages
        /// 
        /// Code from PermissiveXFrameHeaderModule on 
        /// https://ventigrate.codeplex.com/wikipage?title=Permissive%20XFrame%20Header
        /// </summary>
        /// <param name="sender">the event source</param>
        /// <param name="e">the event arguments</param>
        private void RemoveFramingHeader(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;

            // XLViewer
            context.Response.Headers.Remove("X-FRAME-OPTIONS");
        }
    }
}
