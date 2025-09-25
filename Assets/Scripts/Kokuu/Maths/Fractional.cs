using System;
using System.Globalization;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kokuu.Maths
{
    [Serializable]
    public struct Fractional : IEquatable<Fractional>, IComparable<Fractional>, IComparable, IFormattable
    {
        [SerializeField] private int _numerator;
        [SerializeField] private int _denominator;
        
        public int numerator => _numerator;
        public int denominator => _denominator;

        public Fractional(int numerator, int denominator = 1)
        {
            if (denominator < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }
            
            _numerator = numerator;
            _denominator = denominator;
            Simplify();
        }
        private Fractional(long numerator, long denominator = 1)
        {
            _numerator = 0;
            _denominator = 0;
            Set(numerator, denominator);
        }
        public Fractional(Fractional frac)
        {
            _numerator = frac._numerator;
            _denominator = frac._denominator;
        }

        public static Fractional Parse(string text) =>
            new FractionalPhaser().TryPhase(text, out Fractional frac) ? frac : throw new FormatException();
        public static bool TryParse(string text, out Fractional frac) =>
            new FractionalPhaser().TryPhase(text, out frac);

        public static Fractional Zero => new(0, 1);
        public static Fractional One => new(1, 1);
        public static Fractional NaN => new(0, 0);
        public static Fractional PositiveInfinity => new(1, 0);
        public static Fractional NegativeInfinity => new(-1, 0);

        public bool isZero => numerator == 0 && denominator != 0;
        public bool isNaN => numerator == 0 && denominator == 0;
        public bool isPositiveInfinity => numerator == 1 && denominator == 0;
        public bool isNegativeInfinity => numerator == -1 && denominator == 0;
        public bool isInfinity => numerator != 0 && denominator == 0;
        public bool isFinity => denominator != 0;
        
        public override string ToString() => ToString(null, null);
        public string ToString(string format) => ToString(format, null);
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format)) format = "D";
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;

            if (isNaN) return "NaN";
            if (isPositiveInfinity) return "Infinity";
            if (isNegativeInfinity) return "-Infinity";
            if (denominator == 1) return numerator.ToString(format, formatProvider);
            return $"{numerator.ToString(format, formatProvider)}/{denominator.ToString(format, formatProvider)}";
        }

        public override int GetHashCode() => HashCode.Combine(numerator, denominator);

        public bool Equals(Fractional other) =>
            numerator.Equals(other.numerator) && denominator.Equals(other.denominator) && !isNaN;
        public override bool Equals(object obj) => obj is Fractional other && Equals(other);
        
        public int CompareTo(Fractional other) =>
            ((long)numerator * other.denominator).CompareTo((long)other.numerator * denominator);
        public int CompareTo(object obj)
        {
            if (obj is null) return 1;
            return obj is Fractional other
                ? CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(Fractional)}");
        }

        public static Fractional operator +(Fractional a, Fractional b) =>
            new((long)a.numerator * b.denominator + (long)b.numerator * a.denominator,
                (long)a.denominator * b.denominator);
        public static Fractional operator -(Fractional a, Fractional b) =>
            new((long)a.numerator * b.denominator - (long)b.numerator * a.denominator,
                (long)a.denominator * b.denominator);
        public static Fractional operator *(Fractional a, Fractional b) =>
            new((long)a.numerator * b.numerator, (long)a.denominator * b.denominator);
        public static Fractional operator /(Fractional a, Fractional b) => 
            new((long)a.numerator * b.denominator, (long)a.denominator * b.numerator);
        public static Fractional operator %(Fractional a, Fractional b) => a - Truncate(a / b) * b;
        public static Fractional operator -(Fractional a) => new(-a.numerator, a.denominator);

        public static bool operator ==(Fractional lhs, Fractional rhs) => lhs.Equals(rhs);
        public static bool operator !=(Fractional lhs, Fractional rhs) => !lhs.Equals(rhs);
        
        public static bool operator <(Fractional left, Fractional right) => left.CompareTo(right) < 0;
        public static bool operator >(Fractional left, Fractional right) => left.CompareTo(right) > 0;
        public static bool operator <=(Fractional left, Fractional right) => left.CompareTo(right) <= 0;
        public static bool operator >=(Fractional left, Fractional right) => left.CompareTo(right) >= 0;

        public static Fractional Abs(Fractional x) => new(Math.Abs(x.numerator), x.denominator);
        public static Fractional Min(Fractional x, Fractional y) => x < y ? x : y;
        public static Fractional Max(Fractional x, Fractional y) => x > y ? x : y;
        public static Fractional Pow(Fractional x, int p)
        {
            if (p < 0)
            {
                x = new Fractional(x.denominator, x.numerator);
                p = -p;
            }

            Fractional result = One;
            Fractional temp = x;
            
            for (; p > 0; p /= 2)
            {
                if (p % 2 == 1) result *= temp;
                temp *= temp;
            }

            return result;
        }
        public static Fractional Ceil(Fractional x)
        {
            if (!x.isFinity) return x;
            return x.denominator == 1 ? x.numerator : x.numerator / x.denominator + 1;
        }
        public static Fractional Floor(Fractional x)
        {
            if (!x.isFinity) return x;
            return x.numerator / x.denominator;
        }
        public static Fractional Round(Fractional x)
        {
            if (!x.isFinity) return x;
            return (int)((x.numerator * 2L + x.denominator) / (x.denominator * 2L));
        }
        public static int CeilToInt(Fractional x) => (int)Ceil(x);
        public static int FloorToInt(Fractional x) => (int)Floor(x);
        public static int RoundToInt(Fractional x) => (int)Round(x);
        public static int Sign(Fractional x) => Math.Sign(x.numerator);
        public static Fractional Clamp(Fractional value, Fractional min, Fractional max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        public static Fractional Clamp01(Fractional value)
        {
            if (value.numerator < 0) return Zero;
            if (value.numerator > value.denominator) return One;
            return value;
        }
        public static Fractional Lerp(Fractional a, Fractional b, Fractional t) => a + (b - a) * Clamp01(t);
        public static Fractional LerpUnclamped(Fractional a, Fractional b, Fractional t) => a + (b - a) * t;
        public static Fractional Repeat(Fractional t, Fractional length) =>
            Clamp(t - Floor(t / length) * length, Zero, length);
        public static Fractional PingPong(Fractional t, Fractional length)
        {
            t = Repeat(t, 2 * length);
            return length - Abs(t - length);
        }
        public static Fractional Truncate(Fractional x) => x.numerator >= 0 ? Floor(x) : Ceil(x);

        private const long MaxValue_2 = long.MaxValue / 2;

        public static implicit operator Fractional(int x) => new(x, 1);
        public static explicit operator int(Fractional x)
        {
            if (x.isNaN) return 0;
            if (x.isPositiveInfinity) return int.MaxValue;
            if (x.isNegativeInfinity) return int.MinValue;
            return x.numerator / x.denominator;
        }
        public static explicit operator Fractional(float x)
        {
            if (float.IsNaN(x)) return NaN;
            if (float.IsPositiveInfinity(x)) return PositiveInfinity;
            if (float.IsNegativeInfinity(x)) return NegativeInfinity;
            
            if (x > int.MaxValue) return PositiveInfinity;
            if (x < int.MinValue) return NegativeInfinity;
            
            bool isNegative = x < 0;
            if (isNegative) x = -x;
            
            long d = 1;
            while (!(x - (float)Math.Floor(x)).IsZero() && x <= MaxValue_2 && d <= MaxValue_2)
            {
                x *= 2;
                d *= 2;
            }
            long n = (long)Math.Round(x);
            if (isNegative) n = -n;
            Fractional frac = new();
            frac.Set(n, d, false);
            return frac;
        }
        public static explicit operator float(Fractional x) => (float)x.numerator / x.denominator;

        private void Set(long n, long d, bool warning = true)
        {
            if (d == 0)
            {
                _numerator = Math.Sign(n);
                _denominator = 0;
                return;
            }
            
            if (d < 0)
            {
                n = -n;
                d = -d;
            }
            
            long a = Math.Abs(n), b = d;
            while (b != 0) (a, b) = (b, a % b);
            n /= a;
            d /= a;

            if (Overflown() && warning) OverflowWarning();
            while (Overflown())
            {
                n /= 2;
                d /= 2;
            }
            
            _numerator = (int)n;
            _denominator = (int)d;
            Simplify();
            return;
            
            bool Overflown() => n > int.MaxValue || n < int.MinValue ||
                                d > int.MaxValue || d < int.MinValue;
        }

        private void Simplify()
        {
            if (_denominator == 0)
            {
                _numerator = Math.Sign(_numerator);
                return;
            }

            int a = Math.Abs(_numerator), b = _denominator;
            while (b != 0) (a, b) = (b, a % b);
            _numerator /= a;
            _denominator /= a;
        }

        private void OverflowWarning()
        {
            Debug.LogWarning("[Fractional] Fractional Overflown, Which Will Cause Accuracy Lose.");
        }
        
        private class FractionalPhaser
        {
            private string str;
            private int index;

            public bool TryPhase(string text, out Fractional frac)
            {
                str = text;
                
                index = 0;
                if (TryPhaseFractional(out frac)) return true;

                index = 0;
                if (TryPhaseDecimal(out frac)) return true;
                
                frac = Zero;
                return false;
            }

            private bool TryPhaseFractional(out Fractional f)
            {
                f = Zero;
                
                PhaseWhiteSpace();
                if (index >= str.Length) return true;
                
                bool isNegative = TryPhaseChar('-');
                PhaseWhiteSpace();
                
                long numerator = 0, denominator = 0;
                
                int numeratorBegin = index;
                if (!TryPhasePositiveInteger()) return false;
                int numeratorEnd = index;
                PhaseWhiteSpace();
                
                bool isFractional = TryPhaseChar('/');
                PhaseWhiteSpace();
                if (!isFractional)
                {
                    if (index < str.Length) return false;

                    for (int i = numeratorBegin; i < numeratorEnd; i++)
                    {
                        long digit = str[i] - '0';
                        if (numerator > (long.MaxValue - digit) / 10)
                        {
                            f = isNegative ? NegativeInfinity : PositiveInfinity;
                            return true;
                        }
                        numerator = numerator * 10 + digit;
                    }
                    f.Set(isNegative ? -numerator : numerator, 1, false);
                    return true;
                }
                
                int denominatorBegin = index;
                if (!TryPhasePositiveInteger()) return false;
                int denominatorEnd = index;
                PhaseWhiteSpace();
                
                if (index < str.Length) return false;

                int numeratorIndex, denominatorIndex;
                for (numeratorIndex = numeratorBegin; numeratorIndex < numeratorEnd; numeratorIndex++)
                {
                    long numeratorDigit = str[numeratorIndex] - '0';
                    if (numerator > (long.MaxValue - numeratorDigit) / 10) break;
                    numerator = numerator * 10 + numeratorDigit;
                }

                for (denominatorIndex = denominatorBegin; denominatorIndex < denominatorEnd; denominatorIndex++)
                {
                    long denominatorDigit = str[denominatorIndex] - '0';
                    if (denominator > (long.MaxValue - denominatorDigit) / 10) break;
                    denominator = denominator * 10 + denominatorDigit;
                }
                
                int numeratorRemaining = numeratorEnd - numeratorIndex;
                int denominatorRemaining = denominatorEnd - denominatorIndex;
                int deltaRemaining = numeratorRemaining - denominatorRemaining;
                if (deltaRemaining > 0)
                    for (int i = 0; i < deltaRemaining; i++)
                        denominator /= 10;
                if (deltaRemaining < 0)
                    for (int i = 0; i < -deltaRemaining; i++)
                        numerator /= 10;
                
                f.Set(isNegative ? -numerator : numerator, denominator, false);
                return true;
            }

            private bool TryPhaseDecimal(out Fractional f)
            {
                f = Zero;
                
                PhaseWhiteSpace();
                if (index >= str.Length) return true;
                
                bool isNegative = TryPhaseChar('-');
                PhaseWhiteSpace();
                
                long numerator = 0, denominator = 1;
                
                int integerBegin = index;
                if (!TryPhasePositiveInteger()) return false;
                int integerEnd = index;
                PhaseWhiteSpace();
                
                bool isDecimal = TryPhaseChar('.');
                PhaseWhiteSpace();
                if (!isDecimal)
                {
                    if (index < str.Length) return false;

                    for (int i = integerBegin; i < integerEnd; i++)
                    {
                        long digit = str[i] - '0';
                        if (numerator > (long.MaxValue - digit) / 10)
                        {
                            f = isNegative ? NegativeInfinity : PositiveInfinity;
                            return true;
                        }
                        numerator = numerator * 10 + digit;
                    }
                    f.Set(isNegative ? -numerator : numerator, 1, false);
                    return true;
                }
                
                int decimalBegin = index;
                if (!TryPhasePositiveInteger(true)) return false;
                int decimalEnd = index;
                PhaseWhiteSpace();
                
                if (index < str.Length) return false;

                for (int i = integerBegin; i < integerEnd; i++)
                {
                    long digit = str[i] - '0';
                    if (numerator > (long.MaxValue - digit) / 10)
                    {
                        f = isNegative ? NegativeInfinity : PositiveInfinity;
                        return true;
                    }
                    numerator = numerator * 10 + digit;
                }

                for (int i = decimalBegin; i < decimalEnd; i++)
                {
                    long digit = str[i] - '0';
                    if (numerator > (long.MaxValue - digit) / 10 || denominator > long.MaxValue / 10)
                    {
                        f.Set(isNegative ? -numerator : numerator, denominator, false);
                        return true;
                    }
                    numerator = numerator * 10 + digit;
                    denominator *= 10;
                }
                
                f.Set(isNegative ? -numerator : numerator, denominator, false);
                return true;
            }

            private bool TryPhasePositiveInteger(bool allowZeroPrefix = false)
            {
                if (!TryPhaseDigit(out int digit)) return false;
                if (digit == 0 && !allowZeroPrefix) return true;
                while (TryPhaseDigit(out _)) { }
                return true;
            }
                
            private bool TryPhaseDigit(out int d)
            {
                d = 0;
                if (index >= str.Length) return false;
                char c = str[index];
                if (c is not (>= '0' and <= '9')) return false;
                index++;
                d = c - '0';
                return true;
            }
        
            private void PhaseWhiteSpace()
            {
                while (index < str.Length && str[index] is ' ' or '\t' or '\r' or '\n') index++;
            }
        
            private bool TryPhaseChar(char ch)
            {
                if (index < str.Length && str[index] == ch)
                {
                    index++;
                    return true;
                }
                return false;
            }
        }
        
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(Fractional))]
        private class FractionalPropertyDrawer : PropertyDrawer
        {
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                label = EditorGUI.BeginProperty(position, label, property);
                
                SerializedProperty numeratorProperty = property.FindPropertyRelative(nameof(_numerator));
                SerializedProperty denominatorProperty = property.FindPropertyRelative(nameof(_denominator));
                string str = new Fractional(numeratorProperty.intValue, denominatorProperty.intValue).ToString();
                
                EditorGUI.BeginChangeCheck();
                str = EditorGUI.TextField(position, label, str);
                if (EditorGUI.EndChangeCheck())
                {
                    if (TryParse(str, out Fractional frac))
                    {
                        numeratorProperty.intValue = frac.numerator;
                        denominatorProperty.intValue = frac.denominator;
                    }
                }
                
                EditorGUI.EndProperty();
            }
        }
#endif
    }
}