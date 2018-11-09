using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// A simple version of builder of decision trees.
    /// The decision tree aims to guess a (so called) 'gao' attribute to 'test attributes" values.
    /// All the values must be known; the class handles unkown values.
    /// </summary>
    public class SimpleDecisionTreeBuilder
    {

        private double _entropyThreshold = 0.0d;
        public double EntropyThreshold
        {
            get { return _entropyThreshold; }
            set
            {
                if (value >= 0.0d)
                    _entropyThreshold = value;
            }
        }

        private double _scoreThreshold = 0.0d;
        public double ScoreThreshold
        {
            get { return _scoreThreshold; }
            set
            {
                if (value >= 0.0d)
                    _scoreThreshold = value;
            }
        }

        private LearningDecisionTree _tree;
        private ItemSet _learningSet;

        private SymbolicAttribute _goalAttribute;
        private AttributeSet _testAttributeSet;

        public SimpleDecisionTreeBuilder(ItemSet learningItemSet, AttributeSet testAttributeSet, SymbolicAttribute goalAttribute)
        {
            System.Console.WriteLine("Inside the tree builder!!!!!!!!!!");
            if (learningItemSet == null || learningItemSet.NumOfItems() == 0)
                throw new ArgumentNullException();

            this._learningSet = learningItemSet;
            this._testAttributeSet = testAttributeSet;
            this._goalAttribute = goalAttribute;

            LearningDecisionTree tree =
                new LearningDecisionTree(learningItemSet.AttrSet, goalAttribute, learningItemSet);

            this._tree = tree;
        }

        public LearningDecisionTree Build() /// build the learning tree
        {
            while (_tree.HasOpenNode())
                Expand();

            return _tree;
        }

        public void Expand()
        {
            LearningOpenNode node = _tree.OpenNode() as LearningOpenNode;
            if (node == null)
                throw new Exception("No open node left");

            ItemSet set = node.LearningSet();
            double entropy = set.CalEntropy(_goalAttribute);

            if (entropy <= _entropyThreshold || _testAttributeSet.Size() == 0)
            {
                MakeLeafNode(node);
            }
            else 
            {
                TestScore testScore = set.BestSplitTest(_testAttributeSet, _goalAttribute);

                if (testScore.Score * set.Size() <= _scoreThreshold)
                { //forward pruning: test does not provide enough information
                    MakeLeafNode(node);
                }
                else
                {
                    MakeTestNode(node, testScore.Test, testScore.Score * set.Size());
                }
            }

        }

        private void MakeTestNode(LearningOpenNode openNode, Test test, double score)
        {
            double nodeWeight = openNode.LearningSet().Size();

            LearningTestNode testNode = new LearningTestNode(nodeWeight, test, score, openNode.LearningSet());

            openNode.Replace(testNode);

            ItemSet[] subSets = openNode.LearningSet().Split(test).ToArray();

            for (int i = 0; i < test.NumOfIssues; i++)
            {
                LearningOpenNode node = new LearningOpenNode(subSets[i].Size(), subSets[i]);
                testNode.Son(i).Replace(node);
            }
        }

        /// <summary>
        /// Turns an open node to a leaf.
        /// </summary>
        /// <param name="node">The open node to transform into a leaf node.</param>
        private void MakeLeafNode(LearningOpenNode openNode)
        {
            double nodeWeight = openNode.LearningSet().Size();

            LearningLeafNode leafNode =
                new LearningLeafNode(nodeWeight, openNode.LearningSet());

            openNode.Replace(leafNode);
        }
    }
}
