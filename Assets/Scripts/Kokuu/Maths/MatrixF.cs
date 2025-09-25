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

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(MatrixF))]
        private class MatrixFPropertyDrawer : PropertyDrawer
        {
            private const float SizeLabelWidth = 12.5f;
            private const float SizeFieldSpacing = 4;
            private const int MaxRowCount = 10;
            private const float MinColumnWidth = 60;
            private const float MatrixFHorizontalSpacing = 2;

            private Vector2 scrollPosition;
            
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                int lineCount = property.isExpanded
                    ? 2 + Mathf.Min(property.FindPropertyRelative(nameof(_row)).intValue, MaxRowCount)
                    : 1;
                return lineCount * EditorGUIUtility.singleLineHeight +
                       (lineCount - 1) * EditorGUIUtility.standardVerticalSpacing;
            }
        
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                SerializedProperty rowProperty = property.FindPropertyRelative(nameof(_row));
                SerializedProperty columnProperty = property.FindPropertyRelative(nameof(_col));
                SerializedProperty valuesProperty = property.FindPropertyRelative(nameof(_val));
                
                int row = rowProperty.intValue;
                int column = columnProperty.intValue;
                
                Rect titlePosition = position;
                float sizeFieldWidth = EditorGUIUtility.fieldWidth + SizeLabelWidth;
                titlePosition.width -= 2 * (sizeFieldWidth + SizeFieldSpacing);
                titlePosition.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.BeginProperty(titlePosition, label, property);
                property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(titlePosition, property.isExpanded, label);

                EditorGUI.BeginChangeCheck();
                
                int indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = SizeLabelWidth;
                
                Rect sizePosition = titlePosition;
                sizePosition.x = sizePosition.xMax + SizeFieldSpacing;
                sizePosition.width = sizeFieldWidth;
                rowProperty.intValue =
                    EditorGUI.DelayedIntField(sizePosition, new GUIContent("R"), rowProperty.intValue);
                
                sizePosition.x += sizeFieldWidth + SizeFieldSpacing;
                columnProperty.intValue =
                    EditorGUI.DelayedIntField(sizePosition, new GUIContent("C"), columnProperty.intValue);
                
                EditorGUIUtility.labelWidth = labelWidth;
                EditorGUI.indentLevel = indentLevel;
                
                if (EditorGUI.EndChangeCheck())
                {
                    int newRow = rowProperty.intValue;
                    int newColumn = columnProperty.intValue;
                    
                    if (newRow != row || newColumn != column)
                    {
                        if (newRow * newColumn > row * column)
                            valuesProperty.arraySize = newRow * newColumn;

                        if (newColumn > column)
                        {
                            for (int r = Mathf.Min(row, newRow) - 1; r >= 0; r--)
                            for (int c = Mathf.Min(column, newColumn) - 1; c >= 0; c--)
                                valuesProperty.MoveArrayElement(r * column + c, r * newColumn + c);
                        }
                        else if (newColumn < column)
                        {
                            for (int r = 0, rc = Mathf.Min(row, newRow); r < rc; r++)
                            for (int c = 0, sc = Mathf.Min(column, newColumn); c < sc; c++)
                                valuesProperty.MoveArrayElement(r * column + c, r * newColumn + c);
                        }
                        
                        valuesProperty.arraySize = newRow * newColumn;
                        row = newRow;
                        column = newColumn;
                    }
                }
                
                if (property.isExpanded)
                {
                    EditorGUI.indentLevel++;

                    Rect MatrixFPosition = position;
                    MatrixFPosition.y += lineHeight;
                    MatrixFPosition.height -= lineHeight;
                    MatrixFPosition = EditorGUI.IndentedRect(MatrixFPosition);
                    
                    Rect MatrixFColumnLabelsPosition = MatrixFPosition;
                    MatrixFColumnLabelsPosition.x += lineHeight;
                    MatrixFColumnLabelsPosition.width -= lineHeight;
                    MatrixFColumnLabelsPosition.height = lineHeight;
                    
                    Rect MatrixFRowLabelsPosition = MatrixFPosition;
                    MatrixFRowLabelsPosition.width = lineHeight;
                    MatrixFRowLabelsPosition.y += lineHeight;
                    MatrixFRowLabelsPosition.height -= lineHeight;

                    Rect MatrixFValuesPosition = MatrixFPosition;
                    MatrixFValuesPosition.x += lineHeight;
                    MatrixFValuesPosition.y += lineHeight;
                    MatrixFValuesPosition.width -= lineHeight;
                    MatrixFValuesPosition.height -= lineHeight;
                    float MatrixFValuesWidth = MatrixFValuesPosition.width;
                    if (row > MaxRowCount) MatrixFValuesWidth -= GUI.skin.verticalScrollbar.fixedWidth;
                    float valueWidth = Mathf.Max((MatrixFValuesWidth + MatrixFHorizontalSpacing) / column, MinColumnWidth);

                    indentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;
                    
                    GUIStyle labelStyle = new(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
                    
                    Rect MatrixFColumnLabelsViewPosition =
                        new Rect(0, 0, column * valueWidth - MatrixFHorizontalSpacing, lineHeight);
                    Rect elementPosition =
                        new(0, 0, valueWidth - MatrixFHorizontalSpacing, EditorGUIUtility.singleLineHeight);
                    GUI.BeginScrollView(MatrixFColumnLabelsPosition, new Vector2(scrollPosition.x, 0),
                        MatrixFColumnLabelsViewPosition, GUIStyle.none, GUIStyle.none);
                    for (int c = 0; c < column; c++)
                    {
                        elementPosition.x = c * valueWidth;
                        elementPosition.y = 0;
                        if (c % 2 == 0) EditorGUI.DrawRect(elementPosition, Color.gray);
                        EditorGUI.LabelField(elementPosition, $"{c}", labelStyle);
                    }
                    GUI.EndScrollView();
                    
                    Rect MatrixFRowLabelsViewPosition =
                        new Rect(0, 0, lineHeight, row * lineHeight - EditorGUIUtility.standardVerticalSpacing);
                    elementPosition.width = EditorGUIUtility.singleLineHeight;
                    GUI.BeginScrollView(MatrixFRowLabelsPosition, new Vector2(0, scrollPosition.y),
                        MatrixFRowLabelsViewPosition, GUIStyle.none, GUIStyle.none);
                    for (int r = 0; r < row; r++)
                    {
                        elementPosition.x = 0;
                        elementPosition.y = r * lineHeight;
                        if (r % 2 == 0) EditorGUI.DrawRect(elementPosition, Color.gray);
                        EditorGUI.LabelField(elementPosition, $"{r}", labelStyle);
                    }
                    GUI.EndScrollView();
                    
                    Rect MatrixFValuesViewPosition = new Rect(0, 0, column * valueWidth - MatrixFHorizontalSpacing,
                        row * lineHeight - EditorGUIUtility.standardVerticalSpacing);
                    elementPosition.width = valueWidth - MatrixFHorizontalSpacing;
                    scrollPosition =
                        GUI.BeginScrollView(MatrixFValuesPosition, scrollPosition, MatrixFValuesViewPosition);
                    for (int r = 0; r < row; r++)
                    for (int c = 0; c < column; c++)
                    {
                        elementPosition.x = c * valueWidth;
                        elementPosition.y = r * lineHeight;
                        EditorGUI.PropertyField(elementPosition,
                            valuesProperty.GetArrayElementAtIndex(r * column + c), GUIContent.none);
                    }
                    GUI.EndScrollView();

                    EditorGUI.indentLevel = indentLevel;
                    
                    EditorGUI.indentLevel--;
                }
                
                EditorGUI.EndFoldoutHeaderGroup();
                EditorGUI.EndProperty();
            }
        }
#endif
    }
}