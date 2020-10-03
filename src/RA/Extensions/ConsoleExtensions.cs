using System;
using System.Collections.Generic;

namespace RA.Extensions
{
    public static class EnumerationExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action.Invoke(item);
            }
        } 
    }

    public static class ConsoleExtensions
    {
        public static void WriteHeader(this string source, params object[] objects)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(source, objects);
            Console.ResetColor();
        }

        public static void WriteLine(this string source, params object[] objects)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("- " + source, objects);
            Console.ResetColor();
        }

        public static void Write(this string source, params object[] objects)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(source, objects);
            Console.ResetColor();
        }
    }
}