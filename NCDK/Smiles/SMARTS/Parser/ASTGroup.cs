/* Generated By:JJTree: Do not edit this line. ASTGroup.java Version 4.3 */
/* JavaCCOptions:MULTI=true,NODE_USES_PARSER=false,VISITOR=true,TRACK_TOKENS=false,NODE_PREFIX=AST,NODE_EXTENDS=,NODE_FACTORY=,SUPPORT_CLASS_VISIBILITY_PUBLIC=true */
using NCDK.Isomorphisms.Matchers.SMARTS;

namespace NCDK.Smiles.SMARTS.Parser
{
    public class ASTGroup : SimpleNode
    {
        internal const int ROLE_REACTANT = ReactionRoleQueryAtom.ROLE_REACTANT;
        internal const int ROLE_AGENT = ReactionRoleQueryAtom.ROLE_AGENT;
        internal const int ROLE_PRODUCT = ReactionRoleQueryAtom.ROLE_PRODUCT;
        internal const int ROLE_ANY = ReactionRoleQueryAtom.ROLE_ANY;

        private int role = ROLE_ANY;

        public ASTGroup(int id)
          : base(id)
        {
        }

        public ASTGroup(SMARTSParser p, int id)
          : base(p, id)
        {
        }

        public void SetRole(int role)
        {
            this.role = role;
        }

        public int GetRole()
        {
            return this.role;
        }

        /// <summary>Accept the visitor. </summary>
        public override object JjtAccept(ISMARTSParserVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
