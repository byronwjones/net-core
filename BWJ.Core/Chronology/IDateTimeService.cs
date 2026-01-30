using System;

namespace BWJ.Core.Chronology
{
    public interface IDateTimeService
    {
        DateTime GetCurrentTimeLocal();
        DateTime GetCurrentTimeUtc();
    }
}