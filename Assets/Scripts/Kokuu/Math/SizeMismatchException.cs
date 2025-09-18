using System;

namespace Kokuu
{
    public class SizeMismatchException : Exception
    {
        public SizeMismatchException(string expectation) :
            base($"Size Mismatched, Expect {expectation}") { }
    }
}