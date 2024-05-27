using n_ate.Essentials.Models;

namespace n_ate.Essentials.Tests
{
    public class LongDateTimeTests
    {
        [Test]
        public void From_MaxDate()
        {
            //Assume//

            //Arrange//
            var format = LongDateTime.SORTABLE_DATETIME_FORMAT_FRIENDLY;
            var seed = LongDateTime.MaxUtcDateTime;

            //Act//
            var result = LongDateTime.From(seed);

            //Assert//
            var expected = seed.ToString(format);
            var actual = result.ToString(format);
            Assert.That(Equals(expected, actual));
        }

        [Test]
        public void From_MaxLong()
        {
            //Assume//
            Assume.That(() => From_MaxDate(), Throws.Nothing);
            Assume.That(() => ToLong_Max, Throws.Nothing);

            //Arrange//
            var expected = LongDateTime.UtcMax;

            //Act//
            var actual = LongDateTime.From(LongDateTime.MaxLong);

            //Assert//
            Assert.That(Equals(expected, actual));
        }

        [Test]
        public void From_MinDate()
        {
            //Assume//

            //Arrange//
            var format = LongDateTime.SORTABLE_DATETIME_FORMAT_FRIENDLY;
            var seed = LongDateTime.MinUtcDateTime;

            //Act//
            var result = LongDateTime.From(seed);

            //Assert//
            var expected = seed.ToString(format);
            var actual = result.ToString(format);
            Assert.That(Equals(expected, actual));
        }

        [Test]
        public void From_MinLong()
        {
            //Assume//
            Assume.That(() => From_MinDate(), Throws.Nothing);
            Assume.That(() => ToLong_Min(), Throws.Nothing);

            //Arrange//
            var expected = LongDateTime.UtcMin;

            //Act//
            var actual = LongDateTime.From(LongDateTime.MinLong);

            //Assert//
            Assert.That(Equals(expected, actual));
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ToLong_Max()
        {
            //Assume//
            Assume.That(() => From_MaxDate(), Throws.Nothing);

            //Arrange//
            var expected = LongDateTime.MaxLong;
            var longDate = LongDateTime.UtcMax;

            //Act//
            var actual = longDate.ToLong();

            //Assert//
            Assert.That(Equals(expected, actual));
        }

        [Test]
        public void ToLong_Min()
        {
            //Assume//
            Assume.That(() => From_MinDate(), Throws.Nothing);

            //Arrange//
            var expected = LongDateTime.MinLong;
            var longDate = LongDateTime.UtcMin;

            //Act//
            var actual = longDate.ToLong();

            //Assert//
            Assert.That(Equals(expected, actual));
        }
    }
}