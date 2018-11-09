using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// This class implements the LeafNode with LearningNode interface.
    /// </summary>
    public class LearningLeafNode : LeafNode, LearningNode
    {

        private ItemSet _learningSet;

        public LearningLeafNode()
            :base()
        { 
        
        }

        public LearningLeafNode(double weight, ItemSet learningSet)
            : base(weight) 
        {
            this._learningSet = learningSet;
        }

        public ItemSet LearningSet()
        {
            return _learningSet;
        }

        public override void Replace(Node node)
        {
            if (!(node is LearningNode))
                throw new ArgumentException();

            base.Replace(node);
        }

        /// <summary>
        /// Returns the distribution of goal values. This distributionis represented by an array,
        /// its i-th element is proportional to the weight of the i-th goal value.
        /// The Sum of the elements of this array is equal to 1.
        /// </summary>
        /// <returns>An array describing the goal value distribution associated to this node.</returns>
        public override double[] GetGoalValueDistribution()
        {
            WeightedItemSet itemSet;
            DecisionTree dt = base.Tree();

            if (dt == null || _learningSet == null)
                return null;

            if (!(_learningSet is WeightedItemSet))
            {
                itemSet = new WeightedItemSet(_learningSet);
            }
            else
            {
                itemSet = (WeightedItemSet)_learningSet;
            }

            SymbolicAttribute goalAttr = dt.GoalAttribute;

            if (goalAttr == null)
                return null;

            //Find the most frequent goal value in the items of the learning set
            double[] frequencies = new double[goalAttr.NumOfValues];

            for (int i = 0; i < itemSet.NumOfItems(); i++)
            { 
                int id = ((KnownSymbolicValue)(itemSet.Items[i].ValueOf(itemSet.AttrSet.IndexOf(goalAttr)))).IntValue;
                frequencies[id] += itemSet.GetWeight(i);
            }

            for (int i = 0; i < frequencies.Length; i++)
                frequencies[i] /= itemSet.Size();

            return frequencies;
        }

        /// <summary>
        /// Returns the symbolic value associated to this node. This value is 
        /// the goal (guessed) symbolic attribute value associated to this leaf.
        /// This value is computed thanks to the learning set asssociated to this node.
        /// </summary>
        /// <returns>The goal attribute value associated to this node, or -1 if this value has not been fixed.</returns>
        public KnownSymbolicValue GetGoalValue()
        {
            double[] goalValueDistribution = GetGoalValueDistribution();

            int mostFrequent = -1;
            double mostFrequentFrequency = -1.0;

            for (int gav = 0; gav < goalValueDistribution.Length; gav++)
            {
                if (goalValueDistribution[gav] > mostFrequentFrequency)
                {
                    mostFrequent = gav;
                    mostFrequentFrequency = goalValueDistribution[gav];
                }
            }

            return new KnownSymbolicValue(mostFrequent);
        }
    }
}
