/* Generated By:JJTree: Do not edit this line. ASTTotalHCount.java Version 4.3 */
/* JavaCCOptions:MULTI=true,NODE_USES_PARSER=false,VISITOR=true,TRACK_TOKENS=false,NODE_PREFIX=AST,NODE_EXTENDS=,NODE_FACTORY=,SUPPORT_CLASS_VISIBILITY_PUBLIC=true */
namespace NCDK.Smiles.SMARTS.Parser
{

    public
    class ASTTotalHCount : SimpleNode
    {
        public ASTTotalHCount(int id)
          : base(id)
        {
        }

        public ASTTotalHCount(SMARTSParser p, int id)
          : base(p, id)
        {
        }

        public int Count { get; set; }

        /// <summary>Accept the visitor. </summary>
        public override object JjtAccept(ISMARTSParserVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
