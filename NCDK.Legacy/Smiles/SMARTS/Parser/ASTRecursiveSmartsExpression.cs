/* Generated By:JJTree: Do not edit this line. ASTRecursiveSmartsExpression.java Version 4.3 */
/* JavaCCOptions:MULTI=true,NODE_USES_PARSER=false,VISITOR=true,TRACK_TOKENS=false,NODE_PREFIX=AST,NODE_EXTENDS=,NODE_FACTORY=,SUPPORT_CLASS_VISIBILITY_PUBLIC=true */
namespace NCDK.Smiles.SMARTS.Parser
{
    [System.Obsolete]
    internal class ASTRecursiveSmartsExpression : SimpleNode
    {
        public ASTRecursiveSmartsExpression(int id)
          : base(id)
        {
        }

        public ASTRecursiveSmartsExpression(SMARTSParser p, int id)
          : base(p, id)
        {
        }

        public override object JjtAccept(ISMARTSParserVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}