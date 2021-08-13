using System.Collections;
using System.Collections.Generic;

namespace LatexProcessing
{
    public class LexedLatexExp : IEnumerable<IMathToken>
    {
        private IEnumerable<IMathToken> _tokens;
        public char? VariableAssignment;

        public LexedLatexExp(IEnumerable<IMathToken> tokens)
        {
            _tokens = tokens;
        }
        
        public LexedLatexExp(IEnumerable<IMathToken> tokens, char? variableassignment)
        {
            _tokens = tokens;
            VariableAssignment = variableassignment;
        }
        
        public IEnumerator<IMathToken> GetEnumerator() => _tokens.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => _tokens.GetEnumerator();
    }
}