using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;

namespace ABSharp.Internal
{
    public class SqlServer : IDatabase
    {
        private class Session : IDatabaseSession
        {
            private SqlConnection _connection;

            public Session()
            {
                _connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ABSharp"].ConnectionString);
                _connection.Open();
            }

            public int GetOptionForUserAndTest(string userId, string testId)
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = "Select [Option] From [ABSharp_Samples] Where [UserId] = @uid And [TestId] = @tid";
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@tid", testId);
                    var dbResult = cmd.ExecuteScalar();
                    if (dbResult != null && !(dbResult is DBNull))
                    {
                        return Convert.ToInt32(dbResult);
                    }
                }
                return -1;
            }

            public void SetOptionForUserAndTest(string userId, string testId, int option)
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = "Insert Into [ABSharp_Samples] ([UserId], [TestId], [Option], [EnteranceTimestamp]) Values (@uid,@tid,@opt,@ts)";
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@tid", testId);
                    cmd.Parameters.AddWithValue("@opt", option);
                    cmd.Parameters.AddWithValue("@ts", DateTime.UtcNow);
                    cmd.ExecuteNonQuery();
                }
            }

            public bool GetIsUserVerified(string userId)
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = "Select Count(*) From [ABSharp_VerifiedUsers] Where [UserId] = @uid";
                    cmd.Parameters.AddWithValue("@uid", userId);
                    var dbResult = cmd.ExecuteScalar();
                    return !(dbResult == null || dbResult is DBNull || Convert.ToInt32(dbResult) == 0);
                }

            }

            public void SetIsUserVerified(string userId)
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = "If Not Exists (Select [UserId] From [ABSharp_VerifiedUsers] Where [UserId] = @uid) Insert Into [ABSharp_VerifiedUsers] (UserId) Values (@uid)";
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.ExecuteNonQuery();
                }
            }

            public void SetTestData(string testId, string convertionUrl)
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = "If Not Exists (Select [TestId] From [ABSharp_TestData] Where [TestId] = @tid) Insert Into [ABSharp_TestData] (TestId, ConvertionUrl) Values (@tid, @convert)";
                    cmd.Parameters.AddWithValue("@tid", testId);
                    cmd.Parameters.AddWithValue("@convert", convertionUrl);
                    cmd.ExecuteNonQuery();
                }
            }

            public void SetConvertion(string testId, string userId)
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = "If Not Exists (Select [UserId] From [ABSharp_Convertions] Where [UserId] = @uid And [TestId] = @tid) Insert Into [ABSharp_Convertions] (UserId,TestId,ConvertTimestamp) Values (@uid,@tid,@ts)";
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@tid", testId);
                    cmd.Parameters.AddWithValue("@ts", DateTime.UtcNow);
                    cmd.ExecuteNonQuery();
                }
            }

            public IEnumerable<TestData> GetTestData()
            {
                var results = new List<TestData>();
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = "Select [TestId],[ConvertionUrl] From [ABSharp_TestData]";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new TestData { TestId = (string)reader[0], ConvertionUrl = (string)reader[1] });
                        }
                    }
                }
                return results;
            }


            public void Dispose()
            {
                _connection.Dispose();
            }

        }


        public IDatabaseSession OpenSession()
        {
            return new Session();
        }
    }
}
