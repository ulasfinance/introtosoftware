using System;

namespace FoodDelivery.WebApi.Utils
{
    public static class ValidationHelper
    {
        // MAIN COMMIT #3: added basic order validation logic
        public static bool IsValidDeliveryTime(DateTime deliveryTime)
        {
            // Delivery must be at least 30 minutes in the future
            return deliveryTime > DateTime.Now.AddMinutes(30);
        }

        public static bool IsPhoneNumberValid(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\\+7 \\(\\d{3}\\) \\d{3}-\\d{2}-\\d{2}$");
        }
    }
}
