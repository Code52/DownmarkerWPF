using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkPad.Framework
{
    public static class MarkdownHelpers
    {
        private const string CodeDelimiter = "`";
        private const string BoldDelimiter = "**";
        private const string ItalicDelimiter = "*";

        public static bool IsItalic(this string inputString)
        {
            var leadingCount = inputString
                .TakeWhile(c => c.ToString() == ItalicDelimiter)
                .Count();
            var trailingCount = inputString
                .Reverse()
                .TakeWhile(c => c.ToString() == ItalicDelimiter)
                .Count();

            if (leadingCount == 1 && trailingCount == 1)
                return true;

            return (leadingCount == 3 && trailingCount == 3);
        }

        public static bool IsBold(this string inputString)
        {
            return inputString.StartsWith(BoldDelimiter)
                   && inputString.EndsWith(BoldDelimiter);
        }

        public static bool IsCode(this string inputString)
        {
            return inputString.StartsWith(CodeDelimiter)
                   && inputString.EndsWith(CodeDelimiter);
        }


        public static string ToggleItalic(this string inputString, bool makeItalic)
        {
            return inputString.ToggleDelimiter(ItalicDelimiter, makeItalic);
        }

        public static string ToggleBold(this string inputString, bool makeBold)
        {
            return inputString.ToggleDelimiter(BoldDelimiter, makeBold);
        }

        public static string ToggleCode(this string inputString, bool makeCode)
        {
            return inputString.ToggleDelimiter(CodeDelimiter, makeCode);
        }

        private static string ToggleDelimiter(this string inputString, string delimiter, bool turnOn)
        {
            if (turnOn)
                return delimiter + inputString + delimiter;

            var delimLength = delimiter.Length;
            var length = inputString.Length;

            return inputString
                .Substring(delimLength, length - (2 * delimLength));
        }
    }
}
