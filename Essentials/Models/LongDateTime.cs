using System;
using System.Diagnostics.CodeAnalysis;

namespace n_ate.Essentials.Models
{
    /// <summary>
    /// A DateTime value type that is always Utc and makes a friendly long value, e.g. 202312312359590000, i.e. 2023-12-31 23:59:59.0000
    /// Has a date range of 0001-01-01 00:00:00 --> 9999-12-31 23:59:59
    /// </summary>
    public class LongDateTime : IConvertible
    {
        //public const string SORTABLE_DATETIME_FORMAT = "yyyyMMddHHmmss";
        //public const string SORTABLE_DATETIME_FORMAT_FRIENDLY = "yyyy-MM-dd HH:mm:ss.zzz";
        //public static readonly long MaxLong = 99991231235959; //9999-12-31 23:59:59.9999
        //public static readonly DateTime MaxUtcDateTime = new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc); //utc max
        //public static readonly long MinLong = 00010101000000; //0001-01-01 00:00:00.0000
        //public static readonly DateTime MinUtcDateTime = new DateTime(0, DateTimeKind.Utc); //utc min
        //public static readonly LongDateTime UtcMax = new LongDateTime(MaxLong);
        //public static readonly LongDateTime UtcMin = new LongDateTime(MinLong);

        public const string SORTABLE_DATETIME_FORMAT = "yyyyMMddHHmmssffff";
        public const string SORTABLE_DATETIME_FORMAT_FRIENDLY = "yyyy-MM-dd HH:mm:ss.ffffzzz";
        public static readonly long MaxLong = 999912312359599999; //9999-12-31 23:59:59.9999
        public static readonly DateTime MaxUtcDateTime = new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc); //utc max
        public static readonly long MinLong = 000101010000000000; //0001-01-01 00:00:00.0000
        public static readonly DateTime MinUtcDateTime = new DateTime(0, DateTimeKind.Utc); //utc min
        public static readonly LongDateTime UtcMax = new LongDateTime(MaxLong);
        public static readonly LongDateTime UtcMin = new LongDateTime(MinLong);

        private LongDateTime(long date)
        {
            this.Value = MassageAndValidateLongForConversionToLongDateTime(date);
        }

        private LongDateTime(DateTime date)
        {
            this.Value = ConvertDateTimeToLong(date);
        }

        public static LongDateTime UtcNow => new LongDateTime(DateTime.UtcNow);  //LongDateTime.From(DateTime.UtcNow);
        public long Value { get; private set; }

        private static long ConvertDateTimeToLong(DateTime date)
        {
            if (date.Equals(DateTime.MaxValue) || date.Equals(MaxUtcDateTime)) //Massages MaxValues to _maxDateTime
            {
                date = MaxUtcDateTime;
            }
            if (date.Equals(DateTime.MinValue) || date.Equals(MinUtcDateTime)) //Massages MinValues to _minDateTime
            {
                date = MinUtcDateTime;
            }
            date = date.ToUniversalTime();
            if (date.Kind != DateTimeKind.Utc) throw new ArgumentException($"{nameof(date)} argument has invalid time zone: {date.Kind}.", nameof(date));
            if (date > MaxUtcDateTime) throw new ArgumentException($"{nameof(date)} argument ({date.ToString(SORTABLE_DATETIME_FORMAT_FRIENDLY)}) is greater than {nameof(LongDateTime)}.{nameof(MaxUtcDateTime)} ({MaxUtcDateTime.ToString(SORTABLE_DATETIME_FORMAT_FRIENDLY)}).", nameof(date));
            if (date < MinUtcDateTime) throw new ArgumentException($"{nameof(date)} argument ({date.ToString(SORTABLE_DATETIME_FORMAT_FRIENDLY)}) is less than {nameof(LongDateTime)}.{nameof(MinUtcDateTime)} ({MinUtcDateTime.ToString(SORTABLE_DATETIME_FORMAT_FRIENDLY)}).", nameof(date));
            var value = date.ToString(SORTABLE_DATETIME_FORMAT);
            return long.Parse(value);
        }

        private static long MassageAndValidateLongForConversionToLongDateTime(long date)
        {
            if (date == long.MaxValue) //Massages MaxValue to _maxLong
            {
                date = MaxLong;
            }
            if (date == long.MinValue) //Massages MinValue to _minLong
            {
                date = MinLong;
            }
            if (date > MaxLong) throw new ArgumentException($"{nameof(date)} argument ({date}) is greater than {nameof(LongDateTime)}.{nameof(MaxLong)} ({MaxLong}).", nameof(date));
            if (date < MinLong) throw new ArgumentException($"{nameof(date)} argument ({date}) is less than {nameof(LongDateTime)}.{nameof(MinLong)} ({MinLong}).", nameof(date));
            return date;
        }

