using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace BWJ.Core.Chronology
{
    internal class DateEval
    {
        internal DateEval(string date)
        {
            if(Regex.IsMatch(date, InterpretedDate.PossibleDateRegex) == false)
            {
                throw new ArgumentException(nameof(date));
            }

            var strComponents = date.Split('\\', '/', '-');
            DateComponents = strComponents
                .Select(str => Convert.ToInt32(str))
                .ToArray();
            DateDigitCounts = strComponents
                .Select(dte => dte.Length)
                .ToArray();
        }

        public int[] DateComponents { get; }
        public int[] DateDigitCounts { get; }

        public int? MonthIndex { get; set; }
        public int? DateIndex { get; set; }
        public int? YearIndex { get; set; }
        public int[] YearCandidates { get; set; } = Array.Empty<int>();

        public DateTime? Date { get; set; }

        public int ComponentCount { get => DateComponents.Length; }

        public InterpretedDateFormat Format
        {
            get {
                if(invalidFormat is not null)
                {
                    return invalidFormat.Value;
                }

                int format;
                if(ComponentCount == 2)
                {
                    if(MonthIndex.HasValue is false || DateIndex.HasValue is false)
                    {
                        return InterpretedDateFormat.Unspecified;
                    }

                    format = (MonthIndex.Value * 100) + (DateIndex.Value * 10);
                }
                else
                {
                    if (MonthIndex.HasValue is false || DateIndex.HasValue is false || YearIndex.HasValue is false)
                    {
                        return InterpretedDateFormat.Unspecified;
                    }

                    format = (MonthIndex.Value * 100) + (DateIndex.Value * 10) + YearIndex.Value;
                }

                return (InterpretedDateFormat)format;
            }
        }

        private InterpretedDateFormat? invalidFormat = null;
        internal void SetInvalidFormat(InterpretedDateFormat format)
        {
            var notAllowed = new InterpretedDateFormat[] {
                InterpretedDateFormat.Unspecified,
                InterpretedDateFormat.ImpliedYearMonthDate,
                InterpretedDateFormat.ImpliedYearDateMonth,
                InterpretedDateFormat.DateMonthYear,
                InterpretedDateFormat.MonthDateYear,
                InterpretedDateFormat.YearMonthDate,
                InterpretedDateFormat.YearDateMonth
            };
            if(notAllowed.Contains(format))
            {
                throw new ArgumentException(nameof(format));
            }

            invalidFormat = format;
        }

        public DateInterpretationConfidence Confidence { get; set; }
    }
}
