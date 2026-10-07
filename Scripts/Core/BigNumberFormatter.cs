using System;

namespace BloodSeal.Core
{
    public static class BigNumberFormatter
    {
        private static readonly string[] Suffixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };

        public static string Format(double num)
        {
            if (num < 0) return "0";
            if (num < 1000.0) return Math.Floor(num).ToString("0");

            int exp = (int)(Math.Log10(num) / 3);
            if (exp >= Suffixes.Length)
            {
                return num.ToString("0.##e+0");
            }

            double divisor = Math.Pow(10, exp * 3);
            double formatted = num / divisor;

            // Handle edge case where rounding like 999.996 rounds up to 1000.0K instead of 1.0M
            if (Math.Round(formatted, 2) >= 1000.0 && exp + 1 < Suffixes.Length)
            {
                formatted /= 1000.0;
                exp++;
            }

            return $"{formatted:0.##}{Suffixes[exp]}";
        }
    }
}
