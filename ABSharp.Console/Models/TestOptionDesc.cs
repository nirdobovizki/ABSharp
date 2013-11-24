using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ABSharp.Console.Models
{
    public class TestOptionDesc
    {
        public int Id;
        public double TotalCount;
        public double ConvetCount;
        public double ConvertionRate { get { return ConvetCount / TotalCount; } }
        public double StandardError { get { return TotalCount == 0 ? 0 : Math.Sqrt(ConvertionRate * (1 - ConvertionRate) / TotalCount); } }
        public double Error99 { get { return StandardError * 1.96; } }
        public double Error95 { get { return StandardError * 1.65; } }
    }
}