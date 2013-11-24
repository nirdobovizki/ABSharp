using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ABSharp
{
    public static class Test
    {
        public static bool Begin(string testName, string convertionUrl)
        {
            return Begin(testName, convertionUrl, null, false);
        }

        public static bool Begin(string testId, string convertionUrl, string userId, bool userVerified)
        {
            int option;
            bool requireVerify;
            Internal.ABTestManager.Instance.ProcessEnterance(testId, convertionUrl, userId, userVerified, 2, out option, out requireVerify);

            if (requireVerify)
            {
                HttpContext.Current.Items["ABSharp.requireVerify"] = true;
            }
            return option == 1;
        }

        public static string EmitJS()
        {
            if (HttpContext.Current.Items.Contains("ABSharp.requireVerify"))
            {
                return "<script> $(document).ready(function(){$.post(\"_ABSharp/VerifyUser\", {r : Math.random(), v : 5 + 7});}); </script>";
            }
            else
            {
                return "";
            }
        }
    }
}
