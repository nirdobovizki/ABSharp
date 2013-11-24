using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ABSharp.Console.Models
{
    public class TestDesc
    {
        public string Id;
        public DateTime First;
        public DateTime Last;
        public Dictionary<int,TestOptionDesc> Options = new Dictionary<int, TestOptionDesc>();

        public double Improvment { get { return Options.Count < 2 ? 0 : Options[0].ConvertionRate == 0 ? 1 : (Options[1].ConvertionRate - Options[0].ConvertionRate) / Options[0].ConvertionRate; } }
        public double Confidence
        {
            get
            {
                if (Options.Count < 2) return 0;
                double a = Options[0].TotalCount - Options[0].ConvetCount;
                double b = Options[0].ConvetCount;
                double c = Options[1].TotalCount - Options[1].ConvetCount;
                double d = Options[1].ConvetCount;
                double x = (((a * d) - (b * c)) * ((a * d) - (b * c)) * (a + b + c + d)) / ((a + b) * (c + d) * (b + d) * (a + c));

                //                double x = 4 * Math.Pow((Options[0].ConvetCount - Options[1].ConvetCount) / 2, 2) / (Options[0].ConvetCount + Options[1].ConvetCount);
                return x > 10.8 ? 0.999 : x > 6.635 ? 0.99 : x > 5 ? 0.975 : x > 3.8 ? 0.95 : x > 2.7 ? 0.9 : 0;
            }
        }
    }
}