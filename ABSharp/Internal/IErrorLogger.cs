using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABSharp.Internal
{
    public interface IErrorLogger
    {
        void LogError(Exception ex);
    }
}
