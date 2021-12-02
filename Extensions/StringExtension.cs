using System.Text.RegularExpressions;

namespace TaskPlusPlus.API.Extensions
{
    public static class StringExtension
    {
        private static readonly string regex = @"[0][9][0,1,2,3,4,5]+[0-9]";
        public static bool IsValidPhoneNumber(this string number)
        {
            if (string.IsNullOrEmpty(number)) return false;
            try
            {
                return Regex.Match(number, regex).Success && number.Length == 11;
            }
            catch
            {
                return false;
            }
        }
    }
}
