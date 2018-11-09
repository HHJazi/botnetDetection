using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// A test on a single attribute.
    /// </summary>
    public abstract class Test
    {
        /// <summary>
        /// The attribute on which the test is applied.
        /// </summary>
        public Attribute Attribute { get; set; }

        /// <summary>
        /// The number of possbile test issues.
        /// </summary>
        /// <returns>this test's number of issues.</returns>
        public abstract int NumOfIssues
        {
            get;
        }

        public Test()
        { 
        
        }

        public Test(Attribute attr)
        {
            if (attr == null)
                throw new ArgumentNullException();

            this.Attribute = attr;
        }

        /// <summary>
        /// Apply the test on a given value
        /// </summary>
        /// <param name="val">the value of the test</param>
        /// <returns>the issue of the test</returns>
        public abstract int Perform(AttributeValue val);



        /// <summary>
        /// Describes a test issue.
        /// </summary>
        /// <param name="num">The number of the test issue to describe. </param>
        /// <returns></returns>
        public abstract string IssueToString(int num);
        
    }
}
