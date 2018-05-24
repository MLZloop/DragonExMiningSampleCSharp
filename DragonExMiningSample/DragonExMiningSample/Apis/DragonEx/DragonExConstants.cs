using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonExMiningSampleCSharp.Apis.DragonEx
{
    class DragonExConstants
    {
        /// <summary>
        /// ACCESS KEY For A
        /// </summary>
        public static string ACCESS_KEY_A = DragonExMiningSampleCSharp.Properties.Settings.Default.DragonEx_KeyA;

        /// <summary>
        /// SECRET KEY For A
        /// </summary>
        public static string SECRET_KEY_A = DragonExMiningSampleCSharp.Properties.Settings.Default.DragonEx_SecretA;

        /// <summary>
        /// ACCESS KEY For B
        /// </summary>
        public static string ACCESS_KEY_B = DragonExMiningSampleCSharp.Properties.Settings.Default.DragonEx_KeyB;

        /// <summary>
        /// SECRET KEY For B
        /// </summary>
        public static string SECRET_KEY_B = DragonExMiningSampleCSharp.Properties.Settings.Default.DragonEx_SecretB;

        /// <summary>
        /// API Base Url
        /// </summary>
        public const string API_BASE_URL = "https://openapi.dragonex.im";

        /// <summary>
        /// API URL Format {0}:Base Url, {1}:Api relative path
        /// </summary>
        public const string API_URL_FORMAT = "{0}{1}";
    }
}
