using System;

namespace LatexProcessing
{
    public class InvalidLatexExpressionException : Exception
    {
        private const string defaultReason = "Unrecognized Symbol";

        public readonly int? IndexOfError;

        public InvalidLatexExpressionException(string reason = defaultReason) : base(reason)
        {
        }

        public InvalidLatexExpressionException(IMathToken problemToken, string reason = defaultReason) : base(reason)
        {
            IndexOfError = problemToken.ExpressionPosition;
        }
        
        public InvalidLatexExpressionException(int indexOfError, string reason = defaultReason) : base(reason)
        {
            this.IndexOfError = indexOfError;
        }

        public override string ToString()
        {
            string s = "";

            if (IndexOfError != null)
            {
                string location = IndexOfError.ToString()!;
                s = $"Invalid LaTeX expression at [{location}], ";
            }

            return s + '"' + base.Message + '"';
        }
    }
}