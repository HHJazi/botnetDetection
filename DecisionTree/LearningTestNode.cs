using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// This class impelments the learning test node.
    /// </summary>
    public class LearningTestNode : ScoreTestNode, LearningNode
    {
        private ItemSet _learningSet;
   

        public LearningTestNode() : base()
        {
    
        }

        public LearningTestNode(double weight, Test test, double score, ItemSet itemSet)
            : base(weight, test, score)
        {
            if (itemSet == null)
                throw new ArgumentNullException();

            this._learningSet = itemSet;

            for (int i = 0; i < this.NumOfSons(); i++)
            {
                this.Son(i).Replace(new LearningOpenNode(this.Son(i).Weight, null));
            }
        }

        public override void Replace(Node node)
        {
            //if (!(node is LearningNode))
            //    throw new ArgumentException("A learning node can only be replaced by another learning node.");

            base.Replace(node);
        }

        public ItemSet LearningSet()
        {
            return _learningSet;
        }
    }
}
