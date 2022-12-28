using System;

namespace BWJ.Core.Chronology
{
    public static class NumericalDateTimeExtensions
    {
        private static DateTime UnixEpoch = new DateTime(1970, 1, 1);
        private static long MAX_VALUE = 253402300800000L;
        private static long MIN_VALUE = -62135596800000L;

        /// <summary>
        /// Returns a value equivalent to the given DateTime, represented as the number of milliseconds since January 1, 1970 (UTC)
        /// This method assumes that the provided DateTime value is expressed in UTC.
        /// </summary>
        public static long ToLong(this DateTime date)
            => (long)(date - UnixEpoch).TotalMilliseconds;

        /// <summary>
        /// Returns a DateTime value (in UTC) for the given long integer, evaluating this value as the
        /// number of as the number of milliseconds since January 1, 1970 (UTC).
        /// This method will not throw an exception if the value given represents a date/time that cannot
        /// be represented using System.DateTime.  Instead, it will truncate the given value to DateTime.MinValue
        /// or DateTime.MaxValue
        /// </summary>
        public static DateTime ToDateTime(this long date)
        {
            if(date > MAX_VALUE) { return DateTime.MaxValue; }
            if(date < MIN_VALUE) { return DateTime.MinValue; }

            return UnixEpoch.AddMilliseconds(date);
        }
    }
}
