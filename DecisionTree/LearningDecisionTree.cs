using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// This class implementes a learning decision tree. 
    /// A learning decision tree associates to each node a learning set of elements matching this node.
    /// All the nodes of the tree implement the LearningNode interface.
    /// </summary>
    public class LearningDecisionTree : DecisionTree
    {

        /// <summary>
        /// Create an empty learning decision tree.
        /// </summary>
        /// <param name="attrSet"></param>
        /// <param name="goalAttr"></param>
        /// <param name="learnignSet"></param>
        public LearningDecisionTree(AttributeSet attrSet, SymbolicAttribute goalAttr, ItemSet learnignSet)
            : base(attrSet, goalAttr)
        {
            Root().Replace(new LearningOpenNode(0, learnignSet));
        }

        /// <summary>
        /// Returns a DecisionTree object equivalent to this learning decision tree 
        /// (i.e. without learning set).
        /// </summary>
        /// <returns></returns>
        //public DecisionTree GetDecisionTree()
        //{
        //    DecisionTree dt = new DecisionTree(AttributeSet, GoalAttribute);

        //    Queue<Node> treeIterator = new Queue<Node>();
        //    treeIterator.Enqueue(Root());

        //    while (treeIterator.Count > 0)
        //    {
        //        OpenNode oNode = (OpenNode)treeIterator.Dequeue();

        //        Node newNode = 
        //    }

        //    return dt;
        //}

        #region Private Methods

        private Node ConvertNode(Node node)
        {
            if (node is LearningTestNode)
            {
                return this.ConvertToTestNode((LearningTestNode)node);
            }
            else if (node is LearningLeafNode)
            {
                return this.ConvertToLeafNode((LearningLeafNode)node);
            }
            else
                return this.ConvertOpenNode((LearningOpenNode)node);
        }

        private TestNode ConvertToTestNode(LearningTestNode node)
        {
            return new ScoreTestNode(node.Weight, node.Test, node.Score);
        }

        private LeafNode ConvertToLeafNode(LearningLeafNode node)
        {
            LeafNode lNode = new LeafNode(node.Weight);

            lNode.Entropy = node.LearningSet().CalEntropy(GoalAttribute); 
            lNode.GoalValueDistribution = node.GoalValueDistribution;

            return lNode;
        }

        private OpenNode ConvertOpenNode(LearningOpenNode node)
        {
            return new OpenNode(node.Weight);
        }

        

        #endregion

    }
}
