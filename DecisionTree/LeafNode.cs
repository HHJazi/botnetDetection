using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// A leaf node is a node with no sons.
    /// </summary>
    public class LeafNode : Node
    {
        /// <summary>
        /// The distribution of the goal values.
        /// </summary>
        private double[] _goalValueDistribution;
        public double[] GoalValueDistribution
        {
            get
            {
                return GetGoalValueDistribution();
            }
            set
            {
                _goalValueDistribution = value;
            }
        }

        /// <summary>
        /// Entropy of this node.
        /// </summary>
        [XmlIgnore]
        public double Entropy { get; set; }

        virtual public double[] GetGoalValueDistribution()
        {
            return _goalValueDistribution;
        }

        /// <summary>
        ///  Get the most likely goal value of this node given its goal values distribution
        ///  (i.e. the value corresponding to the maximum value of the goal value distribution).
        /// </summary>
        public KnownSymbolicValue GoalValue
        {
            get
            {
                if (_goalValueDistribution == null)
                    throw new InvalidOperationException("Goal value distribution unknown");

                int mostFrequent = -1;
                double mostFrequentFrequency = -1.0d;

                for (int i = 0; i < _goalValueDistribution.Length; i++)
                {
                    if (_goalValueDistribution[i] > mostFrequentFrequency)
                    {
                        mostFrequent = i;
                        mostFrequentFrequency = _goalValueDistribution[i];
                    }
                }

                return new KnownSymbolicValue(mostFrequent);
            }
        }

        public LeafNode()
            : base()
        {

        }

        public LeafNode(double weight)
            : base(weight)
        {
            Entropy = -1;
            _goalValueDistribution = null;
        }

        public override bool HasOpenNode()
        {
            return false;
        }

        public override void UpdateHasOpenNode()
        {
            ;
        }

        public override int NumOfSons()
        {
            return 0;
        }

        protected override void ReplaceSon(Node oldRoot, Node newSon)
        {
            throw new InvalidOperationException("Leaf node doesn't have sons");
        }

        public override Node Son(int id)
        {
            throw new InvalidOperationException("Leaf node doesn't have sons.");
        }
    }
}
