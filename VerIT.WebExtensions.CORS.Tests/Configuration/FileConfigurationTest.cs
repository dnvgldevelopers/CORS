using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;
using VerIT.WebExtensions.CORS.Configuration;

namespace VerIT.WebExtensions.CORS.Tests.Configuration
{
    [TestClass]
    public class FileConfigurationTest
    {
        [TestMethod]
        public void TestCachedConfig()
        {
            string filename = "cors-config-unittest.txt";

            CreateConfig(filename);

            int cacheTimeoutSecs = 1;
            IConfiguration c = new FileConfiguration(filename, cacheTimeoutSecs);

            //Put your hosts and origins here, following that examples

            allow(c, "search.mydomain.com", "https://intranet.mydomain.com");
            allow(c, "mysite.mydomain.com", "https://intranet.mydomain.com");

            disallow(c, "search.mydomain.com", "https://hackers-nest.ru.pl.it.net");
            disallow(c, "mysite.mydomain.com", "https://illreadyourprivatedata.net");
        }

        [TestMethod]
        public void TestExpiredCachedConfig()
        {
            string filename = "cors-config-unittest.txt";

            CreateConfig(filename);

            int cacheTimeoutSecs = 0;
            IConfiguration c = new FileConfiguration(filename, cacheTimeoutSecs);

            //Put your hosts and origins here, following that examples

            allow(c, "search.mydomain.com", "https://intranet.mydomain.com");
            allow(c, "mysite.mydomain.com", "https://intranet.mydomain.com");

            disallow(c, "search.mydomain.com", "https://hackers-nest.ru.pl.it.net");
            disallow(c, "mysite.mydomain.com", "https://illreadyourprivatedata.net");
        }

        [TestMethod]
        public void TestConfigUpdated()
        {
            string filename = "cors-config-unittest.txt";

            CreateConfig(filename);

            int cacheTimeoutSecs = 0;
            IConfiguration c = new FileConfiguration(filename, cacheTimeoutSecs);

            //Put your hosts and origins here, following that examples

            allow(c, "search.mydomain.com", "https://intranet.mydomain.com");
            allow(c, "mysite.mydomain.com", "https://intranet.mydomain.com");

            disallow(c, "search.mydomain.com", "https://hackers-nest.ru.pl.it.net");
            disallow(c, "mysite.mydomain.com", "https://illreadyourprivatedata.net");

            AddToConfig(filename, "intranet.mydomain.com<https://dms.mydomain.com");

            Thread.Sleep(1000);

            allow(c, "search.mydomain.com", "https://intranet.mydomain.com");
            allow(c, "mysite.mydomain.com", "https://intranet.mydomain.com");


            disallow(c, "search.mydomain.com", "https://hackers-nest.ru.pl.it.net");
            disallow(c, "mysite.mydomain.com", "https://illreadyourprivatedata.net");

            CreateConfig(filename);

            Thread.Sleep(1000);

            disallow(c, "intranet.mydomain.com", "https://dms.mydomain.com");
            allow(c, "search.mydomain.com", "https://intranet.mydomain.com");

        }

        private void AddToConfig(string filename, string config)
        {
            using (StreamWriter sw = File.AppendText(filename))
            {
                sw.WriteLine();
                sw.Write(config);
            }
        }

        private void CreateConfig(string filename)
        {
            using (StreamWriter sw = File.CreateText(filename))
            {
                sw.WriteLine("############################################################");
                sw.WriteLine("# This is a comment                                          ");
                sw.WriteLine("############################################################");
                sw.WriteLine("search.mydomain.com<https://intranet.mydomain.com");
                sw.WriteLine("mysite.mydomain.com<https://intranet.mydomain.com");
            }
        }

        private void allow(IConfiguration c, string host, string origin)
        {
            string msg = string.Format("CORS should be allowed for '{0}' <- '{1}'", host, origin);
            bool allowed = c.IsAllowed(host, origin);
            Assert.IsTrue(allowed, msg);
        }

        private void disallow(IConfiguration c, string host, string origin)
        {
            string msg = string.Format("CORS should not be allowed for '{0}' <- '{1}'", host, origin);
            bool allowed = c.IsAllowed(host, origin);
            Assert.IsFalse(allowed, msg);
        }
    }
}
