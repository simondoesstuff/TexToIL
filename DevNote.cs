using System;
using System.Diagnostics;

namespace LatexProcessing
{
    [Conditional("DEBUG")]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class DevNote : Attribute
    {
        public string Note;
        
        public DevNote(string note)
        {
            Note = note;
        }
    }
}