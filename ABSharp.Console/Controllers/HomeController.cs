using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABSharp.Console.Models;
using System.Data.SqlClient;
using System.Configuration;

namespace ABSharp.Console.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            List<TestDesc> openAndRecent = new List<TestDesc>();
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ABSharp"].ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT [TestId],[Option],Count(*),MIN(EnteranceTimestamp),MAX(EnteranceTimestamp) FROM ABSharp_Samples INNER JOIN ABSharp_VerifiedUsers ON ABSharp_Samples.UserId = ABSharp_VerifiedUsers.UserId WHERE TestId IN (SELECT DISTINCT TestId FROM ABSharp_Samples WHERE (EnteranceTimestamp > @cutoff)) GROUP BY [TestId],[Option] ORDER BY [TestId],[Option]";
                    cmd.Parameters.AddWithValue("@cutoff", DateTime.UtcNow.AddDays(-60));
                    using (var reader = cmd.ExecuteReader())
                    {
                        string lastTestId = "";
                        TestDesc lastTest = null;
                        while (reader.Read())
                        {
                            string currentTestId = (string)reader[0];
                            int currentOption = (int)reader[1];
                            if (lastTestId != currentTestId)
                            {
                                if (lastTest != null)
                                {
                                    openAndRecent.Add(lastTest);
                                }
                                lastTestId = currentTestId;
                                lastTest = new TestDesc { Id = currentTestId, First = (DateTime)reader[3], Last = (DateTime)reader[4] };
                            }
                            lastTest.Options[currentOption] = new TestOptionDesc { Id = currentOption, TotalCount = (int)reader[2], ConvetCount = -1 };
                        }
                        if (lastTest != null)
                        {
                            openAndRecent.Add(lastTest);
                        }
                    }
                }

                foreach (var currentTest in openAndRecent)
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT [Option],Count(*) "+
                            "FROM ABSharp_Convertions "+
                            "INNER JOIN ABSharp_Samples ON ABSharp_Convertions.UserId = ABSharp_Samples.UserId "+
                            "INNER JOIN ABSharp_VerifiedUsers ON ABSharp_Convertions.UserId = ABSharp_VerifiedUsers.UserId "+
                            "WHERE ABSharp_Samples.TestId = @tid AND [ABSharp_Convertions].[TestId] = @tid GROUP BY [Option]  ORDER BY [Option]";
                        cmd.Parameters.AddWithValue("@tid", currentTest.Id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int currentOption = (int)reader[0];
                                currentTest.Options[currentOption].ConvetCount = (int)reader[1];
                                /*if (lastConvert != null)
                                {
                                    Dictionary<int, TestOptionDesc> check = new Dictionary<int, TestOptionDesc>();
                                    foreach (var opt in currentTest.Options.Values)
                                    {
                                        check[opt.Id] = opt;
                                    }
                                    foreach (var opt in lastConvert.Options)
                                    {
                                        check.Remove(opt.Id);
                                    }
                                    foreach (var opt in check.Values)
                                    {
                                        lastConvert.Options.Add(new TestOptionDesc() { Id = opt.Id, TotalCount = opt.TotalCount, ConvetCount = 0 });
                                    }
                                    currentTest.Targets.Add(lastConvert);
                                }
                                TestOptionDesc optData;
                                if (currentTest.Options.TryGetValue(currentOption, out optData))
                                {
                                    lastConvert.Options.Add(new TestOptionDesc { Id = currentOption, TotalCount = optData.TotalCount, ConvetCount = (int)reader[1] });
                                }*/
                            }
                            /*if (lastConvert != null)
                            {
                                Dictionary<int, TestOptionDesc> check = new Dictionary<int, TestOptionDesc>();
                                foreach (var opt in currentTest.Options.Values)
                                {
                                    check[opt.Id] = opt;
                                }
                                foreach (var opt in lastConvert.Options)
                                {
                                    check.Remove(opt.Id);
                                }
                                foreach (var opt in check.Values)
                                {
                                    lastConvert.Options.Add(new TestOptionDesc() { Id = opt.Id, TotalCount = opt.TotalCount, ConvetCount = 0 });
                                }
                                currentTest.Targets.Add(lastConvert);
                            }*/
                        }
                    }


                    //currentTest.Options = null;
                }
            }

            openAndRecent.Sort((a, b) => -a.Last.CompareTo(b.Last));

            ViewBag.OpenAndRecent = openAndRecent;

            return View();
        }
    }
}
