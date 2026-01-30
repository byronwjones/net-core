namespace BWJ.Core.Chronology
{
    public enum InterpretedDateFormat
    {
        // month | date | year
        Unspecified = 0,
        BelowMinimumThreshold = 1,
        AboveMaximumThreshold = 2,
        OutOfRange = 3,
        InvalidDate = 4,

        // 3-component values are computed as:
        // ([index of month] * 100) + ([index of date] * 10) + [index of year]
        // 2-component values are computed as:
        // ([index of month] * 100) + ([index of date] * 10)
        ImpliedYearMonthDate = 10,
        MonthDateYear = 12,
        ImpliedYearDateMonth = 100,
        DateMonthYear = 102,
        YearMonthDate = 120,
        YearDateMonth = 210,
    }
}
