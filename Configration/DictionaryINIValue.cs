using System;
using INIValue = YCabz.Configration.DictionaryINIValue;

namespace YCabz.Configration
{
    public class DictionaryINIValue
    {
        internal string Value { get; }
        internal string Comment { get; }

        internal DictionaryINIValue(string value = "", string comment = "")
        {
            Value = value;
            Comment = comment;
        }

        public override string ToString()
        {
            return Value;
        }

        public bool ToBool()
        {
            if (bool.TryParse(Value, out var result) == false)
            {
                throw new InvalidCastException($"Invalid Cast To bool: {Value}");
            }

            return result;
        }

        public byte ToByte()
        {
            if (byte.TryParse(Value, out var result) == false)
            {
                throw new InvalidCastException($"Invalid Cast To byte: {Value}");
            }

            return result;
        }

        public sbyte TobyteSigned()
        {
            if (sbyte.TryParse(Value, out var result) == false)
            {
                throw new InvalidCastException($"Invalid Cast To sbyte: {Value}");
            }

            return result;
        }

        public short ToShort()
        {
            if (short.TryParse(Value, out var result) == false)
            {
                throw new InvalidCastException($"Invalid Cast To short: {Value}");
            }

            return result;
        }

        public ushort ToShortUnsigned()
        {
            if (ushort.TryParse(Value, out var result) == false)
            {
                throw new InvalidCastException($"Invalid Cast To ushort: {Value}");
            }

            return result;
        }

        public int ToInt()
        {
            if (int.TryParse(Value, out var result) == false)
            {
                throw new InvalidCastException($"Invalid Cast To int: {Value}");
            }

            return result;
        }

        public uint ToIntUnsigned()
        {
            if (uint.TryParse(Value, out var result) == false)
            {
                throw new InvalidCastException($"Invalid Cast To uint: {Value}");
            }

            return result;
        }

        public long ToLong()
        {
            if (long.TryParse(Value, out var result) == false)
            {
                throw new InvalidCastException($"Invalid Cast To long: {Value}");
            }

            return result;
        }

        public ulong ToLongUnsigned()
        {
            if (ulong.TryParse(Value, out var result) == false)
            {
                throw new InvalidCastException($"Invalid Cast To ulong: {Value}");
            }

            return result;
        }

        public float ToFloat()
        {
            if (float.TryParse(Value, out var result) == false)
            {
                throw new InvalidCastException($"Invalid Cast To float: {Value}");
            }

            return result;
        }

        public double ToDouble()
        {
            if (double.TryParse(Value, out var result) == false)
            {
                throw new InvalidCastException($"Invalid Cast To double: {Value}");
            }

            return result;
        }

        public decimal ToDecimal()
        {
            if (decimal.TryParse(Value, out var result) == false)
            {
                throw new InvalidCastException($"Invalid Cast To decimal: {Value}");
            }

            return result;
        }

        public char ToChar()
        {
            if (char.TryParse(Value, out var result) == false)
            {
                throw new InvalidCastException($"Invalid Cast To char: {Value}");
            }

            return result;
        }


        #region Implicit

        public static implicit operator bool(INIValue value) => value.ToBool();
        public static implicit operator INIValue(bool value) => new INIValue(value.ToString());
        public static implicit operator byte(INIValue value) => value.ToByte();
        public static implicit operator INIValue(byte value) => new INIValue(value.ToString());
        public static implicit operator sbyte(INIValue value) => value.TobyteSigned();
        public static implicit operator INIValue(sbyte value) => new INIValue(value.ToString());
        public static implicit operator short(INIValue value) => value.ToShort();
        public static implicit operator INIValue(short value) => new INIValue(value.ToString());
        public static implicit operator ushort(INIValue value) => value.ToShortUnsigned();
        public static implicit operator INIValue(ushort value) => new INIValue(value.ToString());
        public static implicit operator int(INIValue value) => value.ToInt();
        public static implicit operator INIValue(int value) => new INIValue(value.ToString());
        public static implicit operator uint(INIValue value) => value.ToIntUnsigned();
        public static implicit operator INIValue(uint value) => new INIValue(value.ToString());
        public static implicit operator long(INIValue value) => value.ToLong();
        public static implicit operator INIValue(long value) => new INIValue(value.ToString());
        public static implicit operator ulong(INIValue value) => value.ToLongUnsigned();
        public static implicit operator INIValue(ulong value) => new INIValue(value.ToString());
        public static implicit operator float(INIValue value) => value.ToFloat();
        public static implicit operator INIValue(float value) => new INIValue(value.ToString());
        public static implicit operator double(INIValue value) => value.ToDouble();
        public static implicit operator INIValue(double value) => new INIValue(value.ToString());
        public static implicit operator decimal(INIValue value) => value.ToDecimal();
        public static implicit operator INIValue(decimal value) => new INIValue(value.ToString());
        public static implicit operator string(INIValue value) => value.Value;
        public static implicit operator INIValue(string value) => new INIValue(value);
        public static implicit operator char(INIValue value) => value.ToChar();
        public static implicit operator INIValue(char value) => new INIValue(value.ToString());

        #endregion
    }
}
