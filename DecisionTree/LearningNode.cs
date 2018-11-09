using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// A learning node is a node with an associated learning set. A learning set is a set of items.
    /// Each node implementing this interface should ensure that if it is replaeced by a node N, n also implements 
    /// this interface.
    /// </summary>
    public interface LearningNode
    {
        /// <summary>
        /// Returns the elarning set associated to this node.
        /// </summary>
        /// <returns></returns>
        ItemSet LearningSet();
    }
}
