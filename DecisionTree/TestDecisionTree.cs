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
    /// This class implements a test decision tree.
    /// A test decision tree is generated from a trained learning decision tree.
    /// It replaces all the learning nodes in the learning decision tree. 
    /// </summary>
    public class TestDecisionTree : DecisionTree
    {
        #region Properties

        private Node[] _nodes;
        [XmlArray(ElementName = "Nodes", IsNullable = false)]
        public Node[] Nodes
        {
            get { return _nodes; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                else
                {
                    _nodes = this.ConvertToTestNode(value).ToArray();

                    try
                    {
                        GetTestDecisionTree(_nodes);
                    }
                    catch (Exception ex)
                    {
                        _nodes = null;
                        throw ex;
                    }
                }
            }
        }

        #endregion

        #region Constructors
        public TestDecisionTree()
            : base()
        {
            _nodes = null;
        }

        /// <summary>
        /// Reconstruct a decision tree from 
        /// </summary>
        /// <param name="attrSet"></param>
        /// <param name="goalAttr"></param>
        /// <param name="nodes"></param>
        public TestDecisionTree(AttributeSet attrSet, SymbolicAttribute goalAttr, Node[] nodes)
            : base(attrSet, goalAttr)
        {
            int cur = 0;
            Node curNode;

            Node[] nodeArray = nodes.ToArray();

            if (!(nodeArray[0] is AnchorNode))
                throw new ArgumentException("");

            this._anchor = new AnchorNode(this);

            curNode = nodeArray[++cur];
            this._anchor.Replace(curNode);

            curNode.Father = this._anchor;

            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(curNode);

            do
            {
                curNode = queue.Dequeue();

                int nbSons = curNode.NumOfSons();

                List<Node> sons = new List<Node>();

                for (int i = 0; i < nbSons; i++)
                {
                    Node son = nodeArray[++cur];

                    sons.Add(son);

                    //update the son
                    queue.Enqueue(son);
                }
                
                if (curNode is TestNode)
                {
                    TestNode tn = new TestNode(curNode.Weight, (curNode as TestNode).Test, sons);
                    curNode.Replace(tn);

                    curNode = tn;
                }
                else if (curNode is LeafNode)
                {
                    curNode.Replace((LeafNode)curNode);
                }

                foreach (Node n in sons)
                    n.Father = curNode;
            }
            while (queue.Count > 0);

        }

        public TestDecisionTree(LearningDecisionTree ldt)
        {
            GetTestDecisionTree(ldt);
        }

        #endregion

        #region Public Methods

        public string XmlSerialize()
        {
            try
            {
                string retString = default(string);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (StreamReader sr = new StreamReader(ms))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(TestDecisionTree),
                            new Type[]
                           {
                                    typeof(DecisionTree),
                                    typeof(Node),
                                    typeof(LeafNode),
                                    typeof(OpenNode),
                                    typeof(TestNode),
                                    typeof(ScoreTestNode),
                                    typeof(AnchorNode),
                                    typeof(Test),
                                    typeof(SymbolicTest),
                                    typeof(NumericalTest),
                                    typeof(Attribute),
                                    typeof(SymbolicAttribute),
                                    typeof(IdSymbolicAttribute),
                                    typeof(NumericalAttribute),
                                    typeof(KnownSymbolicValue),
                                    typeof(KnownNumericalValue),
                                    typeof(AttributeSet)
                           });

                        xs.Serialize(ms, this);

                        ms.Seek(0, SeekOrigin.Begin);

                        retString = sr.ReadToEnd();
                    }
                }

                return retString;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static TestDecisionTree XmlDeserialize(string xml)
        {
            TestDecisionTree dt = null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (StreamWriter sr = new StreamWriter(ms))
                    {
                        sr.WriteLine(xml);
                        sr.Flush();

                        ms.Seek(0, SeekOrigin.Begin);

                        XmlSerializer xs = new XmlSerializer(typeof(TestDecisionTree),
                            new Type[]
                           {
                                    typeof(DecisionTree),
                                    typeof(Node),
                                    typeof(LeafNode),
                                    typeof(OpenNode),
                                    typeof(TestNode),
                                    typeof(ScoreTestNode),
                                    typeof(AnchorNode),
                                    typeof(Test),
                                    typeof(SymbolicTest),
                                    typeof(NumericalTest),
                                    typeof(Attribute),
                                    typeof(SymbolicAttribute),
                                    typeof(IdSymbolicAttribute),
                                    typeof(NumericalAttribute),
                                    typeof(KnownSymbolicValue),
                                    typeof(KnownNumericalValue),
                                    typeof(AttributeSet)
                           });

                        dt = xs.Deserialize(ms) as TestDecisionTree;
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dt;
        }

        #endregion

        #region Private Methods

        private IEnumerable<Node> ConvertToTestNode(IEnumerable<Node> nodes)
        {
            List<Node> retNodeList = new List<Node>();

            foreach (Node n in nodes)
            {
                if (n is LeafNode)
                {
                    LeafNode ln = new LeafNode(n.Weight);
                    ln.NbSons = n.NbSons;
                    ln.GoalValueDistribution = (n as LeafNode).GoalValueDistribution;
                    retNodeList.Add(ln);
                }
                else if (n is LearningOpenNode)
                {
                    OpenNode on = new OpenNode(n.Weight);
                    on.NbSons = n.NbSons;
                    retNodeList.Add(on);
                }
                else if (n is LearningTestNode)
                {
                    ScoreTestNode stn = new ScoreTestNode(
                        (n as LearningTestNode).Weight,
                        (n as LearningTestNode).Test,
                        (n as LearningTestNode).Score);
                   
                    retNodeList.Add(stn);
                }
                else
                {
                    retNodeList.Add(n);
                }
            }

            return retNodeList;
        }

        /// <summary>
        /// Construct a Decision tree object with only test nodes. 
        /// This function is useful after a LearningDecision Tree is trained. The 
        /// Test decision tree object is light-weighted and easy for serialization.
        /// </summary>
        /// <param name="ldt">A traininged learning decision tree</param>
        /// <returns></returns>
        private TestDecisionTree GetTestDecisionTree(LearningDecisionTree ldt)
        {
            try
            {
                List<Node> nodeList = this.ConvertToTestNode(ldt.BFIterator()).ToList();

                this._nodes = nodeList.ToArray();

                this.AttributeSet = ldt.AttributeSet;
                this.GoalAttribute = ldt.GoalAttribute;

                return GetTestDecisionTree(nodeList);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private TestDecisionTree GetTestDecisionTree(IEnumerable<Node> nodes)
        {
            try
            {
                TestDecisionTree dt = new TestDecisionTree(
                   this.AttributeSet,
                   this.GoalAttribute,
                   nodes.ToArray());

                this._anchor = dt._anchor;

                return this;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion
    }
}
