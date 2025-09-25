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
    public class VectorC : IEquatable<VectorC>, IFormattable, ISerializationCallbackReceiver
    {
        [SerializeField] private int _dim;
        [SerializeField] private Complex[] _val;
        
        public VectorC(int dimension, IEnumerable<Complex> values = null)
        {
            if (dimension <= 0) throw new ArgumentOutOfRangeException(nameof(dimension));
            
            _dim = dimension;
            _val = new Complex[_dim];

            for (int i = 0; i < _dim; i++) _val[i] = 0;
            if (values is not null) Set(values);
        }
        public VectorC(VectorC vec)
        {
            _dim = vec._dim;
            _val = new Complex[_dim];
            for (int i = 0; i < _dim; i++)
                _val[i] = vec._val[i];
        }
        public static VectorC One(int dimension)
        {
            VectorC vec = new(dimension);
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
                Complex[] val = new Complex[value];
                for (int i = 0; i < Math.Min(value, _dim); i++)
                    val[i] = _val[i];
                _val = val;
                _dim = value;
            }
        }

        public Complex this[int i]
        {
            get => _val[i];
            set => _val[i] = value;
        }
        public Complex this[Index i]
        {
            get => _val[i.GetOffset(_dim)];
            set => _val[i.GetOffset(_dim)] = value;
        }
        public VectorC this[Range r]
        {
            get
            {
                (int offset, int length) = r.GetOffsetAndLength(_dim);
                VectorC vec = new(length);
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

        public void Set(IEnumerable<Complex> values)
        {
            if (values is null) throw new ArgumentNullException(nameof(values));
            
            int i = 0;
            foreach (Complex value in values)
            {
                if (i >= _val.Length) break;
                _val[i++] = value;
            }
        }

        public override string ToString() => ToString(null, null);
        public string ToString(string format) => ToString(format, null);
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format)) format = "A";
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
            
            StringBuilder builder = new StringBuilder().Append("(");
            for (int i = 0; i < _dim; i++)
            {
                builder.Append(this[i].ToString(format, formatProvider));
                if (i != _dim - 1) builder.Append(", ");
            }
            return builder.Append(")").ToString();
        }

        public bool Equals(VectorC other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (_dim != other._dim) return false;
            for (int i = 0; i < _dim; i++)
                if (!_val[i].Equals(other._val[i])) return false;
            return true;
        }
        
        public static VectorC operator +(VectorC a, VectorC b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (a._dim != b._dim) throw new SizeMismatchException($"Dimension: {a._dim}");

            VectorC result = new VectorC(a._dim);
            for (int i = 0; i < a._dim; i++)
                result._val[i] = a._val[i] + b._val[i];
            return result;
        }
        public static VectorC operator -(VectorC a, VectorC b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (a._dim != b._dim) throw new SizeMismatchException($"Dimension: {a._dim}");

            VectorC result = new VectorC(a._dim);
            for (int i = 0; i < a._dim; i++)
                result._val[i] = a._val[i] - b._val[i];
            return result;
        }
        public static VectorC operator *(VectorC a, Complex d)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            
            VectorC result = new VectorC(a._dim);
            for (int i = 0; i < a._dim; i++)
                result._val[i] = d * a._val[i];
            return result;
        }
        public static VectorC operator *(Complex d, VectorC a)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            
            VectorC result = new VectorC(a._dim);
            for (int i = 0; i < a._dim; i++)
                result._val[i] = d * a._val[i];
            return result;
        }
        public static VectorC operator /(VectorC a, Complex d)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            
            VectorC result = new VectorC(a._dim);
            for (int i = 0; i < a._dim; i++)
                result._val[i] = a._val[i] / d;
            return result;
        }
        public static VectorC operator -(VectorC a)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));

            VectorC result = new VectorC(a._dim);
            for (int i = 0; i < a._dim; i++)
                result._val[i] = -a._val[i];
            return result;
        }
        
        public float sqrMagnitude
        {
            get
            {
                float result = 0;
                for (int i = 0; i < _dim; i++)
                    result += _val[i].sqrMagnitude;
                return result;
            }
        }
        public float magnitude => (float)Math.Sqrt(sqrMagnitude);
        
        public void Normalize()
        {
            Complex mag = magnitude;
            for (int i = 0; i < _dim; i++)
                _val[i] /= mag;
        }
        public VectorC normalized
        {
            get
            {
                VectorC norm = this;
                norm.Normalize();
                return norm;
            }
        }
        
        public MatrixC transposed
        {
            get
            {
                MatrixC mat = new(1, _dim);
                for (int c = 0; c < _dim; c++)
                    mat[0, c] = this[c];
                return mat;
            }
        }

        public static Complex Dot(VectorC a, VectorC b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (a._dim != b._dim) throw new SizeMismatchException($"Dimension: {a._dim}");
            
            Complex result = Complex.Zero;
            for (int i = 0; i < a._dim; i++)
                result += a._val[i] * b._val[i].conjugated;
            return result;
        }
        public static VectorC Project(VectorC v, VectorC axis) => Dot(v, axis) / axis.sqrMagnitude * axis;
        public static VectorC Reflect(VectorC v, VectorC norm) => 2 * Project(v, norm) - v;

        public static VectorC operator *(MatrixC m, VectorC v)
        {
            if (m is null) throw new ArgumentNullException(nameof(m));
            if (v is null) throw new ArgumentNullException(nameof(v));
            if (m.column != v.dimension) throw new SizeMismatchException($"Dimension: {m.column}");

            VectorC result = new(m.row);
            for (int i = 0; i < m.row; i++)
            {
                Complex s = 0;
                for (int k = 0; k < v.dimension; k++)
                    s += m[i, k] * v[k];
                result[i] = s;
            }
            return result;
        }
        
        public static implicit operator MatrixC(VectorC v) => new(v._dim, 1, v._val);

        public static implicit operator VectorC(Vector v)
        {
            VectorC result = new VectorC(v.dimension);
            for (int i = 0; i < v.dimension; i++)
                result[i] = v[i];
            return result;
        }
        public static explicit operator Vector(VectorC v)
        {
            Vector result = new Vector(v.dimension);
            for (int i = 0; i < v.dimension; i++)
                result[i] = (float)v[i];
            return result;
        }
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            if (_val is null || _val.Length == 0) _val = new Complex[Math.Max(_dim, 1)];
            _dim = _val.Length;
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(VectorC))]
        private class VectorCPropertyDrawer : PropertyDrawer
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

    public static class VectorCMatrixCExtensions
    {
        public static VectorC RowAt(this MatrixC m, int row)
        {
            VectorC v = new(m.column);
            for (int c = 0; c < m.column; c++)
                v[c] = m[row, c];
            return v;
        }
        public static VectorC ColumnAt(this MatrixC m, int column)
        {
            VectorC v = new(m.row);
            for (int r = 0; r < m.row; r++)
                v[r] = m[r, column];
            return v;
        }
    }
}