using System;

namespace Kokuu
{
    public class MatrixSizeMismatchException : Exception
    {
        public MatrixSizeMismatchException(string expectation) :
            base($"Matrix Size Mismatched, Expect {expectation}") { }
    }
}