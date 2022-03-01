using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTool.File
{
    public class CSVHandler
    {
        // Public Property
        public enum SeparationType
        {
            Comma,
            Tab
        }

        public List<string[]> Matrix
        {
            get { return _getMatrix; }
            set { _setMatrix = value; }
        }

        // Private Const Field
        private const char CommaSep = ',';
        private const char TabSep = '\t';

        // Private Field
        private char _sepChar;
        private List<string[]> _getMatrix = new List<string[]>();
        private List<string[]> _setMatrix = new List<string[]>();

        public CSVHandler(SeparationType type = SeparationType.Comma)
        {
            switch (type)
            {
                case SeparationType.Comma:
                    _sepChar = CommaSep;
                    break;
                case SeparationType.Tab:
                    _sepChar = TabSep;
                    break;
                default:
                    _sepChar = CommaSep;
                    break;
            }
        }

        protected List<string[]> readMatrixFromFile(string filePath)
        {
            List<string[]> matrix = new List<string[]>();

            using (var reader = new StreamReader(filePath))
            {
                matrix = new List<string[]>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(_sepChar);
                    matrix.Add(values);
                }
            }

            _getMatrix = matrix;
            return matrix;
        }

        protected void writeMatrixToFile(string filePath, bool append = false)
        {
            var matrix = _setMatrix;

            // Create CSV directory if not exist
            var folderPath = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(folderPath);

            // Write to file
            using (StreamWriter stream = new StreamWriter(filePath, append))
            {
                for (int i = 0; i < matrix.Count; i++)
                {
                    stream.WriteLine(convertArrayToString(matrix[i]));
                }
            }

            _getMatrix = matrix;
        }

        protected static string[] getColStringArray(List<string[]> matrix, string header)
        {
            int rowLength = matrix.Count;
            int colLength = matrix[0].Length;
            int targetIndex = -1;

            for (int i = 0; i < colLength; i++)
            {
                if (matrix[0][i] == header)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex < 0)
                throw new Exception("Fail to find header = " + header);

            string[] result = new string[rowLength - 1];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = matrix[i + 1][targetIndex];
            }
            return result;
        }

        private string convertArrayToString(string[] input)
        {
            string output = "";
            for (int i = 0; i < input.Length; i++)
            {
                if (i == 0)
                    output += input[i];
                else
                    output += _sepChar + input[i];
            }

            return output;
        }
    }
}
