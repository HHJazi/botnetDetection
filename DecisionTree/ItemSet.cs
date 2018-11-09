using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// This class holds an ordered set of items. An item is defined as a learning/test set element.
    /// 
    /// </summary>
    public class ItemSet
    {
        /// <summary>
        /// The item's attributes
        /// </summary>
        protected AttributeSet _attributeSet;
        public AttributeSet AttrSet
        {
            get { return _attributeSet; }
            set { _attributeSet = value; }
        }

        protected List<Item> _items;
        public List<Item> Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Entropy. (-1 for unknonw)
        /// </summary>
        protected double _entropy = -1;
        
        /// <summary>
        /// The attribute used to compute the entropy.
        /// </summary>
        protected Attribute _entropyAttribute;

        #region Constructors

        /// <summary>
        /// Creates a empty ItemSet object.
        /// </summary>
        /// <param name="attrSet"></param>
        public ItemSet(AttributeSet attrSet)
        {
            if (attrSet == null)
                throw new ArgumentNullException();

            this._items = new List<Item>();

            this._attributeSet = attrSet;
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Add an item to the set. This item must be compatible with this set's 
        /// attribute set.
        /// </summary>
        /// <param name="item"></param>
        public void Add(Item item)
        {
            if (item == null || item.NumberOfAttributes() != this._attributeSet.Size())
                throw new ArgumentException("Incompatible item.");

            _items.Add(item);
            _entropy = -1;
        }

        /// <summary>
        /// Add a range of items.
        /// </summary>
        /// <param name="items"></param>
        public void Add(IEnumerable<Item> items)
        {
            foreach (Item it in items)
            {
                Add(it);
            }
        }

        /// <summary>
        /// Removes an item from the set.
        /// </summary>
        /// <param name="index">item index.</param>
        public void Remove(int index)
        {
            this._items.RemoveAt(index);
            this._entropy = -1;
        }

        /// <summary>
        /// Returns the sum o fthis set's items weights.
        /// </summary>
        /// <returns></returns>
        public int NumOfItems()
        {
            return _items.Count;
        }

        /// <summary>
        /// Returns the set's number of items.
        /// </summary>
        /// <returns></returns>
        public virtual double Size()
        {
            return (double)(_items.Count);
        }

        /// <summary>
        /// Returns an item's attribute value.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        public AttributeValue GetValue(int index, Attribute attr)
        {
            return this._items[index].ValueOf(this._attributeSet, attr);
        }

        /// <summary>
        /// Finds the test on one attribute performing the best split (bringing the most information)
        /// for finding the value of a 'goal' attribute
        /// </summary>
        /// <param name="candidateAttributes">The set of attributes defining which attributes can be tested</param>
        /// <param name="goalAttribute">the attribute guess using the test</param>
        /// <returns></returns>
        public TestScore BestSplitTest(AttributeSet candidateAttributes, SymbolicAttribute goalAttribute)
        {
            return BestSplitTests(candidateAttributes, goalAttribute).Max<TestScore>();
        }


        /// <summary>
        /// Finds the test on each attribute performing the best split for finding the value of a 'goal' 
        /// attribute.
        /// </summary>
        /// <param name="candidateAttributes"></param>
        /// <param name="goalAttribute"></param>
        /// <returns></returns>
        public IEnumerable<TestScore> BestSplitTests(AttributeSet candidateAttributes, SymbolicAttribute goalAttribute)
        {
            if (candidateAttributes == null || goalAttribute == null || candidateAttributes.Size() == 0)
                throw new ArgumentNullException();

            List<TestScore> bestScores = new List<TestScore>();

            List<Attribute> attributes = candidateAttributes.GetAttributes().ToList();

            foreach (Attribute attr in attributes)
            {
                bestScores.Add(BestSplitTest(attr, goalAttribute));
            }

            return bestScores;
        }

        /// <summary>
        /// Splits the set according to a test.
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        public virtual IEnumerable<ItemSet> Split(Test test)
        {
            ItemSet[] sets = new ItemSet[test.NumOfIssues];

            for (int i = 0; i < sets.Length; i++)
            {
                sets[i] = new ItemSet(this._attributeSet);
            }

            foreach (Item it in this._items)
            {
                int id = test.Perform(it.ValueOf(this._attributeSet, test.Attribute));

                sets[id].Add(it);
            }

            return sets;
        }

        /// <summary>
        /// Computes the entropy of the set regarding a given symbolic attribute.
        /// </summary>
        /// <param name="attr">the attribute against which to compute the entropy.</param>
        /// <returns></returns>
        public virtual double CalEntropy(SymbolicAttribute attr)
        {
            if (!_attributeSet.Contains(attr))
            {
                throw new ArgumentException("Unknown attribute");
            }

            if (this._entropy < 0.0d || !_entropyAttribute.Equals(attr))
            {
                double[] frequencies = new double[attr.NumOfValues];

                for (int i = 0; i < _items.Count; i++)
                {
                    KnownSymbolicValue sv = (KnownSymbolicValue)(_items[i].ValueOf(_attributeSet.IndexOf(attr)));
                    frequencies[sv.IntValue]++;
                }

                this._entropy = Entropy.CalEntropy(frequencies);
                _entropyAttribute = attr;
            }

            return _entropy;
        }

        public TestScore BestSplitTest(Attribute testAttribute, SymbolicAttribute goalAttribute)
        {
            if (testAttribute is SymbolicAttribute)
                return BestSplitTest((SymbolicAttribute)testAttribute, goalAttribute);
            else if (testAttribute is NumericalAttribute)
                return BestSplitTest((NumericalAttribute)testAttribute, goalAttribute);
            else
                throw new ArgumentException("Unknow attribute type.");
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Item it in this._items)
            {
                sb.AppendLine(it.ToString());
            }

            return sb.ToString();
        }
 
        #endregion

        #region Private Methods

  

        /// <summary>
        /// Finds the best splitting test involving a Symbolic attribute.
        /// </summary>
        /// <param name="testAttr">Symbolic attribute for test</param>
        /// <param name="goalAttr"></param>
        /// <returns></returns>
        protected TestScore BestSplitTest(SymbolicAttribute testAttr, SymbolicAttribute goalAttr)
        {
            int testNbVal = testAttr.NumOfValues;
            int testIndex = _attributeSet.IndexOf(testAttr);
            int goalNbVal = goalAttr.NumOfValues;
            int goalIndex = _attributeSet.IndexOf(goalAttr);

            //freqMatch[tvi][gvi] is the number of items that has a value equal to tvi for their 'test'
            //attribute and value equal to 'gvi' for their 'goal' attribute. 
            //freqMatchSum[tvi] is the sum of the frequencyMatch[tvi][gvi] elements (for all gvi).
            double[][] freqMatch = new double[testNbVal][];
            for(int i=0; i<testNbVal;i++)
            {
                freqMatch[i] = new double[goalNbVal];
            }
            double[] freqMatchSum = new double[testNbVal];

            //Identically for the items that do not have tvi as a test attribute value.
            double[][] freqNoMatch = new double[testNbVal][];
            for(int i=0; i<testNbVal; i++)
            {
                freqNoMatch[i] = new double[goalNbVal];
            }

            double[] freqNoMatchSum = new double[testNbVal];

            for (int i = 0; i < _items.Count; i++)
            {
                int testVal = ((KnownSymbolicValue)(_items[i].ValueOf(testIndex))).IntValue;
                int goalVal = ((KnownSymbolicValue)(_items[i].ValueOf(goalIndex))).IntValue;

                for (int tvi = 0; tvi < testNbVal; tvi++)
                {
                    if (testVal == tvi)
                    {
                        freqMatch[tvi][goalVal]++;
                        freqMatchSum[tvi]++;
                    }
                    else
                    {
                        freqNoMatch[tvi][goalVal]++;
                        freqNoMatchSum[tvi]++;
                    }
                }
            }

            double bestScore = -1.0d;
            int bestValue = -1;

            for (int tvi = 0; tvi < testNbVal; tvi++)
            {
                double score = CalEntropy(goalAttr)
                   - ((freqMatchSum[tvi] / _items.Count) * Entropy.CalEntropy(freqMatch[tvi]))
                   - ((freqNoMatchSum[tvi] / _items.Count) * Entropy.CalEntropy(freqNoMatch[tvi]));

                if (score > bestScore)
                {
                    bestScore = score;
                    bestValue = tvi;
                }
            }

            //Group the attribute values one by one 
            List<int> remainTestValueIndexes = new List<int>();
            for (int i = 0; i < testNbVal; i++)
                remainTestValueIndexes.Add(i);

            double[] remainingFreqMatch = new double[goalNbVal];
            double[] remainingFreqNoMatch = new double[goalNbVal];

            for (int gvi = 0; gvi < goalNbVal; gvi++)
            {
                remainingFreqNoMatch[gvi] = freqMatch[0][gvi] + freqNoMatch[0][gvi];
            }

            double remainingFreqMatchSum = 0.0d;
            double remainingFreqNoMatchSum = (double)(_items.Count);

            List<int> orderedValueIndex = new List<int>();
            List<double> orderedScores = new List<double>();

            orderedValueIndex.Add(bestValue);
            orderedScores.Add(bestScore);

            //Remove values until only one is left
            while (remainTestValueIndexes.Count >= 2)
            {
                //Update remaining Frequency.. arrays according to the last test attribute value removed.
                remainTestValueIndexes.Remove(bestValue);

                for (int gvi = 0; gvi < goalNbVal; gvi++)
                {
                    remainingFreqMatch[gvi] += freqMatch[bestValue][gvi];
                    remainingFreqNoMatch[gvi] -= freqMatch[bestValue][gvi];
                }

                remainingFreqMatchSum += freqMatchSum[bestValue];
                remainingFreqNoMatchSum -= freqMatchSum[bestValue];

                bestScore = -1.0d;

                //Find the next best test attribute value
                for (int i = 0; i < remainTestValueIndexes.Count; i++)
                {
                    int tvi = remainTestValueIndexes[i];

                    double[] thisFreqMatch = new double[goalNbVal];
                    double[] thisFreqNoMatch = new double[goalNbVal];
                    double thisFreqMatchSum = 0.0d;
                    double thisFreqNoMatchSum = 0.0d;

                    for (int gvi = 0; gvi < goalNbVal; gvi++)
                    {
                        thisFreqMatch[gvi] = freqMatch[tvi][gvi] + remainingFreqMatch[gvi];
                        thisFreqNoMatch[gvi] = remainingFreqNoMatch[gvi] - freqMatch[tvi][gvi];
                    }
                    thisFreqMatchSum = freqMatchSum[tvi] + remainingFreqMatchSum;
                    thisFreqNoMatchSum = remainingFreqNoMatchSum - freqMatchSum[tvi];

                    double score = CalEntropy(goalAttr)
                        - ((thisFreqMatchSum / _items.Count) * Entropy.CalEntropy(thisFreqMatch))
                        - ((thisFreqNoMatchSum / _items.Count) * Entropy.CalEntropy(thisFreqNoMatch));

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestValue = tvi;
                    }
                }
            }

            orderedScores.Add(bestScore);
            orderedValueIndex.Add(bestValue);

            bestScore = -1.0d;
            int bestIndex = 0;
            for (int i = 0; i < orderedScores.Count; i++)
            {
                double score = orderedScores[i];

                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            KnownSymbolicValue[] testValueIndexes = new KnownSymbolicValue[bestIndex + 1];
            for (int i = 0; i <= bestIndex; i++)
            {
                int val = orderedValueIndex[i];
                testValueIndexes[i] = new KnownSymbolicValue(val);
            }

            return new TestScore(new SymbolicTest(testAttr, testValueIndexes), bestScore);
        }

        /// <summary>
        /// Finds the best splitting test involving a numerical attribute
        /// </summary>
        /// <param name="testAttr"></param>
        /// <param name="goalAttr"></param>
        /// <returns></returns>
        private TestScore BestSplitTest(NumericalAttribute testAttr, SymbolicAttribute goalAttr)
        {
            int testIndex = _attributeSet.IndexOf(testAttr);
            int goalNbVal = goalAttr.NumOfValues;
            int goalIndex = _attributeSet.IndexOf(goalAttr);

            //frequencyLower (frequencyHigher) counts the number of items lower 
            //(higher) than the threshold for each goal value.  In the beginning,
            //frequencyLower is zeroed because the threshold is chosen small.  
            double[] freqLower = new double[goalNbVal];
            double[] freqHigher = new double[goalNbVal];

            for (int gvi = 0; gvi < goalNbVal; gvi++)
            {
                SymbolicTest valTest = new SymbolicTest(goalAttr,
                    new KnownSymbolicValue[] { new KnownSymbolicValue(gvi) });

                freqHigher[gvi] = Split(valTest).ElementAt(1).Size();
            }

            //Those two variables hold sum of the elements of the corresponding array.
            double freqLowerSum = 0.0d;
            double freqHigherSum = (double)_items.Count;

            List<TestGoalValue> tgv = new List<TestGoalValue>();
            for (int i = 0; i < _items.Count; i++)
            {
                double testVal = ((KnownNumericalValue)(this._items[i].ValueOf(testIndex))).Value;
                int goalVal = ((KnownSymbolicValue)(this._items[i].ValueOf(goalIndex))).IntValue;
                tgv.Add(new TestGoalValue(testVal, goalVal));
            }

            tgv.Sort();

            int goalValue, goalValueNew = tgv[0].GoalValue;
            double testValue, testValueNew = tgv[0].TestValue;

            double bestScore = 0.0d;
            double bestThreshold = testValueNew;

            for (int i = 1; i < _items.Count; i++)
            {
                testValue = testValueNew;
                goalValue = goalValueNew;
                testValueNew = tgv[i].TestValue;
                goalValueNew = tgv[i].GoalValue;

                freqLower[goalValue]++;
                freqLowerSum++;
                freqHigher[goalValue]--;
                freqHigherSum--;

                if (testValue != testValueNew)
                {
                    double score = CalEntropy(goalAttr)
                        - (freqLowerSum / _items.Count) * Entropy.CalEntropy(freqLower)
                        - (freqHigherSum / _items.Count) * Entropy.CalEntropy(freqHigher);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestThreshold = (testValue + testValueNew) / 2.0d;
                    }
                }
            }

            return new TestScore(new NumericalTest(testAttr, bestThreshold), bestScore);
        }

        #endregion

        #region Internal class TestGoalValue

        class TestGoalValue : IComparable
        {
            public double TestValue;
            public int GoalValue;

            public TestGoalValue(double testVal, int goalVal)
            {
                this.TestValue = testVal;
                this.GoalValue = goalVal;
            }

            public int CompareTo(object obj)
            {
                TestGoalValue to = obj as TestGoalValue;

                if (TestValue < to.TestValue)
                    return -1;
                else if (TestValue == to.TestValue)
                    return 0;
                else
                    return 1;
            }
        }

        #endregion
    }
}
