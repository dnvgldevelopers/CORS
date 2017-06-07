using System;
using System.Collections.Generic;
using System.IO;

namespace VerIT.WebExtensions.CORS.Configuration
{
    /// <summary>
    /// Class to update web.config with CORS Configuration
    /// </summary>
    public class FileConfiguration : IConfiguration
    {
        private const string KEY_FORMAT = "{0}<{1}";

        private readonly string _filename;
        private readonly int _updateFrequency;

        private Dictionary<string, bool> _cache;
        private DateTime _updated;
        private DateTime _checkForUpdates;

        /// <summary>
        /// Initialize File Configuration with properties
        /// </summary>
        public FileConfiguration(string filename, int updateFrequency)
        {
            /* 
             * Example CORS configuration in cors-config.txt:
             * 
             * {host} < {origin}
             * search.mydomain.com<https://intranet.mydomain.com
             * mysite.mydomain.com<https://intranet.mydomain.com
             */
            _filename = filename;
            _updateFrequency = updateFrequency;

            RefreshCacheFromFile();
        }
        /// <summary>
        /// Refresh Cache from CORS Config file
        /// </summary>
        private void RefreshCacheFromFile()
        {
            // Create a new empty dictionary cache
            Dictionary<string, bool> newCache = new Dictionary<string, bool>();
            using (var sr = new StreamReader(new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    if (!s.StartsWith("#"))
                    {
                        newCache.Add(s.Trim(), true);
                    }
                }
            }

            try
            {
                // Update timestamp
                _updated = File.GetLastWriteTime(_filename);
            }
            catch (Exception)
            {
                _updated = DateTime.UtcNow;
            }
            _cache = newCache;

            // Wait at least x seconds before checking for updates
            _checkForUpdates = DateTime.UtcNow.AddSeconds(_updateFrequency);
        }
        /// <summary>
        /// Check if the host and origin is allowed in the CORS Call
        /// </summary>
        public bool IsAllowed(string host, string origin)
        {
            if (DateTime.UtcNow > _checkForUpdates)
            {
                // It is time to check for updates
                DateTime fileModifiedTime = File.GetLastWriteTime(_filename);
                if (fileModifiedTime > _updated)
                {
                    // Only reload file if modified since last read
                    RefreshCacheFromFile();
                }
                else
                {
                    // Wait at least x seconds before checking for updates next
                    _checkForUpdates = DateTime.UtcNow.AddSeconds(_updateFrequency);
                }
            }
            bool allowed;
            _cache.TryGetValue(string.Format(KEY_FORMAT, host, origin), out allowed);
            return allowed;
        }
    }
}
