using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// This class implements athe WeightedItemSet. 
    /// </summary>
    public class WeightedItemSet : ItemSet
    {
        private List<double> _weights;

        private double _weightsSum;

        #region Constructor

        public WeightedItemSet(AttributeSet attrSet)
            : base(attrSet)
        {
            _weights = new List<double>();
            _weightsSum = 0.0d;
        }

        /// <summary>
        /// Build a new weighted item set that contains the same items as an ItemSet, 
        /// with weights set to 1.
        /// </summary>
        /// <param name="itemSet"></param>
        public WeightedItemSet(ItemSet itemSet)
            : base(itemSet.AttrSet)
        {
            _weights = new List<double>();
            _weightsSum = 0.0d;

            foreach (Item it in itemSet.Items)
            {
                Add(it, 1.0d);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a weighted item to the set. This item must be compatible with the
        /// set's attribute set.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="weight"></param>
        public void Add(Item item, double weight)
        {
            this._items.Add(item);
            _weights.Add(weight);
            this._weightsSum += weight;
        }

        /// <summary>
        /// Add a weighted item to the set. The weight is set to 1.
        /// </summary>
        /// <param name="item"></param>
        public new void Add(Item item)
        {
            Add(item, 1.0d);
        }

        /// <summary>
        /// Add multiple <Item, weight> pairs to the set.
        /// </summary>
        /// <param name="items"></param>
        public void Add(IDictionary<Item, double> items)
        {
            foreach (KeyValuePair<Item, double> kv in items)
            {
                Add(kv.Key, kv.Value);
            }
        }

        /// <summary>
        /// Return the weight associated to a specific item.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double GetWeight(int index)
        {
            return _weights[index];
        }

        /// <summary>
        /// Return the sum of this set's items wieghts.
        /// </summary>
        /// <returns></returns>
        public override double Size()
        {
            return _weightsSum;
        }

        /// <summary>
        /// This method computes the entropy of the set regarding a given symbolic attribute.
        /// The frequency of each value of this attribute is counted according to the weights.
        /// The value of this attribute must be known for allt e itmes of this set.
        /// </summary>
        /// <param name="attr">the attribute against which to compute the entropy.</param>
        /// <returns>entropy</returns>
        public override double CalEntropy(SymbolicAttribute attr)
        {
            if (!_attributeSet.Contains(attr))
                throw new ArgumentException("Unknown attribute");

            if (_entropy < 0.0d || !_entropyAttribute.Equals(attr))
            {
                double[] freqs = new double[attr.NumOfValues];


                for (int i = 0; i < Items.Count; i++)
                {
                    KnownSymbolicValue sv = Items[i].ValueOf(_attributeSet, attr) as KnownSymbolicValue;
                    freqs[sv.IntValue] += _weights[i];
                }

                _entropy = Biotracker.Signature.DT.Entropy.CalEntropy(freqs);
                _entropyAttribute = attr;
            }

            return this._entropy;
        }

        /// <summary>
        /// Split the set according to a test.
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        public override IEnumerable<ItemSet> Split(Test test)
        {
            WeightedItemSet[] sets = new WeightedItemSet[test.NumOfIssues];

            double[] setSizes = new double[test.NumOfIssues];

            double setSizeSum = 0.0d;
            List<Item> unkonwnItems = new List<Item>();
            List<double> unknownItemsWeights = new List<double>();

            for (int i = 0; i < sets.Length; i++)
                sets[i] = new WeightedItemSet(this._attributeSet);

            for (int i = 0; i < this._items.Count; i++)
            {
                Item it = this._items[i];
                double weight = this._weights[i];
                AttributeValue val = it.ValueOf(this._attributeSet, test.Attribute);

                if (val.IsUnknown())
                {
                    unkonwnItems.Add(it);
                    unknownItemsWeights.Add(weight);
                }
                else
                {
                    int id = test.Perform(val);
                    sets[id].Add(it, weight);
                }
            }

            for (int i = 0; i < sets.Length; i++)
            {
                setSizes[i] = sets[i].Size();
                setSizeSum += setSizes[i];
            }

            for (int i = 0; i < unkonwnItems.Count; i++)
            {
                Item it = unkonwnItems[i];
                double weight = unknownItemsWeights[i];

                for (int j = 0; j < sets.Length; j++)
                    sets[j].Add(it, weight * setSizes[j] / setSizeSum);
            }

            return sets;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testAttr"></param>
        /// <param name="goalAttr"></param>
        /// <returns></returns>
        protected TestScore BestSplitTest(Attribute testAttr, SymbolicAttribute goalAttr)
        {
            ItemSet knownItems = new ItemSet(_attributeSet);
            int nbKnown = 0;

            foreach (Item it in _items)
            {
                if (!it.ValueOf(_attributeSet, testAttr).IsUnknown())
                {
                    knownItems.Add(it);
                    nbKnown++;
                }
            }

            if (nbKnown == 0)
            { //No Information can be gained from this test
                Test test;

                if (testAttr is SymbolicAttribute)
                { //Symblic test
                    test = new SymbolicTest((SymbolicAttribute)testAttr,
                        new KnownSymbolicValue[] { new KnownSymbolicValue(0) });
                }
                else
                { //Numerical test
                    test = new NumericalTest((NumericalAttribute)testAttr, 0.0d);
                }

                return new TestScore(test, 0.0d);
            }
            else
            {
                TestScore knownTestScore = knownItems.BestSplitTest(testAttr, goalAttr);

                return new TestScore(knownTestScore.Test, knownTestScore.Score * (double)nbKnown / Items.Count);
            }
        }

        #endregion
    }
}
