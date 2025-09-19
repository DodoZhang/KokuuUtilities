using System;
using System.Text;
using UnityEngine;

namespace Kokuu.Maths
{
    public static class LinearEquations
    {
        public class SolutionSet : IFormattable
        {
            public Vector specialSolution;
            public Vector[] fundamentalSystem;
            
            public bool isEmpty => specialSolution is null;
            public bool isUnique => !isEmpty && !isInfinite;
            public bool isInfinite => fundamentalSystem.Length > 0;

            public static SolutionSet Empty => new()
            {
                specialSolution = null,
                fundamentalSystem = Array.Empty<Vector>()
            };
            
            public override string ToString() => ToString(null, null);
            public string ToString(string format) => ToString(format, null);
            public string ToString(string format, IFormatProvider formatProvider)
            {
                if (isEmpty) return "X = Empty";
                if (isUnique) return $"X = {specialSolution.ToString(format, formatProvider)}";
                
                StringBuilder builder = new();
                builder.Append($"X \t= \t{specialSolution.ToString(format, formatProvider)}");
                for (int i = 0; i < fundamentalSystem.Length; i++)
                    builder.Append($"\n\t+ k{i + 1} * \t{fundamentalSystem[i].ToString(format, formatProvider)}");
                return builder.ToString();
            }
        }

        public static SolutionSet Solve(Matrix A, Vector B)
        {
            if (A.row != B.dimension) throw new SizeMismatchException($"Dimension: {A.row}");

            int row = A.row, column = A.column;
            Matrix C = new Matrix(row, column + 1)
            {
                [.., ..^1] = A,
                [.., ^1..] = B
            };

            int[] echelon = C.RowReduce().echelon;
            
            int trace = 0;
            for (int i = 0; i < column; i++)
                if (echelon[i] != -1)
                    trace++;

            for (int i = trace; i < row; i++)
                if (!C[i, column].IsZero())
                    return SolutionSet.Empty;
            
            Vector specialSolution = new(column);
            for (int i = 0; i < column; i++)
                if (echelon[i] != -1)
                    specialSolution[i] = C[echelon[i], column];
            
            Vector[] fundamentalSystem = new Vector[column - trace];
            for (int i = 0, j = 0; j < column; j++)
            {
                if (echelon[j] != -1) continue;
                fundamentalSystem[i++] = new Vector(-C.ColumnAt(j)) { [j] = 1 };
            }

            return new SolutionSet
            {
                specialSolution = specialSolution,
                fundamentalSystem = fundamentalSystem
            };
        }
        
        public class SolutionSetC : IFormattable
        {
            public VectorC specialSolution;
            public VectorC[] fundamentalSystem;
            
            public bool isEmpty => specialSolution is null;
            public bool isUnique => !isEmpty && !isInfinite;
            public bool isInfinite => fundamentalSystem.Length > 0;

            public static SolutionSetC Empty => new()
            {
                specialSolution = null,
                fundamentalSystem = Array.Empty<VectorC>()
            };
            
            public override string ToString() => ToString(null, null);
            public string ToString(string format) => ToString(format, null);
            public string ToString(string format, IFormatProvider formatProvider)
            {
                if (isEmpty) return "X = Empty";
                if (isUnique) return $"X = {specialSolution.ToString(format, formatProvider)}";
                
                StringBuilder builder = new();
                builder.Append($"X \t= \t{specialSolution.ToString(format, formatProvider)}");
                for (int i = 0; i < fundamentalSystem.Length; i++)
                    builder.Append($"\n\t+ k{i + 1} * \t{fundamentalSystem[i].ToString(format, formatProvider)}");
                return builder.ToString();
            }
        }

        public static SolutionSetC Solve(MatrixC A, VectorC B)
        {
            if (A.row != B.dimension) throw new SizeMismatchException($"Dimension: {A.row}");

            int row = A.row, column = A.column;
            MatrixC C = new MatrixC(row, column + 1)
            {
                [.., ..^1] = A,
                [.., ^1..] = B
            };

            int[] echelon = C.RowReduce().echelon;
            
            int trace = 0;
            for (int i = 0; i < column; i++)
                if (echelon[i] != -1)
                    trace++;
            
            for (int i = trace; i < row; i++)
                if (!C[i, column].IsZero())
                    return SolutionSetC.Empty;
            
            VectorC specialSolution = new(column);
            for (int i = 0; i < column; i++)
                if (echelon[i] != -1)
                    specialSolution[i] = C[echelon[i], column];
            
            VectorC[] fundamentalSystem = new VectorC[column - trace];
            for (int i = 0, j = 0; j < column; j++)
            {
                if (echelon[j] != -1) continue;
                fundamentalSystem[i++] = new VectorC(-C.ColumnAt(j)) { [j] = 1 };
            }

            return new SolutionSetC
            {
                specialSolution = specialSolution,
                fundamentalSystem = fundamentalSystem
            };
        }
    }
}