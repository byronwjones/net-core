using BWJ.Core.Chronology;

namespace BWJ.Core.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void WhenStringBlank_Invalid()
        {
            var cxt = DateTime.Now;
            var min = cxt.AddDays(-1);
            var max = cxt.AddDays(1);
            var id = InterpretedDate.FromString("  ", cxt, min, max);

            Assert.That(id.Format, Is.EqualTo(InterpretedDateFormat.InvalidDate));
        }

        [Test]
        public void WhenDateDelimitersNotUniform_Invalid()
        {
            var cxt = DateTime.Now;
            var min = cxt.AddDays(-1);
            var max = cxt.AddDays(1);
            var id = InterpretedDate.FromString("1/1-2024", cxt, min, max);

            Assert.That(id.Format, Is.EqualTo(InterpretedDateFormat.InvalidDate));
        }

        [Test]
        public void WhenMultipleFourDigitDateComponents_Invalid()
        {
            var cxt = DateTime.Now;
            var min = cxt.AddDays(-1);
            var max = cxt.AddDays(1);
            var id = InterpretedDate.FromString("2024/1/2024", cxt, min, max);

            Assert.That(id.Format, Is.EqualTo(InterpretedDateFormat.InvalidDate));
        }

        [Test]
        public void WhenNoTwoDigitDateComponent_Invalid()
        {
            var cxt = DateTime.Now;
            var min = cxt.AddDays(-1);
            var max = cxt.AddDays(1);
            var id = InterpretedDate.FromString("1/1/1", cxt, min, max);

            Assert.That(id.Format, Is.EqualTo(InterpretedDateFormat.InvalidDate));
        }

        [Test]
        public void WhenOnlyOneTwoDigitComponent_ItIsTheYear()
        {
            var cxt = new DateTime(2010, 1, 1);
            var min = new DateTime(2000, 1, 1);
            var max = new DateTime(2025, 1, 1);
            var id = InterpretedDate.FromString("1/1/01", cxt, min, max);

            Assert.That(id.Date, Is.EqualTo(new DateTime(2001, 1, 1)));
            Assert.That(id.Confidence, Is.EqualTo(DateInterpretationConfidence.High));
            Assert.That(id.Format, Is.EqualTo(InterpretedDateFormat.MonthDateYear));
        }

        [Test]
        public void WhenThereIsAFourDigitComponent_ItIsTheYear()
        {
            var cxt = new DateTime(2010, 1, 1);
            var min = new DateTime(2000, 1, 1);
            var max = new DateTime(2025, 1, 1);
            var id = InterpretedDate.FromString("1/1/2024", cxt, min, max);

            Assert.That(id.Date, Is.EqualTo(new DateTime(2024, 1, 1)));
            Assert.That(id.Confidence, Is.EqualTo(DateInterpretationConfidence.High));
            Assert.That(id.Format, Is.EqualTo(InterpretedDateFormat.MonthDateYear));

            id = InterpretedDate.FromString("2024/1/1", cxt, min, max);

            Assert.That(id.Date, Is.EqualTo(new DateTime(2024, 1, 1)));
            Assert.That(id.Confidence, Is.EqualTo(DateInterpretationConfidence.High));
            Assert.That(id.Format, Is.EqualTo(InterpretedDateFormat.YearMonthDate));
        }

        [Test]
        public void WhenFirstAndLastComponentCouldBeYear_MatchToContextYearIsUsed()
        {
            var cxt = new DateTime(2024, 1, 1);
            var min = new DateTime(2000, 1, 1);
            var max = new DateTime(2025, 1, 1);
            var id = InterpretedDate.FromString("24/1/13", cxt, min, max);

            Assert.That(id.Date, Is.EqualTo(new DateTime(2024, 1, 13)));
            Assert.That(id.Confidence, Is.EqualTo(DateInterpretationConfidence.Medium));
            Assert.That(id.Format, Is.EqualTo(InterpretedDateFormat.YearMonthDate));

            id = InterpretedDate.FromString("13/1/24", cxt, min, max);

            Assert.That(id.Date, Is.EqualTo(new DateTime(2024, 1, 13)));
            Assert.That(id.Confidence, Is.EqualTo(DateInterpretationConfidence.Medium));
            Assert.That(id.Format, Is.EqualTo(InterpretedDateFormat.DateMonthYear));
        }

        [Test]
        public void WhenFirstAndLastComponentCouldBeYearAndBothYearsOutOfBounds_OutOfRange()
        {
            var cxt = new DateTime(2024, 1, 1);
            var min = new DateTime(2024, 1, 1);
            var max = new DateTime(2024, 1, 1);
            var id = InterpretedDate.FromString("10/1/13", cxt, min, max);

            Assert.That(id.Confidence, Is.EqualTo(DateInterpretationConfidence.None));
            Assert.That(id.Format, Is.EqualTo(InterpretedDateFormat.OutOfRange));
        }

        [Test]
        public void WhenFirstAndLastComponentCouldBeYearAndOneYearOutOfBounds_YearInRangeIsUsed()
        {
            var cxt = new DateTime(2024, 1, 1);
            var min = new DateTime(2021, 1, 1);
            var max = new DateTime(2025, 1, 1);
            var id = InterpretedDate.FromString("20/1/22", cxt, min, max);

            Assert.That(id.Date.Year, Is.EqualTo(2022));
            Assert.That(id.Confidence, Is.EqualTo(DateInterpretationConfidence.Medium));

            id = InterpretedDate.FromString("22/1/20", cxt, min, max);

            Assert.That(id.Date.Year, Is.EqualTo(2022));
            Assert.That(id.Confidence, Is.EqualTo(DateInterpretationConfidence.Medium));
        }

        [Test]
        public void WhenFirstAndLastComponentCouldBeYearButOneYearCantBeADate_YearIsTheImpossibleDate()
        {
            var cxt = new DateTime(2024, 1, 1);
            var min = new DateTime(1995, 1, 1);
            var max = new DateTime(2025, 1, 1);
            var id = InterpretedDate.FromString("24/1/99", cxt, min, max);

            Assert.That(id.Date.Year, Is.EqualTo(1999));
            Assert.That(id.Confidence, Is.EqualTo(DateInterpretationConfidence.High));

            id = InterpretedDate.FromString("99/1/24", cxt, min, max);

            Assert.That(id.Date.Year, Is.EqualTo(1999));
            Assert.That(id.Confidence, Is.EqualTo(DateInterpretationConfidence.High));
        }

        /*
cd 1/1/22, range 1/1/1995-1/1/2025, 24/1/21, 21/1/24: year 2021, lofi
cd 1/1/22, range 1/1/1995-1/1/2025, 21/1/23: year 2021, no confidence
13/14/2024: invalid date
12/32/2024: invalid date
*invalid if no plausible date/month must present at 1st or 3rd position
range 1/1/2030-1/1/2035, 32/1/33: invalid date
12/12/2024: 12 dec 2024, hifi
*two digit year value guessed on which within range, clear month/day
cd 1/1/24, range 1/1/2014-1/1/2024, 12/12/22: 12 dec 2022, mifi
*two digit year guessed on which closest to cd
cd 1/1/2011, range 1/1/1995-1/1/2025, 12/12/11: 12 dec 2011, lofi
*plausible day, but invalid for given month
2/30/2024: invalid date
range 4/1/2024-6/1/2024, 5/20/2024: below range
range 4/1/2024-6/1/2024, 7/20/2024: above range
*within range, clear year, clear day
range 4/1/2024-8/1/2024, 7/20/2024: 20 jul 2024, hifi
*when there are multiple date candidates, we take the one closest to the context date
cd 1/1/2022, range 1/1/1920-1/1/2024, 7/30/20: 30 jul 2020, mifi
*all variations of month/date out of range
range 4/1/2024-5/1/2024, 6/7/2024: out of range
*only one m/d variation in range
range 4/1/2024-7/1/2024, 6/7/2024: 7 jun 2024, mifi
*two m/d variations in range, take the matching cd month/year
cd 5/20/2024, range 4/1/2024-8/1/2024, 5/6/2024: 6 may 2024, mifi
*two m/d variations in range, take the one closest to cd
cd 8/15/2024 range 4/1/2024-9/1/2024, 6/7/2024: 6 jul 2024, lofi
        */
    }
}