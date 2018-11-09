using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// This class implements a decision tree.
    /// A decision tree is a tree where a test has been assigned to non-leaf nodes. Its aim is to guess
    /// the value of an Item's attribute (call the 'goal' attribute) thanks to tests over other attributes.
    /// If the topology of the tree is changed, take must be taken to create a valid tree, i.e. an acyclic
    /// graph where all the sons of one node are different.
    /// The tree is composed of 3 types of nodes:
    /// <list type="bullet">
    /// <item>
    /// <description>TestNodes: They are associated to a test over items' attributes.
    /// A test node has as many sons as the test's number of different outcomes.</description>
    /// </item>
    /// <item>LeafNodes: They have no sons. A leaf node is associated a goal attribute value.</item>
    /// <item>OpenNode: A node whose purpose has not been found yet. It can be replaced by a test/leaf
    /// node later on. The tree's open nodes can be efficiently (in log(nbNodes) on average) retrieved.</item>
    /// </list>
    /// </summary>
    public class DecisionTree
    {
        protected AnchorNode _anchor;

        #region Properties
        private AttributeSet _attributeSet;
        public AttributeSet AttributeSet
        {
            get { return _attributeSet; }
            set { _attributeSet = value; }
        }

        private SymbolicAttribute _goalAttribute;
        public SymbolicAttribute GoalAttribute
        {
            get { return _goalAttribute; }
            set { _goalAttribute = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// This parameterless constructor is for Xml Serialization
        /// </summary>
        public DecisionTree()
        { 
        
        }

        /// <summary>
        /// Create an empty decision tree object.
        /// </summary>
        /// <param name="attrSet"></param>
        /// <param name="goalAttr"></param>
        public DecisionTree(AttributeSet attrSet, SymbolicAttribute goalAttr)
        {
            if (attrSet == null || goalAttr == null)
                throw new ArgumentNullException();

            _anchor = new AnchorNode(this);
            _attributeSet = attrSet;
            _goalAttribute = goalAttr;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Guess goal attribute value of an item.
        /// </summary>
        /// <param name="item">The item compatible with the tree attribute set.</param>
        /// <returns>The goal attribute value, or -1 if the matching leaf node does not
        /// define a goal attribute.</returns>
        public KnownSymbolicValue GuessGoalAttribute(Item item)
        {
            double[] distribution = GoalValueDistribution(item);

            int index = -1;
            double max = -1.0d;

            for (int i = 0; i < distribution.Length; i++)
            {
                if (distribution[i] > max)
                {
                    index = i;
                    max = distribution[i];
                }
            }

            return new KnownSymbolicValue(index);
        }

        /// <summary>
        /// Finds the goal value distribution matching an item. this distribution describes the probability
        /// of each potential goal value for this item.
        /// </summary>
        /// <param name="item">An item compatible with the tree attribute set.</param>
        /// <returns>The goal attribute value distribution for the item.</returns>
        public double[] GoalValueDistribution(Item item)
        {
            return GoalValueDistribution(item, Root());
        }

        /// <summary>
        /// Find the leaf/Open node matching an item. All the (tested)attributes of the item
        /// must be known.
        /// </summary>
        /// <param name="item">An item compatible with the tree attribute set.</param>
        /// <returns>The leaf node matching item</returns>
        public Node LeafNode(Item item)
        {
            if (_attributeSet == null || _goalAttribute == null)
                throw new InvalidOperationException("No attribute set or goal attribute defined.");

            AttributeSet attrSet = _attributeSet;

            Node node = Root();

            while (!(node.IsLeaf()))
            {
                TestNode testNode = (TestNode)node;

                int testAttrIndex = attrSet.IndexOf(testNode.Test.Attribute);

                node = testNode.MatchingSon(item.ValueOf(testAttrIndex));
            }

            return node;
        }

        /// <summary>
        /// Check if a given node is the root node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool IsRoot(Node node)
        {
            if (node == null)
                throw new ArgumentNullException();

            return node.Equals(Root());
        }

        /// <summary>
        /// Return the leftmost open node of the tree.
        /// 'Leftmost' means that the son chosen at each test node while descending the tree is the smallest
        /// number.
        /// </summary>
        /// <returns>The leftmost open node of the tree, or null if the tree has no open node.</returns>
        public OpenNode OpenNode()
        {
            return _anchor.OpenNode();
        }

        /// <summary>
        /// Checks if the tree has open nodes.
        /// </summary>
        /// <returns></returns>
        public bool HasOpenNode()
        {
            return _anchor.HasOpenNode();
        }

        /// <summary>
        /// Breadth first iterator.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Node> BFIterator()
        {
            List<Node> retList = new List<Node>();

            Queue<Node> queue = new Queue<Node>();

            Node curNode;
            int nbSons = 0;

            queue.Enqueue(_anchor);

            do
            {
                curNode = queue.Dequeue();
                retList.Add(curNode);

                nbSons = curNode.NumOfSons();
                if (nbSons > 0)
                {
                    for (int i = 0; i < nbSons; i++)
                        queue.Enqueue(curNode.Son(i));
                }
            }
            while (queue.Count > 0);

            return retList;
        }

        #endregion

        #region Private Methods

        protected double[] GoalValueDistribution(Item item, Node node)
        {
            if (node.IsLeaf())
            {
                return ((LeafNode)node).GoalValueDistribution;
            }
            else
            {
                if (node is TestNode)
                {
                    TestNode tNode = (TestNode)node;

                    int testAttrIndex = _attributeSet.IndexOf(tNode.Test.Attribute);

                    if (item.ValueOf(testAttrIndex).IsUnknown())
                    {
                        int nbValues = item.NumberOfAttributes();
                        double[] distr = new double[nbValues];
                        for (int i = 0; i < nbValues; i++)
                        {
                            distr[i] = 0.0d;
                        }

                        for (int i = 0; i < tNode.NumOfSons(); i++)
                        {
                            Add(distr, Times(
                                GoalValueDistribution(item, tNode.Son(i)),
                                tNode.Son(i).Weight));

                            Times(distr, 1.0 / tNode.Weight);
                        }

                        return distr;
                    }
                    else
                    {
                        Node nextNode = tNode.MatchingSon(item.ValueOf(testAttrIndex));

                        return GoalValueDistribution(item, nextNode);
                    }
                }
            }

            throw new InvalidOperationException("Open node found while exploring tree.");
        }

        protected Node Root()
        {
            return _anchor.Son();
        }

        private double[] Times(double[] distribution, double weight)
        {
            for (int i = 0; i < distribution.Length; i++)
                distribution[i] *= weight;

            return distribution;
        }

        private double[] Add(double[] d1, double[] d2)
        {
            if (d1.Length != d2.Length)
                throw new InvalidOperationException();

            for (int i = 0; i < d1.Length; i++)
                d1[i] += d2[i];

            return d1;
        }

        #endregion
    }
}
