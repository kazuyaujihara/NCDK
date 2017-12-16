/* Generated By:JJTree: Do not edit this line. ASTRingMembership.java Version 4.3 */
/* JavaCCOptions:MULTI=true,NODE_USES_PARSER=false,VISITOR=true,TRACK_TOKENS=false,NODE_PREFIX=AST,NODE_EXTENDS=,NODE_FACTORY=,SUPPORT_CLASS_VISIBILITY_PUBLIC=true */
namespace NCDK.Smiles.SMARTS.Parser
{
    public
    class ASTRingMembership : SimpleNode
    {
        public ASTRingMembership(int id)
          : base(id)
        {
        }

        public ASTRingMembership(SMARTSParser p, int id)
          : base(p, id)
        {
        }

        /// <summary>
        /// The number of SSSR rings this atom is in.
        /// </summary>
        public int NumOfMembership { get; set; }

        /// <summary>Accept the visitor. </summary>
        public override object JjtAccept(SMARTSParserVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
