/* Generated By:JJTree: Do not edit this line. ASTReaction.java Version 4.3 */
/* JavaCCOptions:MULTI=true,NODE_USES_PARSER=false,VISITOR=true,TRACK_TOKENS=false,NODE_PREFIX=AST,NODE_EXTENDS=,NODE_FACTORY=,SUPPORT_CLASS_VISIBILITY_PUBLIC=true */
namespace NCDK.Smiles.SMARTS.Parser
{

    public
    class ASTReaction : SimpleNode
    {
        public ASTReaction(int id)
          : base(id)
        {
        }

        public ASTReaction(SMARTSParser p, int id)
          : base(p, id)
        {
        }


        /// <summary>Accept the visitor. </summary>
        public override object JjtAccept(SMARTSParserVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
