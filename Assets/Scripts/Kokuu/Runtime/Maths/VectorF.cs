using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Kokuu.Maths
{
    [Serializable]
    public class VectorF : IEquatable<VectorF>, IFormattable, ISerializationCallbackReceiver
    {
        [SerializeField] private int _dim;
        [SerializeField] private Fractional[] _val;
        
        public VectorF(int dimension, IEnumerable<Fractional> values = null)
        {
            if (dimension <= 0) throw new ArgumentOutOfRangeException(nameof(dimension));
            
            _dim = dimension;
            _val = new Fractional[_dim];

            for (int i = 0; i < _dim; i++) _val[i] = 0;
            if (values is not null) Set(values);
        }
        public VectorF(VectorF vec)
        {
            _dim = vec._dim;
            _val = new Fractional[_dim];
            for (int i = 0; i < _dim; i++)
                _val[i] = vec._val[i];
        }
        public static VectorF One(int dimension)
        {
            VectorF vec = new(dimension);
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
                Fractional[] val = new Fractional[value];
                for (int i = 0; i < Math.Min(value, _dim); i++)
                    val[i] = _val[i];
                _val = val;
                _dim = value;
            }
        }

        public Fractional this[int i]
        {
            get => _val[i];
            set => _val[i] = value;
        }
        public Fractional this[Index i]
        {
            get => _val[i.GetOffset(_dim)];
            set => _val[i.GetOffset(_dim)] = value;
        }
        public VectorF this[Range r]
        {
            get
            {
                (int offset, int length) = r.GetOffsetAndLength(_dim);
                VectorF vec = new(length);
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

        public void Set(IEnumerable<Fractional> values)
        {
            if (values is null) throw new ArgumentNullException(nameof(values));
            
            int i = 0;
            foreach (Fractional value in values)
            {
                if (i >= _val.Length) break;
                _val[i++] = value;
            }
        }

        public override string ToString() => ToString(null, null);
        public string ToString(string format) => ToString(format, null);
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format)) format = "D";
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
            
            StringBuilder builder = new StringBuilder().Append("(");
            for (int i = 0; i < _dim; i++)
            {
                builder.Append(this[i].ToString(format, formatProvider));
                if (i != _dim - 1) builder.Append(", ");
            }
            return builder.Append(")").ToString();
        }

        public bool Equals(VectorF other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (_dim != other._dim) return false;
            for (int i = 0; i < _dim; i++)
                if (!_val[i].Equals(other._val[i])) return false;
            return true;
        }
        
        public static VectorF operator +(VectorF a, VectorF b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (a._dim != b._dim) throw new SizeMismatchException($"Dimension: {a._dim}");

            VectorF result = new VectorF(a._dim);
            for (int i = 0; i < a._dim; i++)
                result._val[i] = a._val[i] + b._val[i];
            return result;
        }
        public static VectorF operator -(VectorF a, VectorF b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (a._dim != b._dim) throw new SizeMismatchException($"Dimension: {a._dim}");

            VectorF result = new VectorF(a._dim);
            for (int i = 0; i < a._dim; i++)
                result._val[i] = a._val[i] - b._val[i];
            return result;
        }
        public static VectorF operator *(VectorF a, Fractional d)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            
            VectorF result = new VectorF(a._dim);
            for (int i = 0; i < a._dim; i++)
                result._val[i] = d * a._val[i];
            return result;
        }
        public static VectorF operator *(Fractional d, VectorF a)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            
            VectorF result = new VectorF(a._dim);
            for (int i = 0; i < a._dim; i++)
                result._val[i] = d * a._val[i];
            return result;
        }
        public static VectorF operator /(VectorF a, Fractional d)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            
            VectorF result = new VectorF(a._dim);
            for (int i = 0; i < a._dim; i++)
                result._val[i] = a._val[i] / d;
            return result;
        }
        public static VectorF operator -(VectorF a)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));

            VectorF result = new VectorF(a._dim);
            for (int i = 0; i < a._dim; i++)
                result._val[i] = -a._val[i];
            return result;
        }
        
        public Fractional sqrMagnitude
        {
            get
            {
                Fractional result = 0;
                for (int i = 0; i < _dim; i++)
                    result += _val[i] * _val[i];
                return result;
            }
        }
        public float magnitude => (float)Math.Sqrt((float)sqrMagnitude);
        
        public void Normalize()
        {
            Fractional mag = (Fractional)Math.Sqrt((float)sqrMagnitude);
            for (int i = 0; i < _dim; i++)
                _val[i] /= mag;
        }
        public VectorF normalized
        {
            get
            {
                VectorF norm = this;
                norm.Normalize();
                return norm;
            }
        }
        
        public MatrixF transposed
        {
            get
            {
                MatrixF mat = new(1, _dim);
                for (int c = 0; c < _dim; c++)
                    mat[0, c] = this[c];
                return mat;
            }
        }

        public static Fractional Dot(VectorF a, VectorF b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (a._dim != b._dim) throw new SizeMismatchException($"Dimension: {a._dim}");
            
            Fractional result = Fractional.Zero;
            for (int i = 0; i < a._dim; i++)
                result += a._val[i] * b._val[i];
            return result;
        }
        public static VectorF Project(VectorF v, VectorF axis) => Dot(v, axis) / axis.sqrMagnitude * axis;
        public static VectorF Reflect(VectorF v, VectorF norm) => 2 * Project(v, norm) - v;

        public static VectorF operator *(MatrixF m, VectorF v)
        {
            if (m is null) throw new ArgumentNullException(nameof(m));
            if (v is null) throw new ArgumentNullException(nameof(v));
            if (m.column != v.dimension) throw new SizeMismatchException($"Dimension: {m.column}");

            VectorF result = new(m.row);
            for (int i = 0; i < m.row; i++)
            {
                Fractional s = 0;
                for (int k = 0; k < v.dimension; k++)
                    s += m[i, k] * v[k];
                result[i] = s;
            }
            return result;
        }
        
        public static implicit operator MatrixF(VectorF v) => new(v._dim, 1, v._val);

        public static implicit operator VectorF(Vector v)
        {
            VectorF result = new VectorF(v.dimension);
            for (int i = 0; i < v.dimension; i++)
                result[i] = (Fractional)v[i];
            return result;
        }
        public static explicit operator Vector(VectorF v)
        {
            Vector result = new Vector(v.dimension);
            for (int i = 0; i < v.dimension; i++)
                result[i] = (float)v[i];
            return result;
        }
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            if (_val is null || _val.Length == 0) _val = new Fractional[Math.Max(_dim, 1)];
            _dim = _val.Length;
        }
    }

    public static class VectorFMatrixCExtensions
    {
        public static VectorF RowAt(this MatrixF m, int row)
        {
            VectorF v = new(m.column);
            for (int c = 0; c < m.column; c++)
                v[c] = m[row, c];
            return v;
        }
        public static VectorF ColumnAt(this MatrixF m, int column)
        {
            VectorF v = new(m.row);
            for (int r = 0; r < m.row; r++)
                v[r] = m[r, column];
            return v;
        }
    }
}