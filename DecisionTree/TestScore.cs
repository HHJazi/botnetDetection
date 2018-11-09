using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    public class TestScore : IComparable
    {
        /// <summary>
        /// The test 
        /// </summary>
        public Test Test { get; set; }
        
        public double Score { get; set; }

        public TestScore(Test test, double score)
        {
            Test = test;
            Score = score;
        }

        public int CompareTo(object obj)
        {
            TestScore  to = (TestScore)obj;

            if (Score < to.Score)
                return -1;
            else if (Score > to.Score)
                return 1;
            else
                return 0;
        }

        public override string ToString()
        {
            return "Test: " + Test.ToString() + "Score: " + Score;
        }
    }
}
