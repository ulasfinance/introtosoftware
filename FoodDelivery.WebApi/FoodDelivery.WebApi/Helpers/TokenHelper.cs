using System;

namespace FoodDelivery.WebApi.Helpers
{
    public static class TokenHelper
    {
        public static string GenerateFakeToken(string email)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{email}:{Guid.NewGuid()}"));
        }

        public static string? DecodeFakeToken(string token)
        {
            try
            {
                var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
                return decoded.Split(':')[0];
            }
            catch
            {
                return null;
            }
        }

        public static bool IsTokenValid(string token)
        {
            try
            {
                Convert.FromBase64String(token);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
