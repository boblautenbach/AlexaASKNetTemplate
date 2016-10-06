using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace EchoTemplate.Helpers
{
    public static class AppSettings
    {
        public static string RedisCache = ConfigurationManager.AppSettings["RedisCache"];

        public static string AmazonAppId = ConfigurationManager.AppSettings["AmazonAppId"];
    }
}