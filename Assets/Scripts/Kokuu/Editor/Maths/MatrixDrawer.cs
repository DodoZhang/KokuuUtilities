using UnityEditor;
using UnityEngine;

namespace Kokuu.Maths
{
    [CustomPropertyDrawer(typeof(Matrix))]
    public class MatrixDrawer : PropertyDrawer
    {
        private const string RowPropertyName = "_row";
        private const string ColumnPropertyName = "_col";
        private const string ValuesPropertyName = "_val";
        
        private const float SizeLabelWidth = 12.5f;
        private const float SizeFieldSpacing = 4;
        private const int MaxRowCount = 10;
        protected virtual float MinColumnWidth => 30;
        private const float MatrixHorizontalSpacing = 2;

        private Vector2 scrollPosition;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lineCount = property.isExpanded
                ? 2 + Mathf.Min(property.FindPropertyRelative(RowPropertyName).intValue, MaxRowCount)
                : 1;
            return lineCount * EditorGUIUtility.singleLineHeight +
                   (lineCount - 1) * EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            SerializedProperty rowProperty = property.FindPropertyRelative(RowPropertyName);
            SerializedProperty columnProperty = property.FindPropertyRelative(ColumnPropertyName);
            SerializedProperty valuesProperty = property.FindPropertyRelative(ValuesPropertyName);

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

                Rect matrixPosition = position;
                matrixPosition.y += lineHeight;
                matrixPosition.height -= lineHeight;
                matrixPosition = EditorGUI.IndentedRect(matrixPosition);

                Rect matrixColumnLabelsPosition = matrixPosition;
                matrixColumnLabelsPosition.x += lineHeight;
                matrixColumnLabelsPosition.width -= lineHeight;
                matrixColumnLabelsPosition.height = lineHeight;

                Rect matrixRowLabelsPosition = matrixPosition;
                matrixRowLabelsPosition.width = lineHeight;
                matrixRowLabelsPosition.y += lineHeight;
                matrixRowLabelsPosition.height -= lineHeight;

                Rect matrixValuesPosition = matrixPosition;
                matrixValuesPosition.x += lineHeight;
                matrixValuesPosition.y += lineHeight;
                matrixValuesPosition.width -= lineHeight;
                matrixValuesPosition.height -= lineHeight;
                float matrixValuesWidth = matrixValuesPosition.width;
                if (row > MaxRowCount) matrixValuesWidth -= GUI.skin.verticalScrollbar.fixedWidth;
                float valueWidth = Mathf.Max((matrixValuesWidth + MatrixHorizontalSpacing) / column, MinColumnWidth);

                indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                GUIStyle labelStyle = new(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

                Rect matrixColumnLabelsViewPosition =
                    new Rect(0, 0, column * valueWidth - MatrixHorizontalSpacing, lineHeight);
                Rect elementPosition =
                    new(0, 0, valueWidth - MatrixHorizontalSpacing, EditorGUIUtility.singleLineHeight);
                GUI.BeginScrollView(matrixColumnLabelsPosition, new Vector2(scrollPosition.x, 0),
                    matrixColumnLabelsViewPosition, GUIStyle.none, GUIStyle.none);
                for (int c = 0; c < column; c++)
                {
                    elementPosition.x = c * valueWidth;
                    elementPosition.y = 0;
                    if (c % 2 == 0) EditorGUI.DrawRect(elementPosition, Color.gray);
                    EditorGUI.LabelField(elementPosition, $"{c}", labelStyle);
                }

                GUI.EndScrollView();

                Rect matrixRowLabelsViewPosition =
                    new Rect(0, 0, lineHeight, row * lineHeight - EditorGUIUtility.standardVerticalSpacing);
                elementPosition.width = EditorGUIUtility.singleLineHeight;
                GUI.BeginScrollView(matrixRowLabelsPosition, new Vector2(0, scrollPosition.y),
                    matrixRowLabelsViewPosition, GUIStyle.none, GUIStyle.none);
                for (int r = 0; r < row; r++)
                {
                    elementPosition.x = 0;
                    elementPosition.y = r * lineHeight;
                    if (r % 2 == 0) EditorGUI.DrawRect(elementPosition, Color.gray);
                    EditorGUI.LabelField(elementPosition, $"{r}", labelStyle);
                }

                GUI.EndScrollView();

                Rect matrixValuesViewPosition = new Rect(0, 0, column * valueWidth - MatrixHorizontalSpacing,
                    row * lineHeight - EditorGUIUtility.standardVerticalSpacing);
                elementPosition.width = valueWidth - MatrixHorizontalSpacing;
                scrollPosition =
                    GUI.BeginScrollView(matrixValuesPosition, scrollPosition, matrixValuesViewPosition);
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
    
    [CustomPropertyDrawer(typeof(MatrixC))]
    [CustomPropertyDrawer(typeof(MatrixF))]
    public class MatrixCDrawer : MatrixDrawer
    {
        protected override float MinColumnWidth => 60;
    }
}