using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// This class implements a 'open' node.
    /// A node is open when its purpose has not been fixed. It has no son.
    /// When a tree is created, its root is an open node. The tree is completed when 
    /// all the open nodes have been replaced by test nodes ore leaves.
    /// </summary>
    public class OpenNode : Node
    {

        public OpenNode()
        { 
        
        }

        public OpenNode(double weight)
            : base(weight)
        {
            ;
        }

        public override bool HasOpenNode()
        {
            return true;
        }

        public override void UpdateHasOpenNode()
        {
            ;
        }

        protected override void ReplaceSon(Node oldRoot, Node newSon)
        {
            throw new InvalidOperationException("OpenNode has no son.");
        }

        public override Node Son(int id)
        {
            throw new InvalidOperationException("OpenNode has no son.");
        }

        public override int NumOfSons()
        {
            return 0;
        }
    }
}
