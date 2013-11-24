using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ABSharp
{
    public class ABSharpModule : IHttpModule
    {
        private HttpApplication _application;

        public void Dispose()
        {
            _application.BeginRequest -= OnBeginRequest;
        }

        public void Init(HttpApplication context)
        {
            _application = context;
            _application.BeginRequest += OnBeginRequest;
        }

        void OnBeginRequest(object sender, EventArgs e)
        {
            var url = HttpContext.Current.Request.Url.PathAndQuery.ToUpperInvariant();
            System.Diagnostics.Debug.WriteLine(url);

            var abSharpUrlLocation = url.IndexOf("/_ABSHARP/");
            if (abSharpUrlLocation >= 0) // Special AB# URL
            {
                var action = url.Substring(abSharpUrlLocation + 10);
                var qStart = action.IndexOf('?');
                if (qStart >= 0)
                {
                    action = action.Substring(0, qStart);
                }

                switch (action)
                {
                    case "VERIFYUSER":
                        var verificationValue = HttpContext.Current.Request.Form["v"];
                        if (verificationValue == "12")
                        {
                            Internal.ABTestManager.Instance.ProcessVerification();
                        }
                        break;
                    default:
                        // unknown action, abort our processing an let the app handle it
                        return;
                }

                // we handled, don't pass to app
                HttpContext.Current.Response.Write("OK");
                HttpContext.Current.Response.End();
            }
            else // Normal URL, but can still be a convertion URL
            {
                Internal.ABTestManager.Instance.ProcessPossibleConvertion(url);
            }

        }
    }
}
