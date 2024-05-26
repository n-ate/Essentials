namespace n_ate.Essentials
{
    public static class DateTimeExtensions
    {
        ////public static readonly DateTime UnixMinimumDate = DateTime.UnixEpoch.ToUniversalTime();
        ////public static readonly long UnixMinimumMS = UnixMinimumDate.ToUnixTimeMS();

        ////public static DateTime FromUnixTimeMS(this long value)
        ////{
        ////    if (value < UnixMinimumMS) value = UnixMinimumMS;
        ////    return DateTimeOffset.FromUnixTimeMilliseconds(value).UtcDateTime;
        ////}

        ////public static long ToUnixTimeMS(this DateTime value)
        ////{
        ////    return new DateTimeOffset(value.ToUniversalTime()).ToUnixTimeMilliseconds();
        ////}

        //public static DateTime UtcMaxValue(this DateTime value)
        //{
        //    return DateTime.Parse($"{DateTime.MaxValue.ToString("o")}Z").ToUniversalTime();
        //}

        ////public static DateTime UtcMinValue(this DateTime value)
        ////{
        ////    return DateTime.Parse($"{DateTime.MinValue.ToString("o")}Z").ToUniversalTime();
        ////}

        //public static readonly DateTime MinNumericDateTime = new(1000, 1, 1);
        //private static readonly string NumericDateTimeFormat = "yyyyMMddHHmmssffff";
        //public static long ToNumericDateTime(this DateTime date)
        //{
        //    if(date.Equals(DateTime.MaxValue) || date.Equals(DateTime.MaxValue.UtcMaxValue())) //is utc max or just max
        //    {
        //    }
        //    date = date.ToUniversalTime();
        //    if (date.Year < 1000)
        //    {
        //        date = MinNumericDateTime;
        //    }
        //    if (!long.TryParse(date.ToString(NumericDateTimeFormat), out var numericDate))
        //    {
        //        throw new ArgumentException("Must be a valid DateTime object", nameof(date));
        //    }
        //    return numericDate;
        //}
    }
}