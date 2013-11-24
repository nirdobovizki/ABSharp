using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABSharp.Internal
{
    public interface IDatabaseSession : IDisposable
    {
        void SetTestData(string testId, string convertionUrl);
        
        int GetOptionForUserAndTest(string userId, string testId);

        void SetOptionForUserAndTest(string userId, string testId, int option);

        bool GetIsUserVerified(string userId);

        void SetIsUserVerified(string userId);

        void SetConvertion(string testId, string userId);

        IEnumerable<TestData> GetTestData();
    }
}
