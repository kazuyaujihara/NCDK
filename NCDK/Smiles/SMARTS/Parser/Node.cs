/* Copyright (C) 2004-2007  The Chemistry Development Kit (CDK) project
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or(at your option) any later version.

*
* This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA.
 * (or see http://www.gnu.org/copyleft/lesser.html)
 */

namespace NCDK.Smiles.SMARTS.Parser
{
    // @author Dazhi Jiao
    // @cdk.created 2007-04-24
    // @cdk.module smarts
    // @cdk.githash
    // @cdk.keyword SMARTS AST
    /// <summary>
    /// All AST nodes must implement this interface.  It provides basic
    /// machinery for constructing the parent and child relationships
    /// between nodes.
    /// </summary>
    // Automatically generated by JJTree
    public interface Node
    {
        /// <summary> This method is called after the node has been made the current node.  It indicates that child nodes can now be added to it.</summary>
        void JjtOpen();

        /// <summary> This method is called after all the child nodes have been added.</summary>
        void JjtClose();

        /// <summary> This pair of methods are used to inform the node of its parent.</summary>
        void JjtSetParent(Node n);

        Node JjtGetParent();

        /// <summary> This method tells the node to add its argument to the node's list of children. </summary>
        void JjtAddChild(Node n, int i);

        /// <summary> This method returns a child node.  The children are numbered from zero, left to right.</summary>
        Node JjtGetChild(int i);

        /// <summary>Return the number of children the node has.</summary>
        int JjtGetNumChildren();

        /// <summary>Accept the visitor. </summary>
        object JjtAccept(ISMARTSParserVisitor visitor, object data);

        /// <summary>
        /// Removes a child from this node
        /// </summary>
        /// <param name="i"></param>
        void JjtRemoveChild(int i);
    }
}
