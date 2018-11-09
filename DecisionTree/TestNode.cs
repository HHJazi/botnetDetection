using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// This class implemnts a testing node. All the nodes between the root and the leaves are test node.
    /// Each test node can perform a test, i.e. can split a test in several classes according to the test issue.
    /// A test node has as many leaves as the number of issues of the test.
    /// </summary>
    public class TestNode : Node
    {
        private Node[] _sons;
        private bool _hasOpenNode;

        #region Properties
        private Test _test;
        public Test Test 
        { 
            get 
            {
                return _test;
            }
            set 
            {
                _test = value;
            }
        }

        #endregion

        #region Constructor

        public TestNode()
            : base()
        {
        }

        public TestNode(double weight, Test test)
            : base(weight)
        {
            if (test == null)
                throw new ArgumentNullException("No Test defined.");
            else
                this._test = test;

            this._sons = new Node[test.NumOfIssues];
            for (int i = 0; i < this._sons.Length; i++)
            {
                _sons[i] = new OpenNode(0.0);
                _sons[i].Father = this;
            }

            this._hasOpenNode = true;
        }

        public TestNode(double weight, Test test, IEnumerable<Node> sons)
            :base(weight)
        {
            this._test = test;

            this._hasOpenNode = false;

            this._sons = sons.ToArray();
            foreach (Node s in this._sons)
            {
                if (s.HasOpenNode())
                {
                    this._hasOpenNode = true;
                    break;
                }
            }
        }

        #endregion


        protected override void ReplaceSon(Node oldRoot, Node newSon)
        {
            if (newSon == null)
                throw new ArgumentNullException("New son cannot be null.");

            for (int i = 0; i < _sons.Length; i++)
            {
                if (_sons[i] == oldRoot)
                {
                    _sons[i] = newSon;
                    UpdateHasOpenNode();
                    return;
                }
            }

            throw new ArgumentException("The first argument is not a son.");
        }

        #region Public Methods

        /// <summary>
        /// Returns the son matching a value given this node's test. The argument is tested
        /// adn the matching son is returned. If the test is numerical, the argument must be 
        /// Double object, else it must be an Integer.
        /// </summary>
        /// <param name="val">the value to test</param>
        /// <returns>The node matching the test issue.</returns>
        public Node MatchingSon(AttributeValue val)
        {
            return this._sons[_test.Perform(val)];
        }

        public override bool HasOpenNode()
        {
            return _hasOpenNode;
        }

        public override int NumOfSons()
        {
            return this._test.NumOfIssues;
        }

        public override Node Son(int id)
        {
            return this._sons[id];
        }

        public override void UpdateHasOpenNode()
        {
            bool hasOpenNode = false;

            for (int i = 0; i < NumOfSons(); i++)
            {
                if (_sons[i].HasOpenNode())
                {
                    hasOpenNode = true;
                    break;
                }
            }

            if (this._hasOpenNode != hasOpenNode)
            {
                this._hasOpenNode = hasOpenNode;

                if (Father != null)
                    Father.UpdateHasOpenNode();
            }
        }

        #endregion
    }
}
