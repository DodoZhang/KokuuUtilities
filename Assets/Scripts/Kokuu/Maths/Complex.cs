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

        public static Complex zero => new(0, 0);
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
            if (mag < float.Epsilon) this = zero;
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
                if (mag < float.Epsilon) this = new Complex(value, 0);
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
            if (string.IsNullOrEmpty(format)) format = "F2";
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;

            if (Math.Abs(real) < float.Epsilon)
            {
                return imag switch
                {
                    <= -float.Epsilon => $"-j{(-imag).ToString(format, formatProvider)}",
                    >= float.Epsilon => $"j{imag.ToString(format, formatProvider)}",
                    _ => "0"
                };
            }
            else
            {
                return imag switch
                {
                    <= -float.Epsilon => $"{real.ToString(format, formatProvider)} -" +
                                         $" j{(-imag).ToString(format, formatProvider)}",
                    >= float.Epsilon => $"{real.ToString(format, formatProvider)} +" +
                                        $" j{imag.ToString(format, formatProvider)}",
                    _ => $"{real.ToString(format, formatProvider)}"
                };
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

        public static implicit operator Complex(float x) => new(x, 0);
        public static explicit operator float(Complex x) => x.real;

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(Complex))]
        private class ComplexPropertyDrawer : PropertyDrawer
        {
            private static readonly GUIContent[] sublabels = { new("Re"), new("Im") };

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                label = EditorGUI.BeginProperty(position, label, property);
                SerializedProperty iter = property.Copy();
                iter.Next(true);
                EditorGUI.MultiPropertyField(position, sublabels, iter, label);
                EditorGUI.EndProperty();
            }
        }
#endif
    }
}