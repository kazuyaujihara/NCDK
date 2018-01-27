/* Generated By:JJTree: Do not edit this line. ASTPeriodicGroupNumber.java Version 4.3 */
/* JavaCCOptions:MULTI=true,NODE_USES_PARSER=false,VISITOR=true,TRACK_TOKENS=false,NODE_PREFIX=AST,NODE_EXTENDS=,NODE_FACTORY=,SUPPORT_CLASS_VISIBILITY_PUBLIC=true */
namespace NCDK.Smiles.SMARTS.Parser
{
    public
    class ASTPeriodicGroupNumber : SimpleNode
    {
        /// <summary>
        /// The periodic table group number for this element.
        /// </summary>
        public int GroupNumber { get; set; }

        public ASTPeriodicGroupNumber(int id) : base(id)
        { }

        public ASTPeriodicGroupNumber(SMARTSParser p, int id)
          : base(p, id)
        {
        }


        /// <summary>Accept the visitor. </summary>
        public override object JjtAccept(ISMARTSParserVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
