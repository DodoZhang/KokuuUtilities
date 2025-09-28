using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Kokuu.Maths
{
    [Serializable]
    public class MatrixF : IEquatable<MatrixF>, IFormattable, ISerializationCallbackReceiver
    {
        [SerializeField] private int _row, _col;
        [SerializeField] private Fractional[] _val;

        public MatrixF(int row, int column, IEnumerable<Fractional> values = null)
        {
            if (row <= 0) throw new ArgumentOutOfRangeException(nameof(row));
            if (column <= 0) throw new ArgumentOutOfRangeException(nameof(column));
            
            _row = row;
            _col = column;
            _val = new Fractional[_row * _col];

            for (int i = 0; i < _val.Length; i++) _val[i] = 0;
            if (values is not null) Set(values);
        }
        public MatrixF(MatrixF mat)
        {
            _row = mat._row;
            _col = mat._col;
            _val = new Fractional[_row * _col];
            for (int i = 0, s = _row * _col; i < s; i++)
                _val[i] = mat._val[i];
        }
        public static MatrixF Identity(int row)
        {
            MatrixF mat = new(row, row);
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
                Fractional[] val = new Fractional[value * _col];
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
                Fractional[] val = new Fractional[_row * value];
                for (int r = 0; r < _row; r++)
                for (int c = 0; c < Math.Min(value, _col); c++)
                    val[r * value + c] = _val[r * _col + c];
                _val = val;
                _col = value;
            }
        }

        public Fractional this[int r, int c]
        {
            get => _val[r * _col + c];
            set => _val[r * _col + c] = value;
        }
        public Fractional this[Index r, Index c]
        {
            get => _val[r.GetOffset(_row) * _col + c.GetOffset(_col)];
            set => _val[r.GetOffset(_row) * _col + c.GetOffset(_col)] = value;
        }
        public MatrixF this[Range r, Range c]
        {
            get
            {
                (int offsetR, int lengthR) = r.GetOffsetAndLength(_row);
                (int offsetC, int lengthC) = c.GetOffsetAndLength(_col);
                MatrixF mat = new MatrixF(lengthR, lengthC);
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
                    throw new SizeMismatchException($"Size: {lengthR} * {lengthC}");
                for (int i = 0; i < lengthR; i++)
                for (int j = 0; j < lengthC; j++)
                    this[offsetR + i, offsetC + j] = value[i, j];
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

        public bool Equals(MatrixF other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (_row != other._row || _col != other._col) return false;
            for (int i = 0, s = _row * _col; i < s; i++)
                if (!_val[i].Equals(other._val[i])) return false;
            return true;
        }
        
        public static MatrixF operator +(MatrixF a, MatrixF b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (a._row != b._row || a._col != b._col)
                throw new SizeMismatchException($"Size: {a._row} * {a._col}");

            MatrixF result = new MatrixF(a._row, a._col);
            for (int i = 0, s = a._row * a._col; i < s; i++)
                result._val[i] = a._val[i] + b._val[i];
            return result;
        }
        public static MatrixF operator -(MatrixF a, MatrixF b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (a._row != b._row || a._col != b._col)
                throw new SizeMismatchException($"Size: {a._row} * {a._col}");

            MatrixF result = new MatrixF(a._row, a._col);
            for (int i = 0, s = a._row * a._col; i < s; i++)
                result._val[i] = a._val[i] - b._val[i];
            return result;
        }
        public static MatrixF operator *(MatrixF a, MatrixF b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (a._col != b._row) throw new SizeMismatchException($"Row Counts: {a._col}");

            MatrixF result = new MatrixF(a._row, b._col);
            for (int i = 0; i < a._row; i++)
            for (int j = 0; j < b._col; j++)
            {
                Fractional v = 0;
                for (int k = 0; k < a._col; k++)
                    v += a[i, k] * b[k, j];
                result[i, j] = v;
            }
            return result;
        }
        public static MatrixF operator *(MatrixF a, Fractional d)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            
            MatrixF result = new MatrixF(a._row, a._col);
            for (int i = 0, s = a._row * a._col; i < s; i++)
                result._val[i] = d * a._val[i];
            return result;
        }
        public static MatrixF operator *(Fractional d, MatrixF a)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            
            MatrixF result = new MatrixF(a._row, a._col);
            for (int i = 0, s = a._row * a._col; i < s; i++)
                result._val[i] = d * a._val[i];
            return result;
        }
        public static MatrixF operator /(MatrixF a, Fractional d)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            
            MatrixF result = new MatrixF(a._row, a._col);
            for (int i = 0, s = a._row * a._col; i < s; i++)
                result._val[i] = a._val[i] / d;
            return result;
        }
        public static MatrixF operator -(MatrixF a)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));

            MatrixF result = new MatrixF(a._row, a._col);
            for (int i = 0, s = a._row * a._col; i < s; i++)
                result._val[i] = -a._val[i];
            return result;
        }

        public static MatrixF Pow(MatrixF m, int p)
        {
            if (m is null) throw new ArgumentNullException(nameof(m));
            if (m.row != m.column)
                throw new SizeMismatchException($"Row Counts({m.row}) Equals to Column Counts({m.column})");

            if (p < 0)
            {
                m = m.inversed;
                p = -p;
                if (m is null) return null;
            }

            MatrixF result = Identity(m.row);
            MatrixF temp = new MatrixF(m);
            
            for (; p > 0; p /= 2)
            {
                if (p % 2 == 1) result *= temp;
                temp *= temp;
            }

            return result;
        }

        public MatrixF transposed
        {
            get
            {
                MatrixF mat = new(column, row);
                for (int r = 0; r < row; r++)
                for (int c = 0; c < column; c++)
                    mat[c, r] = this[r, c];
                return mat;
            }
        }
        
        public Fractional determinant
        {
            get
            {
                if (row != column)
                    throw new SizeMismatchException($"Row Counts({row}) Equals to Column Counts({column})");

                return new MatrixF(this).RowReduce().determinant;
            }
        }

        public MatrixF inversed
        {
            get
            {
                if (row != column)
                    throw new SizeMismatchException($"Row Counts({row}) Equals to Column Counts({column})");
                
                MatrixF mat = new(row, row * 2)
                {
                    [.., ..row] = this,
                    [.., row..] = Identity(row)
                };
                return mat.RowReduce().determinant.isZero ? null : mat[.., row..];
            }
        }

        public int trace => new MatrixF(this).RowReduce().trace;

        public (Fractional determinant, int trace, int[] echelon) RowReduce()
        {
            int i, j;
            Fractional det = Fractional.One;
            int[] echelon = new int[column];
            
            for (i = 0, j = 0; i < row && j < column; i++, j++)
            {
                while (j < column && this[i, j].isZero)
                {
                    for (int k = i + 1; k < row; k++)
                    {
                        if (this[k, j].isZero) continue;
                        SwapRows(k, i);
                        det = -det;
                        break;
                    }

                    if (!this[i, j].isZero) continue;
                    det = Fractional.Zero;
                    echelon[j++] = -1;
                }
                
                if (j >= column) break;

                Fractional s = this[i, j];
                ScaleRow(i, Fractional.One / s);
                det *= s;
                
                for (int k = 0; k < row; k++)
                {
                    if (k == i) continue;
                    AddRow(k, -this[k, j], i);
                }

                echelon[j] = i;
            }
            
            for (; j < column; j++) echelon[j] = -1;
            
            return (det, i, echelon);

            void SwapRows(int r1, int r2)
            {
                for (int c = 0; c < column; c++)
                    (this[r1, c], this[r2, c]) = (this[r2, c], this[r1, c]);
            }
            void ScaleRow(int r, Fractional s)
            {
                for (int c = 0; c < column; c++) this[r, c] *= s;
            }
            void AddRow(int r1, Fractional s, int r2)
            {
                for (int c = 0; c < column; c++)
                    this[r1, c] += s * this[r2, c];
            }
        }

        public static implicit operator MatrixF(Matrix mat)
        {
            MatrixF result = new MatrixF(mat.row, mat.column);
            for (int r = 0; r < mat.row; r++)
            for (int c = 0; c < mat.column; c++)
                result[r, c] = (Fractional)mat[r, c];
            return result;
        }
        public static explicit operator Matrix(MatrixF mat)
        {
            Matrix result = new Matrix(mat.row, mat.column);
            for (int r = 0; r < mat.row; r++)
            for (int c = 0; c < mat.column; c++)
                result[r, c] = (float)mat[r, c];
            return result;
        }
        
        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if (_row <= 0) _row = 1;
            if (_col <= 0) _col = 1;
            
            if (_val is not null && _val.Length == _row * _col) return;
            _val = new Fractional[_row * _col];
            for (int i = 0, s = _row * _col; i < s; i++)
                _val[i] = 0;
        }
    }
}