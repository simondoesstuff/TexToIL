using System;

namespace LatexProcessing
{
    public class InvalidLatexExpressionException : Exception
    {
        private const string defaultReason = "Unrecognized";

        public readonly int? IndexOfError;
        public readonly int? WidthOfError;

        public InvalidLatexExpressionException(string reason = defaultReason) : base(reason)
        {
        }
        
        public InvalidLatexExpressionException(int indexOfError, string reason = defaultReason) : base(reason)
        {
            this.IndexOfError = indexOfError;
        }
        
        public InvalidLatexExpressionException(int indexOfError, int widthOfError, string reason = defaultReason) : base(reason)
        {
            this.IndexOfError = indexOfError;
            this.WidthOfError = widthOfError;
        }
    }
}