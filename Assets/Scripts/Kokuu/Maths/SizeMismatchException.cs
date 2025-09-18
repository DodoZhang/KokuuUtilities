using System;

namespace Kokuu.Maths
{
    public class SizeMismatchException : Exception
    {
        public SizeMismatchException(string expectation) :
            base($"Size Mismatched, Expect {expectation}") { }
    }
}