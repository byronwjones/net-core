using System;

namespace BWJ.Core.Chronology
{
    public sealed class DateTimeService : IDateTimeService
    {
        public DateTime GetCurrentTimeUtc() => DateTime.UtcNow;
        public DateTime GetCurrentTimeLocal() => DateTime.Now;
    }
}
