using System;
using System.Globalization;

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
#endif

namespace Kokuu.Maths
{
    [Serializable]
    public struct Complex : IEquatable<Complex>, IFormattable
    {
        public float real;
        public float imag;

        public Complex(float real, float imag)
        {
            this.real = real;
            this.imag = imag;
        }
        public Complex(Complex c) : this(c.real, c.imag) { }
        public static Complex MagArg(float mag, float arg) =>
            new(mag * (float)Math.Cos(arg), mag * (float)Math.Sin(arg));
        public static Complex Parse(string text) => new ComplexPhaser().Phase(text);
        public static bool TryParse(string text, out Complex result)
        {
            try
            {
                result = Parse(text);
                return true;
            }
            catch
            {
                result = zero;
                return false;
            }
        }

        public static Complex zero => new(0, 0);
        public static Complex one => new(1, 0);
        public static Complex j => new(0, 1);

        public static Complex Lerp(Complex a, Complex b, float t)
        {
            t = Math.Clamp(t, 0, 1);
            return new Complex(a.real + (b.real - a.real) * t, a.imag + (b.imag - a.imag) * t);
        }

        public static Complex LerpUnclamped(Complex a, Complex b, float t)
        {
            return new Complex(a.real + (b.real - a.real) * t, a.imag + (b.imag - a.imag) * t);
        }

        public static Complex Exp(Complex x) => MagArg((float)Math.Exp(x.real), x.imag);
        public static Complex Log(Complex x) => new((float)Math.Log(x.magnitude), x.argument);
        public static Complex Pow(Complex x, Complex p) => Exp(p * Log(x));
        public static Complex Log(Complex x, Complex b) => Log(x) / Log(b);
        public static Complex Log2(Complex x) => Log(x) / 0.69314718056f;
        public static Complex Log10(Complex x) => Log(x) / 2.30258509299f;
        public static Complex Sin(Complex x) => (Exp(j * x) - Exp(-j * x)) / (2 * j);
        public static Complex Cos(Complex x) => (Exp(j * x) + Exp(-j * x)) / 2;
        public static Complex Sinh(Complex x) => (Exp(x) - Exp(-x)) / 2;
        public static Complex Cosh(Complex x) => (Exp(x) + Exp(-x)) / 2;

        public void Normalize()
        {
            float mag = magnitude;
            if (mag.IsZero()) this = zero;
            else this /= mag;
        }

        public Complex normalized
        {
            get
            {
                Complex norm = this;
                norm.Normalize();
                return norm;
            }
        }

        public void Conjugate() => imag = -imag;
        public Complex conjugated => new(real, -imag);

        public float magnitude
        {
            get => (float)Math.Sqrt(real * real + imag * imag);
            set
            {
                float mag = magnitude;
                if (mag.IsZero()) this = new Complex(value, 0);
                else this *= value / mag;
            }
        }
        public float sqrMagnitude => real * real + imag * imag;
        public float argument
        {
            get => (float)Math.Atan2(imag, real);
            set => this = MagArg(magnitude, value);
        }

