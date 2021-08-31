using System;

namespace LatexProcessing
{
    public class InvalidLatexExpressionException : Exception
    {
        private const string defaultReason = "Unrecognized Symbol";

        public readonly int? IndexOfError;
        public readonly int? WidthOfError;

        public InvalidLatexExpressionException(string reason = defaultReason) : base(reason)
        {
        }

        public InvalidLatexExpressionException(IMathToken problemToken, string reason = defaultReason)
        {
            IndexOfError = problemToken.ExpressionPosition;
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

        public override string ToString()
        {
            string s = "";

            if (IndexOfError != null)
            {
                string location;

                if (WidthOfError != null)
                    location = $"{IndexOfError}..{IndexOfError + WidthOfError}";
                else location = IndexOfError.ToString()!; 

                s = $"Invalid LaTeX expression at [{location}], ";
            }

            return s + '"' + base.Message + '"';
        }
    }
}