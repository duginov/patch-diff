using System.Globalization;

namespace Patch
{
    public record MasterKey
    {
        public string Key { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }

        public MasterKey(string key, string startDate, string endDate)
        {
            Key = key;
            StartDate = ParseDateOrDefault(startDate, DateTime.MinValue, nameof(startDate));
            EndDate = ParseDateOrDefault(endDate, DateTime.MaxValue, nameof(endDate));
        }

        private static DateTime ParseDateOrDefault(string dateString, DateTime defaultValue, string paramName)
        {
            if (string.IsNullOrEmpty(dateString))
            {
                return defaultValue;
            }

            if (!TryParseDate(dateString, out var parsedDate))
            {
                throw new ArgumentException($"Invalid date: {dateString}", paramName);
            }

            return parsedDate;
        }

        public override string ToString()
        {
            if (StartDate == DateTime.MinValue && EndDate == DateTime.MaxValue)
            {
                return $"{Key}, for all dates";
            }
            else if (StartDate == DateTime.MinValue)
            {
                return $"{Key}, for all dates before {EndDate:MM/dd/yyyy}";
            }
            else if (EndDate == DateTime.MaxValue)
            {
                return $"{Key}, for all dates after {StartDate:MM/dd/yyyy}";
            }
            else
            {
                return $"{Key}, for dates between {StartDate:MM/dd/yyyy} and {EndDate:MM/dd/yyyy}";
            }
        }

        // allows to parse DateTime strings with standard and non-standard formats
        public static bool TryParseDate(string dateString, out DateTime parsedDate)
        {
            if (DateTime.TryParse(dateString, out parsedDate))
            {
                return true;
            }

            // if standard TryParse fails we can use some custom formats
            // IRL they should be in a config file, not hardcoded, to help quick additions
            var customFormats = new List<string>
            {
                "yyyyMMdd",      
                "yyyy MM dd",    
            };

            // Try custom formats if TryParse failed
            foreach (var format in customFormats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                {
                    return true;
                }
            }

            // If all parsing attempts fail
            return false;
        }
    }
}
