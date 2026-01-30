using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BWJ.Core.Chronology
{
    public class InterpretedDate
    {
        private InterpretedDate(DateTime date, InterpretedDateFormat format, DateInterpretationConfidence confidence)
        {
            Date = date;
            Format = format;
            Confidence = confidence;
        }

        public DateTime Date { get; }
        public InterpretedDateFormat Format { get; }
        public DateInterpretationConfidence Confidence { get; }

        public static InterpretedDate FromString(string date, DateTime contextDate, DateTime minResult, DateTime maxResult)
        {
            if(string.IsNullOrWhiteSpace(date))
            {
                return GetInvalidDateInstance();
            }

            // TODO: context date must be between min/max dates
            // TODO: min date <= max date

            var evaluator = GetDateEval(date);
            if(evaluator is null)
            {
                return GetInvalidDateInstance();
            }

            // zeroes and 3-digit numbers not allowed
            if (evaluator.DateComponents.Any(x => x == 0) || evaluator.DateDigitCounts.Any(x => x == 3))
            {
                return GetInvalidDateInstance();
            }
            
            if (evaluator.ComponentCount == 3)
            {
                // year is the only value that may contain 4 digits, and must be at either the 1st or 3rd position.
                // therefore, 2 4-digit values are not allowed
                // Also, 2-character years are allowed, but not single character years.  Therefore, two 1-character values at the
                // 1st and 3rd positions are not allowed
                if ((evaluator.DateDigitCounts[0] == 4 && evaluator.DateDigitCounts[2] == 4)
                    || (evaluator.DateDigitCounts[0] == 1 && evaluator.DateDigitCounts[2] == 1))
                {
                    return GetInvalidDateInstance();
                }

                DetermineYear(evaluator, contextDate, minResult, maxResult);
                DetermineMonthAndDay(evaluator, contextDate, minResult, maxResult);

                if (evaluator.Format == InterpretedDateFormat.InvalidDate)
                {
                    return GetInvalidDateInstance();
                }

                return new InterpretedDate(evaluator.Date.GetValueOrDefault(), evaluator.Format, evaluator.Confidence);
            }


            return new InterpretedDate(DateTime.Now, InterpretedDateFormat.InvalidDate, DateInterpretationConfidence.None);
        }

        private static void DetermineYear(DateEval evaluator, DateTime contextDate, DateTime minResult, DateTime maxResult)
        {
            // year must be at 1st or 3rd position.  If a 4-digit number is in either place, this is the year's position
            var yearIndices = new int[] { 0, 2 };
            foreach(var i in yearIndices)
            {
                if (evaluator.DateDigitCounts[i] == 4)
                {
                    evaluator.YearIndex = i;
                    evaluator.YearCandidates = new int[] { evaluator.DateComponents[i] };
                    evaluator.Confidence = DateInterpretationConfidence.High;

                    return;
                }
            }

            // 2-character year

            // if the 1st or 3rd position has one digit, the other position is the year (this is why right side of || clause is below)
            if (evaluator.DateDigitCounts[0] == 1)
            {
                ManageSinglePossibleYearIndex(2, evaluator, minResult, maxResult);
                return;
            }
            // if the 1st and 3rd position are equal, we'll call the 1st position the year
            if (evaluator.DateComponents[0] == evaluator.DateComponents[2] ||
                evaluator.DateDigitCounts[2] == 1)
            {
                ManageSinglePossibleYearIndex(0, evaluator, minResult, maxResult);
                return;
            }

            // both 1st and 3rd position have two digits

            // either the 1st or 3rd position must be plausible as a date for this to be valid
            if (evaluator.DateComponents[0] > 31 && evaluator.DateComponents[2] > 31)
            {
                evaluator.YearIndex = 0; // it doesn't matter
                evaluator.SetInvalidFormat(InterpretedDateFormat.InvalidDate);
                evaluator.Confidence = DateInterpretationConfidence.None;
            }

            // if the 1st position can't possibly be a date, it has to be the year / vice versa
            foreach (var i in yearIndices)
            {
                if (evaluator.DateComponents[i] > 31)
                {
                    ManageSinglePossibleYearIndex(i, evaluator, minResult, maxResult);
                    return;
                }
            }

            // the position matching the 2-digit context date's year is given preference, if it exists
            var contextYY = contextDate.Year % 100;
            foreach (var i in yearIndices)
            {
                if (evaluator.DateComponents[i] == contextYY)
                {
                    ManageChosenPossibleYearIndex(i, evaluator, minResult, maxResult);
                    return;
                }
            }

            // get possible year values for both positions
            var possibleYears = new int[][] {
                    GetPlausibleYears(evaluator.DateComponents[0], minResult, maxResult),
                    new int[0], // to fill in index 1 (not used)
                    GetPlausibleYears(evaluator.DateComponents[2], minResult, maxResult)
                };
            // if the 1st and 3rd positions offer no possible matches, this date is invalid
            if (possibleYears.Any(x => x.Length > 0) == false)
            {
                evaluator.YearIndex = 0; // it doesn't matter if this is not the author's intent
                evaluator.YearCandidates =
                    GetPlausibleYears(evaluator.DateComponents[0], DateTime.MinValue, DateTime.MaxValue); // so we can form some date, valid or not
                evaluator.SetInvalidFormat(InterpretedDateFormat.OutOfRange);
                evaluator.Confidence = DateInterpretationConfidence.None;
                return;
            }

            // if the 1st position has no matches, we go with the 3rd / vice versa
            if (possibleYears[0].Length == 0)
            {
                ManageChosenPossibleYearIndex(2, evaluator, minResult, maxResult);
                return;
            }
            if (possibleYears[2].Length == 0)
            {
                ManageChosenPossibleYearIndex(0, evaluator, minResult, maxResult);
                return;
            }

            // two sets of plausible years here, go with the one closest to context date -- low confidence
            var possibleYearContextDeltas = new int[][] {
                    possibleYears[0].Select(x => Math.Abs(contextDate.Year - x)).ToArray(),
                    possibleYears[2].Select(x => Math.Abs(contextDate.Year - x)).ToArray()
            };
            var minDeltaLeft = possibleYearContextDeltas[0].Min();
            var minDeltaRight = possibleYearContextDeltas[1].Min();
            var possibleYearIndex = 0;

            // possible years are equally close -- pick the left side, no confidence
            if (minDeltaRight == minDeltaLeft)
            {
                evaluator.YearIndex = 0;
                evaluator.YearCandidates = possibleYears[0];
                evaluator.Confidence = DateInterpretationConfidence.None;
                return;
            }
            if (minDeltaRight < minDeltaLeft)
            {
                possibleYearIndex = 2;
            }
            
            evaluator.YearIndex = possibleYearIndex;
            evaluator.YearCandidates = possibleYears[possibleYearIndex];
            evaluator.Confidence = evaluator.YearCandidates.Length == 1 ?
                    DateInterpretationConfidence.Low : DateInterpretationConfidence.None;
        }

        private static void DetermineMonthAndDay(DateEval evaluator, DateTime contextDate, DateTime minResult, DateTime maxResult)
        {
            // note the month/day indices.  Year must be either the first or last date component, if present
            var mdIndices = evaluator.YearIndex.HasValue == false || evaluator.YearIndex.Value == 2
                ? new int[] { 0, 1 } : new int[] { 1, 2 };

            // if the year was out of range, the date is a wild guess, since its irrelevant
            if (evaluator.Format == InterpretedDateFormat.OutOfRange)
            {
                evaluator.MonthIndex = mdIndices[0];
                evaluator.DateIndex = mdIndices[1];
                try
                {
                    evaluator.Date = new DateTime(
                        evaluator.YearCandidates[0],
                        evaluator.DateComponents[evaluator.MonthIndex!.Value],
                        evaluator.DateComponents[evaluator.DateIndex!.Value]);
                }
                catch {
                    evaluator.SetInvalidFormat(InterpretedDateFormat.InvalidDate);
                }
                return;
            }

            // invalid date if neither value can be a month,
            // or if either can't be a date
            if ((evaluator.DateComponents[mdIndices[0]] > 12 && evaluator.DateComponents[mdIndices[1]] > 12)
                || mdIndices.Any(x => evaluator.DateComponents[x] > 31))
            {
                evaluator.MonthIndex = mdIndices[0];
                evaluator.DateIndex = mdIndices[1];
                evaluator.Confidence = DateInterpretationConfidence.None;
                evaluator.SetInvalidFormat(InterpretedDateFormat.InvalidDate);
                return;
            }
            
            DetermineDateWithKnownMonth(mdIndices, evaluator, contextDate, minResult, maxResult);

            if(evaluator.Date is null)
            {
                DetermineDateWithAmbiguousMonth(mdIndices, evaluator, contextDate, minResult, maxResult);
            }
        }

        private static void DetermineDateWithKnownMonth(int[] mdIndices, DateEval evaluator, DateTime contextDate, DateTime minResult, DateTime maxResult)
        {
            // if one value can't be a month, the other value is the month, and vice-versa
            if (evaluator.DateComponents[mdIndices[0]] > 12)
            {
                evaluator.MonthIndex = mdIndices[1];
                evaluator.DateIndex = mdIndices[0];
            }
            // also, when month and date are equal values, use the first index as month -- in this case which is which doesn't matter
            else if (evaluator.DateComponents[mdIndices[1]] > 12
                || evaluator.DateComponents[mdIndices[0]] == evaluator.DateComponents[mdIndices[1]])
            {
                evaluator.MonthIndex = mdIndices[0];
                evaluator.DateIndex = mdIndices[1];
            }
            // neither month/date value can be clearly identified
            else
            {
                return;
            }

            // get date candidates based on year
            var dateCandidates = new List<DateTime>();
            foreach (var yc in evaluator.YearCandidates)
            {
                try
                {
                    var dc = new DateTime(yc,
                        evaluator.DateComponents[evaluator.MonthIndex!.Value],
                        evaluator.DateComponents[evaluator.DateIndex!.Value]);

                    dateCandidates.Add(dc);
                }
                catch { }
            }

            // no valid date can be derived
            if(dateCandidates.Any() == false)
            {
                evaluator.SetInvalidFormat(InterpretedDateFormat.InvalidDate);
                evaluator.Confidence = DateInterpretationConfidence.None;
                return;
            }

            var mdConfidence = GetMonthDateConfidenceScale(evaluator);

            if(dateCandidates.Count == 1)
            {
                var dc = dateCandidates[0];
                evaluator.Confidence = mdConfidence[DateInterpretationConfidence.High];
                evaluator.Date = dc;
                if(dc < minResult)
                {
                    evaluator.SetInvalidFormat(InterpretedDateFormat.BelowMinimumThreshold);
                }
                else if(dc > maxResult)
                {
                    evaluator.SetInvalidFormat(InterpretedDateFormat.AboveMaximumThreshold);
                }

                return;
            }

            // when there are multiple date candidates, we take the one closest to the context date
            var dcDeltas = dateCandidates.Select(x => Math.Abs((contextDate - x).TotalDays)).ToList();
            var minDelta = dcDeltas.Min();
            if(dcDeltas.Where(x => x == minDelta).Count() == 1)
            {
                evaluator.Date = dateCandidates[dcDeltas.IndexOf(minDelta)];
                evaluator.Confidence = mdConfidence[DateInterpretationConfidence.Medium];
                return;
            }

            // there are multiple candidates equidistant to the context date; we'll just use the first one -- blind guess, no confidence
            evaluator.Date = dateCandidates.First(x => Math.Abs((contextDate - x).TotalDays) == minDelta);
            evaluator.Confidence = DateInterpretationConfidence.None;
        }

        private static void DetermineDateWithAmbiguousMonth(int[] mdIndices, DateEval evaluator, DateTime contextDate, DateTime minResult, DateTime maxResult)
        {
            var mdConfidence = GetMonthDateConfidenceScale(evaluator);

            // get each date candidate alternative based on year
            var dateCandidates = new List<DateTime>();
            foreach (var yc in evaluator.YearCandidates)
            {
                var dc = new DateTime(yc,
                        evaluator.DateComponents[mdIndices[0]],
                        evaluator.DateComponents[mdIndices[1]]);
                if(DateInRange(dc, minResult, maxResult))
                {
                    dateCandidates.Add(dc);
                }

                dc = new DateTime(yc,
                        evaluator.DateComponents[mdIndices[1]],
                        evaluator.DateComponents[mdIndices[0]]);
                if (DateInRange(dc, minResult, maxResult))
                {
                    dateCandidates.Add(dc);
                }
            }

            // no valid date option means all possibilities are out of range
            if (dateCandidates.Any() == false)
            {
                evaluator.Confidence = DateInterpretationConfidence.None;
                evaluator.SetInvalidFormat(InterpretedDateFormat.OutOfRange);
                evaluator.MonthIndex = mdIndices[0];
                evaluator.DateIndex = mdIndices[1];

                evaluator.Date = new DateTime(
                        evaluator.YearCandidates[0],
                        evaluator.DateComponents[evaluator.MonthIndex!.Value],
                        evaluator.DateComponents[evaluator.DateIndex!.Value]);
                return;
            }

            // one valid date option is the one we want (medium confidence)
            if(dateCandidates.Count == 1)
            {
                evaluator.Confidence = mdConfidence[DateInterpretationConfidence.Medium];
                evaluator.Date = dateCandidates[0];
                ResolveMonthDateIndices(evaluator);
                return;
            }

            // multiple valid date options: a date option sharing the context month and year is assumed to be the intended one (medium confidence)
            var likelyDate = dateCandidates.FirstOrDefault(x => x.Month == contextDate.Month && x.Year == contextDate.Year);
            if(likelyDate != default)
            {
                evaluator.Confidence = mdConfidence[DateInterpretationConfidence.Medium];
                evaluator.Date = likelyDate;
                ResolveMonthDateIndices(evaluator);
                return;
            }

            // multiple valid date options: get the one closest to the context date
            var dcDeltas = dateCandidates.Select(x => Math.Abs((contextDate - x).TotalDays)).ToList();
            var minDelta = dcDeltas.Min();
            if (dcDeltas.Where(x => x == minDelta).Count() == 1)
            {
                evaluator.Date = dateCandidates[dcDeltas.IndexOf(minDelta)];
                evaluator.Confidence = mdConfidence[DateInterpretationConfidence.Low];
                ResolveMonthDateIndices(evaluator);
                return;
            }

            // there are multiple candidates equidistant to the context date; we'll just use the first one -- blind guess, no confidence
            evaluator.Date = dateCandidates.First(x => Math.Abs((contextDate - x).TotalDays) == minDelta);
            evaluator.Confidence = DateInterpretationConfidence.None;
            ResolveMonthDateIndices(evaluator);
        }

        private static void ResolveMonthDateIndices(DateEval evaluator)
        {
            var dc = evaluator.DateComponents;
            evaluator.MonthIndex = Array.IndexOf(dc, evaluator.Date!.Value.Month);
            evaluator.DateIndex = Array.IndexOf(dc, evaluator.Date.Value.Day);
        }

        private static bool DateInRange(DateTime date, DateTime min, DateTime max)
            => date >= min && date <= max;

        private static Dictionary<DateInterpretationConfidence, DateInterpretationConfidence> GetMonthDateConfidenceScale(DateEval evaluator)
        {
            var scale = new Dictionary<DateInterpretationConfidence, DateInterpretationConfidence>
            {
                { DateInterpretationConfidence.High, DateInterpretationConfidence.None },
                { DateInterpretationConfidence.Medium, DateInterpretationConfidence.None },
                { DateInterpretationConfidence.Low, DateInterpretationConfidence.None },
                { DateInterpretationConfidence.None, DateInterpretationConfidence.None }
            };

            switch(evaluator.Confidence)
            {
                case DateInterpretationConfidence.High:
                    scale[DateInterpretationConfidence.High] = DateInterpretationConfidence.High;
                    scale[DateInterpretationConfidence.Medium] = DateInterpretationConfidence.Medium;
                    scale[DateInterpretationConfidence.Low] = DateInterpretationConfidence.Low;
                    break;
                case DateInterpretationConfidence.Medium:
                    scale[DateInterpretationConfidence.High] = DateInterpretationConfidence.Medium;
                    scale[DateInterpretationConfidence.Medium] = DateInterpretationConfidence.Low;
                    break;
                case DateInterpretationConfidence.Low:
                    scale[DateInterpretationConfidence.High] = DateInterpretationConfidence.Low;
                    break;
            }

            return scale;
        }

        private static void ManageChosenPossibleYearIndex(int index, DateEval evaluator, DateTime minResult, DateTime maxResult)
        {
            evaluator.YearIndex = index;
            evaluator.YearCandidates = GetPlausibleYears(evaluator.DateComponents[index], minResult, maxResult);

            evaluator.Confidence = evaluator.YearCandidates.Length == 1 ?
                    DateInterpretationConfidence.Medium : DateInterpretationConfidence.Low;
        }

        private static void ManageSinglePossibleYearIndex(int index, DateEval evaluator, DateTime minResult, DateTime maxResult)
        {
            evaluator.YearIndex = index;
            evaluator.YearCandidates = GetPlausibleYears(evaluator.DateComponents[index], minResult, maxResult);
            if (evaluator.YearCandidates.Length == 0)
            {
                evaluator.YearCandidates =
                    GetPlausibleYears(evaluator.DateComponents[index], DateTime.MinValue, DateTime.MaxValue); // so we can form some date, valid or not
                evaluator.SetInvalidFormat(InterpretedDateFormat.OutOfRange);
                evaluator.Confidence = DateInterpretationConfidence.None;
            }
            else
            {
                evaluator.Confidence = evaluator.YearCandidates.Length == 1 ?
                    DateInterpretationConfidence.High : DateInterpretationConfidence.Medium;
            }
        }

        private static int[] GetPlausibleYears(int yy, DateTime minResult, DateTime maxResult)
        {
            var minCentury = minResult.Year / 100;
            var maxCentury = maxResult.Year / 100;
            var yearCandidates = new List<int>();
            for(int i = minCentury; i <= maxCentury; i++)
            {
                var yearCandidate = (i * 100) + yy;
                if(yearCandidate >= minResult.Year && yearCandidate <= maxResult.Year)
                {
                    yearCandidates.Add(yearCandidate);
                }
            }

            return yearCandidates.ToArray();
        }

        private static DateEval? GetDateEval(string date)
        {
            if(Regex.IsMatch(date, PossibleDateRegex) == false)
            {
                return null;
            }

            // allowable separators are -, /, and \
            var separatorToken = "-";
            if(date.Contains('\\'))
            {
                separatorToken = "\\";
            }
            else if(date.Contains('/'))
            {
                separatorToken = "/";
            }

            var dateRegex = $"^([0-9]{{1,4}}{separatorToken}[0-9]{{1,2}}{separatorToken}[0-9]{{1,4}})|([0-9]{{1,2}}{separatorToken}[0-9]{{1,2}})$";
            // ensure that only one separator token is used (no funny business like 1/1-2024)
            if(Regex.IsMatch(date, dateRegex) == false)
            {
                return null;
            }

            return new(date);
        }

        private static InterpretedDate GetInvalidDateInstance()
            => new(DateTime.MinValue, InterpretedDateFormat.InvalidDate, DateInterpretationConfidence.None);

        internal const string PossibleDateRegex = @"^([0-9]{1,4}[\\/-][0-9]{1,2}[\\/-][0-9]{1,4})|([0-9]{1,2}[\\/-][0-9]{1,2})$";
    }
}
