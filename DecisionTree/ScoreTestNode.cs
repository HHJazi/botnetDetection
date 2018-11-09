using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// This class implements a testing node with an associated score value.s
    /// </summary>
    public class ScoreTestNode : TestNode
    {
        private  double _score;
        public double Score 
        { 
            get { return _score; } 
            set { _score = value; } 
        }

        #region Constructors
        public ScoreTestNode()
            : base()
        { 
        
        }

        public ScoreTestNode(double weight, Test test, double score)
            : base(weight, test)
        {
            this._score = score;
        }

        public ScoreTestNode(double weight, Test test, IEnumerable<Node> sons, double score)
         :base (weight, test, sons)
        {
            this._score = score;
        }
        #endregion



    }
}