        public static LongDateTime From(DateTime date) => new LongDateTime(ConvertDateTimeToLong(date));

        public static LongDateTime From(long date) => new LongDateTime(MassageAndValidateLongForConversionToLongDateTime(date));

        public static TimeSpan operator -(LongDateTime left, LongDateTime right) => left.ToDateTime() - right.ToDateTime();

        public static bool operator !=(LongDateTime left, LongDateTime right) => !left.Equals(right);

        public static bool operator <(LongDateTime left, LongDateTime right) => left.Value < right.Value;

        public static bool operator <=(LongDateTime left, LongDateTime right) => left.Value <= right.Value;

        public static bool operator ==(LongDateTime left, LongDateTime right) => left.Equals(right);

        public static bool operator >(LongDateTime left, LongDateTime right) => left.Value > right.Value;

        public static bool operator >=(LongDateTime left, LongDateTime right) => left.Value >= right.Value;

        public LongDateTime AddDays(int days) => From(ToDateTime().AddDays(days));

        public LongDateTime AddHours(int hours) => From(ToDateTime().AddHours(hours));

        public LongDateTime AddMilliseconds(int milliseconds) => From(ToDateTime().AddMilliseconds(milliseconds));

        public LongDateTime AddMinutes(int minutes) => From(ToDateTime().AddMinutes(minutes));

        public LongDateTime AddMonths(int months) => From(ToDateTime().AddMonths(months));

        public LongDateTime AddSeconds(int seconds) => From(ToDateTime().AddSeconds(seconds));

        public LongDateTime AddTicks(int ticks) => From(ToDateTime().AddTicks(ticks));

        public LongDateTime AddYears(int years) => From(ToDateTime().AddYears(years));

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is LongDateTime) return this.Value == ((LongDateTime)obj).Value;
            return obj is DateTime && this.Value == (new LongDateTime((DateTime)obj)).Value;
        }

        public override int GetHashCode() => Value.GetHashCode();

        public TypeCode GetTypeCode() => TypeCode.DateTime;

        public bool ToBoolean(IFormatProvider? provider) => throw new InvalidCastException();

        public byte ToByte(IFormatProvider? provider) => throw new InvalidCastException();

        public char ToChar(IFormatProvider? provider) => throw new InvalidCastException();

        public DateTime ToDateTime(bool translateMaxAndMin = true)
        {
            DateTime? result = null;
            if (translateMaxAndMin)
            {
                if (Value.Equals(MaxLong)) //Massages _maxLong to _maxDateTime
                {
                    result = MaxUtcDateTime;
                }
                if (Value.Equals(MinLong)) //Massages _minLong to _minDateTime
                {
                    result = MinUtcDateTime;
                }
            }
            if (!result.HasValue)
            {
                result = DateTime.ParseExact($"{Value.ToString().PadLeft(SORTABLE_DATETIME_FORMAT.Length, '0')}-0", SORTABLE_DATETIME_FORMAT + "z", null).ToUniversalTime();
            }
            return result.Value;
        }

        public DateTime ToDateTime(IFormatProvider? provider) => ToDateTime();

        public decimal ToDecimal(IFormatProvider? provider) => throw new InvalidCastException();

        public double ToDouble(IFormatProvider? provider) => throw new InvalidCastException();

        public short ToInt16(IFormatProvider? provider) => throw new InvalidCastException();

        public int ToInt32(IFormatProvider? provider) => throw new InvalidCastException();

        public long ToInt64(IFormatProvider? provider) => this.Value;

        public long ToLong() => this.Value;

        public sbyte ToSByte(IFormatProvider? provider) => throw new InvalidCastException();

        public float ToSingle(IFormatProvider? provider) => throw new InvalidCastException();

        public string ToString(string format) => ToDateTime().ToString(format);

        public override string ToString() => ToString(SORTABLE_DATETIME_FORMAT_FRIENDLY);

        public string ToString(IFormatProvider? provider) => ToString();

        public object ToType(Type conversionType, IFormatProvider? provider) => conversionType.Name switch
        {
            nameof(Boolean) => ToBoolean(provider),
            nameof(Int64) => ToInt64(provider),
            nameof(String) => ToString(provider),
            _ => throw new InvalidCastException(),
        };

        public ushort ToUInt16(IFormatProvider? provider) => throw new InvalidCastException();

        public uint ToUInt32(IFormatProvider? provider) => throw new InvalidCastException();

        public ulong ToUInt64(IFormatProvider? provider) => throw new InvalidCastException();
    }
}