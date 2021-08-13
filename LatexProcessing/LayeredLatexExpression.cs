using System.Collections.Generic;
using System.Linq.Expressions;

namespace LatexProcessing
{
    public class LayeredLatexExpression
    {
        private Dictionary<char, ParameterExpression> _linqParameters = new Dictionary<char, ParameterExpression>();
        public List<string> _latexStringLayers = new List<string>();

        // external update layer
        public string this[int i]
        {
            set
            {
                _latexStringLayers[i] = value;
                _UpdateLayer(i);
            }
            
            get => _latexStringLayers[i];
        }

        public void _UpdateLayer(int iLayer)
        {
            
        }
    }
}