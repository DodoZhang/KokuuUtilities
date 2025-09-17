using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Kokuu
{
    [Serializable]
    public class Matrix : IEquatable<Matrix>, IFormattable
    {
        [SerializeField] private int _row, _col;
        [SerializeField] private float[] _val;
        
        public Matrix(int row, int col, IEnumerable<float> values = null)
        {
            if (row <= 0) throw new ArgumentOutOfRangeException(nameof(row));
            if (col <= 0) throw new ArgumentOutOfRangeException(nameof(col));
            
            _row = row;
            _col = col;
            _val = new float[_row * _col];
            
            if (values is not null) Set(values);
        }
        public Matrix(Matrix mat)
        {
            _row = mat._row;
            _col = mat._col;
            _val = new float[_row * _col];
            for (int i = 0, s = _row * _col; i < s; i++)
                _val[i] = mat._val[i];
        }

        public int row
        {
            get => _row;
            set
            {
                if (value == _row) return;
                float[] val = new float[value * _col];
                for (int r = 0; r < Math.Min(value, _row); r++)
                for (int c = 0; c < _col; c++)
                    val[r * _col + c] = _val[r * _col + c];
                _val = val;
                _row = value;
            }
        }
        public int column
        {
            get => _col;
            set
            {
                if (value == _col) return;
                float[] val = new float[_row * value];
                for (int r = 0; r < _row; r++)
                for (int c = 0; c < Math.Min(value, _col); c++)
                    val[r * value + c] = _val[r * _col + c];
                _val = val;
                _col = value;
            }
        }
        
        public float this[int r, int c]
        {
            get => _val[r * _col + c];
            set => _val[r * _col + c] = value;
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
            if (string.IsNullOrEmpty(format)) format = "F2";
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
            StringBuilder builder = new();
            for (int r = 0; r < _row; r++)
            {
                for (int c = 0; c < _col; c++)
                {
                    builder.Append(this[r, c].ToString(format, formatProvider));
                    if (c != _col - 1) builder.Append('\t');
                }
                if (r != _row - 1) builder.Append('\n');
            }
            return builder.ToString();
        }

        public bool Equals(Matrix other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (_row != other._row || _col != other._col) return false;
            for (int i = 0, s = _row * _col; i < s; i++)
                if (Math.Abs(_val[i] - other._val[i]) >= float.Epsilon)
                    return false;
            return true;
        }
        
        public static Matrix operator +(Matrix a, Matrix b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (a._row != b._row || a._col != b._col)
                throw new MatrixSizeMismatchException($"Size: {a._row} * {a._col}");

            Matrix result = new Matrix(a._row, a._col);
            for (int i = 0, s = a._row * a._col; i < s; i++)
                result._val[i] = a._val[i] + b._val[i];
            return result;
        }
        public static Matrix operator -(Matrix a, Matrix b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (a._row != b._row || a._col != b._col)
                throw new MatrixSizeMismatchException($"Size: {a._row} * {a._col}");

            Matrix result = new Matrix(a._row, a._col);
            for (int i = 0, s = a._row * a._col; i < s; i++)
                result._val[i] = a._val[i] - b._val[i];
            return result;
        }
        public static Matrix operator *(Matrix a, Matrix b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (a._col != b._row) throw new MatrixSizeMismatchException($"Row Counts: {a._col}");

            Matrix result = new Matrix(a._row, b._col);
            for (int i = 0; i < a._row; i++)
            for (int j = 0; j < b._col; j++)
            {
                float v = 0;
                for (int k = 0; k < a._col; k++)
                    v += a[i, k] * b[k, j];
                result[i, j] = v;
            }
            return result;
        }
        public static Matrix operator *(Matrix a, float d)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            
            Matrix result = new Matrix(a._row, a._col);
            for (int i = 0, s = a._row * a._col; i < s; i++)
                result._val[i] = d * a._val[i];
            return result;
        }
        public static Matrix operator *(float d, Matrix a)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            
            Matrix result = new Matrix(a._row, a._col);
            for (int i = 0, s = a._row * a._col; i < s; i++)
                result._val[i] = d * a._val[i];
            return result;
        }

        public float determinant
        {
            get
            {
                if (row != column)
                    throw new MatrixSizeMismatchException($"Row Counts({row}) Equals to Column Counts({column})");
                
                float det = 1;
                for (int i = 0; i < row; i++)
                {
                    if (Math.Abs(this[i, i]) < float.Epsilon)
                    {
                        for (int j = i + 1; j <= row; j++)
                        {
                            if (j == row) return 0;
                            if (Math.Abs(this[j, i]) < float.Epsilon) continue;
                            SwapRows(i, j);
                            det = -det;
                            break;
                        }
                    }

                    float s = this[i, i];
                    ScaleRow(i, 1 / s);
                    det *= s;

                    for (int j = 0; j < row; j++)
                    {
                        if (j == i) continue;
                        AddRow(j, -this[j, i], i);
                    }
                }
                
                return det;
            }
        }

        private void SwapRows(int i, int j)
        {
            for (int c = 0; c < column; c++)
                (this[i, c], this[j, c]) = (this[j, c], this[i, c]);
        }
        private void ScaleRow(int r, float s)
        {
            for (int c = 0; c < column; c++) this[r, c] *= s;
        }
        private void AddRow(int i, float s, int j)
        {
            for (int c = 0; c < column; c++)
                this[i, c] += s * this[j, c];
        }
    }
}