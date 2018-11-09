using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// Decision Tree node.
    /// </summary>
    public abstract class Node
    {
        #region Properties

        /// <summary>
        /// Father node 
        /// </summary>
        [XmlIgnore]
        public Node Father { get; set; }

        public double Weight { get; set; }

        /// <summary>
        /// Number of sons of this node.
        /// <remarks>This property is only used for serialization. Don't used in anywhere else.</remarks>
        /// </summary>
        private int _nbSons;
        public int NbSons { 
            get { return NumOfSons(); }
            set { _nbSons = value; }
        }

        #endregion

        #region Constructor

        public Node()
        { 
        
        }

        public Node(double weight)
        {
            Father = null;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Returns the leftmost open onode of the subtree defined by this object.
        /// Leftmost means that the son number chosen at each test node while descending
        /// the tree is the smalleset.
        /// </summary>
        /// <returns></returns>
        public virtual OpenNode OpenNode()
        {
            if (HasOpenNode() == false)
                return null;

            if (this is OpenNode)
            {
                return (OpenNode)this;
            }
            else
            {
                for (int i = 0; i < NumOfSons(); i++)
                {
                    if (Son(i).HasOpenNode())
                    {
                        return Son(i).OpenNode();
                    }
                }
            }

            throw new Exception("Tree error!");
        }

        /// <summary>
        /// Check if this node is the tree root.
        /// </summary>
        /// <returns></returns>
        public bool IsRoot()
        {
            return Father != null && (Father is AnchorNode);
        }

        abstract protected void ReplaceSon(Node oldRoot, Node newSon);

        /// <summary>
        /// Shows if one of the descendants of this node is open. More formally, this functions 
        /// returns true iff this node is open or there exists one son of this node 's'.
        /// </summary>
        /// <returns></returns>
        abstract public bool HasOpenNode();

        /// <summary>
        /// Check if the return value of hasOpenNode could have change. If yes, 
        /// calls Father.UpdateHashOpenNode().
        /// </summary>
        abstract public void UpdateHasOpenNode();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public virtual void Replace(Node node)
        { 
            node.Father = this.Father;

            if (Father != null)
                Father.ReplaceSon(this, node);

            Father = null;
        }

        abstract public int NumOfSons();

        /// <summary>
        /// Returns a son Node of this node. Sons are described by an id.
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        abstract public Node Son(int id);

        public virtual bool IsLeaf() 
        {
            return (NumOfSons() == 0);
        }

        /// <summary>
        /// Returns the tree associated to this node. Notice that this requires to find the root of the tree, an 
        /// operation with compelxity log(n) on average.
        /// </summary>
        /// <returns>The node's tree, or null if the node is not associated to a tree</returns>
        public virtual DecisionTree Tree()
        {
            return Father == null ? null : Father.Tree();
        }

        #endregion
    }
}
