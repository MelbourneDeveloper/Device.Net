using System;
using System.Globalization;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var asdasd = "Hello".ContainsIgnoreCase("hella");
            Console.WriteLine("Hello World!");
        }
    }

    public static class ass
    {
        public static bool ContainsIgnoreCase(this string paragraph, string word)
        {
            return new CultureInfo("en-US").CompareInfo.IndexOf(paragraph, word, CompareOptions.IgnoreCase) >= 0;
        }

    }
}
