namespace LatexProcessing
{
    public class OpToken : IMathToken
    {
        public MathTokenTypes TokenType => MathTokenTypes.Op;
        public int? ExpressionPosition { get; }
        public MathOperatorTokenTypes OpType { get; }

        public OpToken(MathOperatorTokenTypes opType, int? expressionPosition)
        {
            OpType = opType;
            ExpressionPosition = expressionPosition;
        }
        
        /// <summary>
        /// Ignores ExpressionPosition property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (base.Equals(obj)) return true;
            
            var other = obj as OpToken;
        
            if (other == null) return false;
        
            if (this.OpType != other.OpType) return false;

            return true;
        }
        
        public override string ToString()
        {
            return $"(Op, {OpType})";
        }
    }
}