/* Generated By:JJTree: Do not edit this line. ASTAtom.java Version 4.3 */
/* JavaCCOptions:MULTI=true,NODE_USES_PARSER=false,VISITOR=true,TRACK_TOKENS=false,NODE_PREFIX=AST,NODE_EXTENDS=,NODE_FACTORY=,SUPPORT_CLASS_VISIBILITY_PUBLIC=true */
namespace NCDK.Smiles.SMARTS.Parser
{
    public
    class ASTAtom : SimpleNode
    {
        public ASTAtom(int id)
          : base(id)
        {
        }

        public ASTAtom(SMARTSParser p, int id)
          : base(p, id)
        {
        }

        /// <summary>Accept the visitor. </summary>
        public override object JJTAccept(SMARTSParserVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
