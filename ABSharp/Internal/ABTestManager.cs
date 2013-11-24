using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ABSharp.Internal
{
    class ABTestManager
    {
        private const string _cookieName = "AB.UID";

        private Dictionary<string, List<string>> _convertionUrls;
        private object _convertionUrlsLock = new object();

        public static ABTestManager Instance = new ABTestManager();

        public IDatabase Database = new SqlServer();
        public IErrorLogger ErrorLogger = null;

        public string GetUserIdFromCookie()
        {
            string userId = string.Empty;
            // get cookie
            var cookie = HttpContext.Current.Request.Cookies[_cookieName];
            if (cookie != null && !string.IsNullOrWhiteSpace(cookie.Value))
            {
                userId = cookie.Value;
            }
            return userId;
        }

        public void ProcessEnterance(string testId, string convertionUrl, string userId, bool userVerified, int optionCount, out int option, out bool requireVerify)
        {
            try
            {
                bool newUser = false;

                if (userId == null)
                {
                    // get cookie
                    userId = GetUserIdFromCookie();
                    if (string.IsNullOrEmpty(userId))
                    {
                        userId = Guid.NewGuid().ToString();
                        userVerified = false;
                        newUser = true;
                    }
                }

                HttpContext.Current.Response.Cookies.Remove(_cookieName);
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(_cookieName, userId) { Expires = DateTime.Now.AddDays(300) });

                using (var session = Database.OpenSession())
                {
                    session.SetTestData(testId, convertionUrl);
                    lock (_convertionUrlsLock)
                    {
                        if (_convertionUrls != null)
                        {
                            var cUrl = convertionUrl.ToUpperInvariant();
                            List<string> tests;
                            if(_convertionUrls.TryGetValue(cUrl,out tests))
                            {
                                if (tests.IndexOf(testId) == -1)
                                {
                                    tests.Add(testId);
                                }
                            }
                            else
                            {
                                _convertionUrls.Add(cUrl, new List<string> { testId });
                            }
                        }
                    }

                    option = -1;
                    if (!newUser)
                    {
                        option = session.GetOptionForUserAndTest(userId, testId);
                    }

                    if (option == -1)
                    {
                        option = new Random().Next(optionCount);
                        session.SetOptionForUserAndTest(userId, testId, option);
                    }

                    if (!userVerified)
                    {
                        if (newUser)
                        {
                            requireVerify = true;
                        }
                        else
                        {
                            requireVerify = !session.GetIsUserVerified(userId);
                        }
                    }
                    else
                    {
                        try
                        {
                            session.SetIsUserVerified(userId);
                        }
                        catch
                        {
                            // we probably got into a race condition on SQL server - but we don't care
                        }
                        requireVerify = false;
                    }
                }
            }
            catch(Exception ex)
            {
                option = 0;
                requireVerify = false;
                if(ErrorLogger!=null)
                {
                    ErrorLogger.LogError(ex);
                }
            }
        }

        public void ProcessPossibleConvertion(string url)
        {
            try
            {
                var userId = GetUserIdFromCookie();

                if (string.IsNullOrEmpty(userId))
                {
                    return;
                }

                lock (_convertionUrlsLock)
                {
                    if (_convertionUrls == null)
                    {
                        _convertionUrls = new Dictionary<string, List<string>>();
                        using (var session = Database.OpenSession())
                        {
                            foreach (var current in session.GetTestData())
                            {
                                var converionUrl = current.ConvertionUrl.ToUpperInvariant();
                                List<string> tests;
                                if (!_convertionUrls.TryGetValue(converionUrl, out tests))
                                {
                                    tests = new List<string>();
                                    _convertionUrls.Add(converionUrl, tests);
                                }
                                tests.Add(current.TestId);
                            }
                        }
                    }

                    var qStart = url.IndexOf('?');
                    if (qStart >= 0)
                    {
                        url = url.Substring(0, qStart);
                    }

                    List<string> participatingTest;
                    if (_convertionUrls.TryGetValue(url, out participatingTest))
                    {
                        using (var session = Database.OpenSession())
                        {
                            foreach (var currentTest in participatingTest)
                            {
                                session.SetConvertion(currentTest, userId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ErrorLogger != null)
                {
                    ErrorLogger.LogError(ex);
                }
            }

        }

        public void ProcessVerification()
        {
            try
            {
                string userId = GetUserIdFromCookie();

                if (string.IsNullOrEmpty(userId)) 
                {
                    return;
                }

                using (var session = Database.OpenSession())
                {
                    session.SetIsUserVerified(userId);
                }
            }
            catch(Exception ex)
            {
                if (ErrorLogger != null)
                {
                    ErrorLogger.LogError(ex);
                }
            }
        }
    }
}