        public override string ToString() => ToString(null, null);
        public string ToString(string format) => ToString(format, null);
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format)) format = "A";
            bool autoFormat = format == "A";
            if (autoFormat) format = "G";
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;

            if (real.IsZero())
            {
                if (imag.IsZero()) return "0";
                if (autoFormat && imag.IsMinusOne()) return "-j";
                if (autoFormat && imag.IsOne()) return "j";
                if (imag < 0) return $"-j{(-imag).ToString(format, formatProvider)}";
                return $"j{imag.ToString(format, formatProvider)}";
            }
            else
            {
                if (imag.IsZero()) return real.ToString(format, formatProvider);
                if (autoFormat && imag.IsMinusOne()) return $"{real.ToString(format, formatProvider)} - j";
                if (autoFormat && imag.IsOne()) return $"{real.ToString(format, formatProvider)} + j";
                if (imag < 0) return $"{real.ToString(format, formatProvider)} -" +
                                     $" j{(-imag).ToString(format, formatProvider)}";
                return $"{real.ToString(format, formatProvider)} +" +
                       $" j{imag.ToString(format, formatProvider)}";
            }
        }

        public override int GetHashCode() => HashCode.Combine(real, imag);

        public bool Equals(Complex other) => real.Equals(other.real) && imag.Equals(other.imag);
        public override bool Equals(object obj) => obj is Complex other && Equals(other);

        public static Complex operator +(Complex a, Complex b) => new(a.real + b.real, a.imag + b.imag);
        public static Complex operator -(Complex a, Complex b) => new(a.real - b.real, a.imag - b.imag);

        public static Complex operator *(Complex a, Complex b) =>
            new(a.real * b.real - a.imag * b.imag, a.imag * b.real + a.real * b.imag);

        public static Complex operator /(Complex a, Complex b) => a * b.conjugated / b.sqrMagnitude;
        public static Complex operator -(Complex a) => new(-a.real, -a.imag);
        public static Complex operator *(Complex a, float d) => new(a.real * d, a.imag * d);
        public static Complex operator *(float d, Complex a) => new(a.real * d, a.imag * d);
        public static Complex operator /(Complex a, float d) => new(a.real / d, a.imag / d);

        public static bool operator ==(Complex lhs, Complex rhs) => lhs.Equals(rhs);
        public static bool operator !=(Complex lhs, Complex rhs) => !lhs.Equals(rhs);
        
        public bool IsZero() => real.IsZero() && imag.IsZero();

        public static implicit operator Complex(float x) => new(x, 0);
        public static explicit operator float(Complex x) => x.real;

        private class ComplexPhaser
        {
            private string str;
            private int index;

            public Complex Phase(string text)
            {
                str = text;
                
                PhaseWhiteSpace();
                if (index >= str.Length) return zero;
                
                Complex c = PhaseNumber(true);
                while (index < str.Length) c += PhaseNumber();

                return c;
            }

            private Complex PhaseNumber(bool isFirst = false)
            {
                bool isNegative = false;
                float number;
                
                switch (Top())
                {
                    case '+':
                        index++;
                        break;
                    case '-':
                        index++;
                        isNegative = true;
                        break;
                    default:
                        if (!isFirst) throw new FormatException();
                        break;
                }
                PhaseWhiteSpace();
                
                if (Top() is 'i' or 'j')
                {
                    index++;
                    PhaseWhiteSpace();

                    if (index >= str.Length || Top() is < '0' or > '9')
                        return isNegative ? -j : j;
                    
                    number = PhasePositiveNumber();
                    PhaseWhiteSpace();
                    return new Complex(0, isNegative ? -number : number);
                }

                number = PhasePositiveNumber();
                PhaseWhiteSpace();
                
                if (index >= str.Length || Top() is not ('i' or 'j'))
                    return new Complex(isNegative ? -number : number, 0);

                index++;
                PhaseWhiteSpace();
                return new Complex(0, isNegative ? -number : number);
            }

            private float PhasePositiveNumber()
            {
                float fraction = 0;
                int exponent = 0;
                int temp;

                char ch = Next();

                if (ch is '0') { }
                else if (ch is >= '1' and <= '9')
                {
                    fraction = ch - '0';

                    while (TryPhaseDigit(out temp))
                        fraction = fraction * 10 + temp;
                }
                else throw new FormatException();

                if (index < str.Length && str[index] is '.')
                {
                    index++;
                    
                    fraction = fraction * 10 + PhaseDigit();
                    exponent--;

                    while (TryPhaseDigit(out temp))
                    {
                        fraction = fraction * 10 + temp;
                        exponent--;
                    }
                }

                if (index < str.Length && str[index] is 'e' or 'E')
                {
                    index++;

                    bool isExpNegative = false;
                    ch = Next();
                    if (ch is '-') isExpNegative = true;
                    else if (ch is '+') { }
                    else if (ch is >= '0' and <= '9') index--;
                    else throw new FormatException();
                    
                    int exp = PhaseDigit();
                    while (TryPhaseDigit(out int digit)) exp = exp * 10 + digit;
                    
                    if (isExpNegative) exp = -exp;
                    exponent += exp;
                }
                
                if (exponent >= 0)
                    for (int i = 0; i < exponent; i++)
                        fraction *= 10;
                else
                    for (int i = 0; i > exponent; i--)
                        fraction /= 10;
                return fraction;

                int PhaseDigit()
                {
                    char c = Next();
                    if (c is >= '0' and <= '9') return c - '0';
                    throw new FormatException();
                }
                
                bool TryPhaseDigit(out int d)
                {
                    d = 0;
                    if (index >= str.Length) return false;
                    char c = str[index];
                    if (c is not (>= '0' and <= '9')) return false;
                    index++;
                    d = c - '0';
                    return true;
                }
            }
        
            private void PhaseWhiteSpace()
            {
                while (index < str.Length && str[index] is ' ' or '\t' or '\r' or '\n') index++;
            }
        
            private char Top()
            {
                if (index >= str.Length) throw new FormatException();
                return str[index];
            }
            
            private char Next()
            {
                if (index >= str.Length) throw new FormatException();
                return str[index++];
            }
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(Complex))]
        private class ComplexPropertyDrawer : PropertyDrawer
        {
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                label = EditorGUI.BeginProperty(position, label, property);
                
                SerializedProperty realProperty = property.FindPropertyRelative(nameof(real));
                SerializedProperty imagProperty = property.FindPropertyRelative(nameof(imag));
                string str = new Complex(realProperty.floatValue, imagProperty.floatValue).ToString();
                
                EditorGUI.BeginChangeCheck();
                str = EditorGUI.TextField(position, label, str);
                if (EditorGUI.EndChangeCheck())
                {
                    if (TryParse(str, out Complex c))
                    {
                        realProperty.floatValue = c.real;
                        imagProperty.floatValue = c.imag;
                    }
                }
                
                EditorGUI.EndProperty();
            }
        }
#endif
    }
}