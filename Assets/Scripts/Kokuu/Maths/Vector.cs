using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kokuu.Maths
{
    [Serializable]
    public class Vector : IEquatable<Vector>, IFormattable, ISerializationCallbackReceiver
    {
        [SerializeField] private int _dim;
        [SerializeField] private float[] _val;
        
        public Vector(int dimension, IEnumerable<float> values = null)
        {
            if (dimension <= 0) throw new ArgumentOutOfRangeException(nameof(dimension));
            
            _dim = dimension;
            _val = new float[_dim];

            for (int i = 0; i < _dim; i++) _val[i] = 0;
            if (values is not null) Set(values);
        }
        public Vector(Vector vec)
        {
            _dim = vec._dim;
            _val = new float[_dim];
            for (int i = 0; i < _dim; i++)
                _val[i] = vec._val[i];
        }
        public static Vector One(int dimension)
        {
            Vector vec = new(dimension);
            for (int i = 0; i < dimension; i++)
                vec[i] = 1;
            return vec;
        }

        public int dimension
        {
            get => _dim;
            set
            {
                if (value == _dim) return;
                float[] val = new float[value];
                for (int i = 0; i < Math.Min(value, _dim); i++)
                    val[i] = _val[i];
                _val = val;
                _dim = value;
            }
        }

        public float this[int i]
        {
            get => _val[i];
            set => _val[i] = value;
        }
        public float this[Index i]
        {
            get => _val[i.GetOffset(_dim)];
            set => _val[i.GetOffset(_dim)] = value;
        }
        public Vector this[Range r]
        {
            get
            {
                (int offset, int length) = r.GetOffsetAndLength(_dim);
                Vector vec = new(length);
                for (int i = 0; i < length; i++)
                    vec[i] = this[offset + i];
                return vec;
            }
            set
            {
                (int offset, int length) = r.GetOffsetAndLength(_dim);
                if (value._dim != length)
                    throw new SizeMismatchException($"Dimension: {length}");
                for (int i = 0; i < length; i++)
                    this[offset + i] = value[i];
            }
        }

        public void Set(IEnumerable<float> values)
        {
            if (values is null) throw new ArgumentNullException(nameof(values));
            
            int i = 0;
            foreach (float value in values)
            {
                if (i >= _val.Length) break;
                _val[i++] = value;
            }
        }

        public override string ToString() => ToString(null, null);
        public string ToString(string format) => ToString(format, null);
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format)) format = "G";
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
            StringBuilder builder = new StringBuilder().Append("(");
            for (int i = 0; i < _dim; i++)
            {
                builder.Append(this[i].ToString(format, formatProvider));
                if (i != _dim - 1) builder.Append(", ");
            }
            return builder.Append(")").ToString();
        }

        public bool Equals(Vector other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (_dim != other._dim) return false;
            for (int i = 0; i < _dim; i++)
                if (!_val[i].Equals(other._val[i])) return false;
            return true;
        }
        
        public static Vector operator +(Vector a, Vector b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (a._dim != b._dim) throw new SizeMismatchException($"Dimension: {a._dim}");

            Vector result = new Vector(a._dim);
            for (int i = 0; i < a._dim; i++)
                result._val[i] = a._val[i] + b._val[i];
            return result;
        }
        public static Vector operator -(Vector a, Vector b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (a._dim != b._dim) throw new SizeMismatchException($"Dimension: {a._dim}");

            Vector result = new Vector(a._dim);
            for (int i = 0; i < a._dim; i++)
                result._val[i] = a._val[i] - b._val[i];
            return result;
        }
        public static Vector operator *(Vector a, float d)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            
            Vector result = new Vector(a._dim);
            for (int i = 0; i < a._dim; i++)
                result._val[i] = d * a._val[i];
            return result;
        }
        public static Vector operator *(float d, Vector a)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            
            Vector result = new Vector(a._dim);
            for (int i = 0; i < a._dim; i++)
                result._val[i] = d * a._val[i];
            return result;
        }
        public static Vector operator /(Vector a, float d)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            
            Vector result = new Vector(a._dim);
            for (int i = 0; i < a._dim; i++)
                result._val[i] = a._val[i] / d;
            return result;
        }
        
        public float sqrMagnitude
        {
            get
            {
                float result = 0;
                for (int i = 0; i < _dim; i++)
                    result += _val[i] * _val[i];
                return result;
            }
        }
        public float magnitude => (float)Math.Sqrt(sqrMagnitude);
        
        public void Normalize()
        {
            float mag = magnitude;
            for (int i = 0; i < _dim; i++)
                _val[i] /= mag;
        }
        public Vector normalized
        {
            get
            {
                Vector norm = this;
                norm.Normalize();
                return norm;
            }
        }
        
        public Matrix transposed
        {
            get
            {
                Matrix mat = new(1, _dim);
                for (int c = 0; c < _dim; c++)
                    mat[0, c] = this[c];
                return mat;
            }
        }

        public static float Dot(Vector a, Vector b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (a._dim != b._dim) throw new SizeMismatchException($"Dimension: {a._dim}");
            
            float result = 0;
            for (int i = 0; i < a._dim; i++)
                result += a._val[i] * b._val[i];
            return result;
        }
        public static Vector Project(Vector v, Vector axis) => Dot(v, axis) / axis.sqrMagnitude * axis;
        public static Vector Reflect(Vector v, Vector norm) => 2 * Project(v, norm) - v;
        public static float CosAngleBetween(Vector a, Vector b) => Dot(a, b) / (a.magnitude * b.magnitude);
        public static float AngleBetween(Vector a, Vector b)
        {
            float cosAngle = CosAngleBetween(a, b);
            return cosAngle switch
            {
                >= 1 => 0,
                <= 0 => (float)Math.PI,
                _ => (float)Math.Acos(cosAngle)
            };
        }

        public static Vector operator *(Matrix m, Vector v)
        {
            if (m is null) throw new ArgumentNullException(nameof(m));
            if (v is null) throw new ArgumentNullException(nameof(v));
            if (m.column != v.dimension) throw new SizeMismatchException($"Dimension: {m.column}");

            Vector result = new(m.row);
            for (int i = 0; i < m.row; i++)
            {
                float s = 0;
                for (int k = 0; k < v.dimension; k++)
                    s += m[i, k] * v[k];
                result[i] = s;
            }
            return result;
        }
        
        public static implicit operator Matrix(Vector v) => new(v._dim, 1, v._val);
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            if (_val is null || _val.Length == 0) _val = new float[Math.Max(_dim, 1)];
            _dim = _val.Length;
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(Vector))]
        private class VectorPropertyDrawer : PropertyDrawer
        {
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(_val)), label);
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(_val)), label);
            }
        }
#endif
    }

    public static class VectorMatrixExtensions
    {
        public static Vector RowAt(this Matrix m, int row)
        {
            Vector v = new(m.column);
            for (int c = 0; c < m.column; c++)
                v[c] = m[row, c];
            return v;
        }
        public static Vector ColumnAt(this Matrix m, int column)
        {
            Vector v = new(m.row);
            for (int r = 0; r < m.row; r++)
                v[r] = m[r, column];
            return v;
        }
    }
}