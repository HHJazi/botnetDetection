using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// An anchor node. each tree has one (and only one) anchor rood node which is root's father;
    /// this node has only one son.
    /// </summary>
    public sealed class AnchorNode : Node
    {
        protected DecisionTree _tree;
        private Node _root;

        public AnchorNode()
            : base()
        { }

        public AnchorNode(DecisionTree tree)
            : base(0.0)
        {
            this._tree = tree;
            this._root = new OpenNode(0.0);
            this._root.Father = this;
        }

        protected override void ReplaceSon(Node oldRoot, Node newSon)
        {
            //if (oldRoot != _root)
            //    throw new ArgumentException("Invalid root");

            this._root = newSon;
        }

        public Node Son()
        {
            return _root;
        }

        public override Node Son(int id)
        {
            if (id != 0)
                throw new ArgumentException("the index must be 0");

            return _root;
        }

        public override bool HasOpenNode()
        {
            return _root.HasOpenNode();
        }

        public override int NumOfSons()
        {
            return 1;
        }

        public override DecisionTree Tree()
        {
            return _tree;
        }

        public override void UpdateHasOpenNode()
        {
            ;
        }

        public override bool IsLeaf()
        {
            return false;
        }
    }
}
