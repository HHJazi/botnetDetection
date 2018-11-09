using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// This class implements the open node impelmenting the LearningNode interface.
    /// </summary>
    public class LearningOpenNode : OpenNode, LearningNode 
    {
        private ItemSet _learningSet;

        public LearningOpenNode()
            : base()
        { }

        public LearningOpenNode(double weight, ItemSet learningSet)
            : base(weight)
        {
            this._learningSet = learningSet;
        }

        public override void Replace(Node node)
        {
            if (!(node is LearningNode))
            {
                throw new InvalidOperationException();
            }

            base.Replace(node);
        }

        public ItemSet LearningSet()
        {
            return _learningSet;
        }
    }
}
