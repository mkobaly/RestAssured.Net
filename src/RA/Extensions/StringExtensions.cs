﻿namespace RA.Extensions
{
    public static class StringExtensions
    {
        //public static bool IsEmpty(this string source)
        //{
        //    return string.IsNullOrEmpty(source);
        //}

        public static bool IsNotEmpty(this string source)
        {
            return !string.IsNullOrEmpty(source);
        }

        public static string Quote(this string source)
        {
            return "\"" + source + "\"";
        }
    }
}
