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
        
        public Matrix(int row, int column, IEnumerable<float> values = null)
        {
            if (row <= 0) throw new ArgumentOutOfRangeException(nameof(row));
            if (column <= 0) throw new ArgumentOutOfRangeException(nameof(column));
            
            _row = row;
            _col = column;
            _val = new float[_row * _col];

            for (int i = 0; i < _val.Length; i++) _val[i] = 0;
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
        public static Matrix Identity(int row)
        {
            Matrix mat = new(row, row);
            for (int i = 0; i < row; i++)
                mat[i, i] = 1;
            return mat;
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
        public float this[Index r, Index c]
        {
            get => _val[r.GetOffset(_row) * _col + c.GetOffset(_col)];
            set => _val[r.GetOffset(_row) * _col + c.GetOffset(_col)] = value;
        }
        public Matrix this[Range r, Range c]
        {
            get
            {
                (int offsetR, int lengthR) = r.GetOffsetAndLength(_row);
                (int offsetC, int lengthC) = c.GetOffsetAndLength(_col);
                Matrix mat = new Matrix(lengthR, lengthC);
                for (int i = 0; i < lengthR; i++)
                for (int j = 0; j < lengthC; j++)
                    mat[i, j] = this[offsetR + i, offsetC + j];
                return mat;
            }
            set
            {
                (int offsetR, int lengthR) = r.GetOffsetAndLength(_row);
                (int offsetC, int lengthC) = c.GetOffsetAndLength(_col);
                if (value._row != lengthR || value._col != lengthC)
                    throw new MatrixSizeMismatchException($"Size: {lengthR} * {lengthC}");
                for (int i = 0; i < lengthR; i++)
                for (int j = 0; j < lengthC; j++)
                    this[offsetR + i, offsetC + j] = value[i, j];
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

        public static Matrix Pow(Matrix m, int p)
        {
            if (m is null) throw new ArgumentNullException(nameof(m));
            if (m.row != m.column)
                throw new MatrixSizeMismatchException($"Row Counts({m.row}) Equals to Column Counts({m.column})");

            if (p < 0)
            {
                m = m.inversed;
                p = -p;
                if (m is null) return null;
            }

            Matrix result = Identity(m.row);
            Matrix temp = new Matrix(m);
            
            for (; p > 0; p /= 2)
            {
                if (p % 2 == 1) result *= temp;
                temp *= temp;
            }

            return result;
        }

        public Matrix transposed
        {
            get
            {
                Matrix mat = new(column, row);
                for (int r = 0; r < row; r++)
                for (int c = 0; c < column; c++)
                    mat[c, r] = this[r, c];
                return mat;
            }
        }
        
        public float determinant
        {
            get
            {
                if (row != column)
                    throw new MatrixSizeMismatchException($"Row Counts({row}) Equals to Column Counts({column})");

                return new Matrix(this).Identify();
            }
        }

        public Matrix inversed
        {
            get
            {
                if (row != column)
                    throw new MatrixSizeMismatchException($"Row Counts({row}) Equals to Column Counts({column})");
                
                Matrix mat = new(row, row * 2)
                {
                    [.., ..row] = this,
                    [.., row..] = Identity(row)
                };
                mat.Identify();
                return mat[.., row..];
            }
        }

        public float trace
        {
            get
            {
                int tra = 0;
                Matrix mat = row >= column ? new Matrix(this) : transposed;
                
                for (int i = 0; i < row; i++)
                {
                    if (Math.Abs(mat[i, i]) < float.Epsilon)
                    {
                        for (int j = i + 1; j <= row; j++)
                        {
                            if (j == row) goto SkipRow;
                            if (!(Math.Abs(mat[j, i]) >= float.Epsilon)) continue;
                            mat.SwapRows(i, j);
                            break;
                        }
                    }
                    
                    float s = mat[i, i];
                    for (int j = i + 1; j < row; j++)
                        mat.AddRow(j, -mat[j, i] / s, i);
                    tra++;
                    
                    SkipRow: ;
                }

                return tra;
            }
        }

        private float Identify()
        {
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