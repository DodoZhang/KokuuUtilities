namespace Kokuu.Maths
{
    public static class FloatExtensions
    {
        private const float Epsilon = 1e-5f;
        
        public static bool IsZero(this float value) => value is > -Epsilon and < Epsilon;
        public static bool IsMinusOne(this float value) => value + 1 is > -Epsilon and < Epsilon;
        public static bool IsOne(this float value) => value - 1 is > -Epsilon and < Epsilon;
    }
}