using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static implicit operator bool(DictionaryINIValue value) => value.ToBool();
        public static implicit operator DictionaryINIValue(bool value) => new DictionaryINIValue(value.ToString());
        public static implicit operator byte(DictionaryINIValue value) => value.ToByte();
        public static implicit operator DictionaryINIValue(byte value) => new DictionaryINIValue(value.ToString());
        public static implicit operator sbyte(DictionaryINIValue value) => value.TobyteSigned();
        public static implicit operator DictionaryINIValue(sbyte value) => new DictionaryINIValue(value.ToString());
        public static implicit operator short(DictionaryINIValue value) => value.ToShort();
        public static implicit operator DictionaryINIValue(short value) => new DictionaryINIValue(value.ToString());
        public static implicit operator ushort(DictionaryINIValue value) => value.ToShortUnsigned();
        public static implicit operator DictionaryINIValue(ushort value) => new DictionaryINIValue(value.ToString());
        public static implicit operator int(DictionaryINIValue value) => value.ToInt();
        public static implicit operator DictionaryINIValue(int value) => new DictionaryINIValue(value.ToString());
        public static implicit operator uint(DictionaryINIValue value) => value.ToIntUnsigned();
        public static implicit operator DictionaryINIValue(uint value) => new DictionaryINIValue(value.ToString());
        public static implicit operator long(DictionaryINIValue value) => value.ToLong();
        public static implicit operator DictionaryINIValue(long value) => new DictionaryINIValue(value.ToString());
        public static implicit operator ulong(DictionaryINIValue value) => value.ToLongUnsigned();
        public static implicit operator DictionaryINIValue(ulong value) => new DictionaryINIValue(value.ToString());
        public static implicit operator float(DictionaryINIValue value) => value.ToFloat();
        public static implicit operator DictionaryINIValue(float value) => new DictionaryINIValue(value.ToString());
        public static implicit operator double(DictionaryINIValue value) => value.ToDouble();
        public static implicit operator DictionaryINIValue(double value) => new DictionaryINIValue(value.ToString());
        public static implicit operator decimal(DictionaryINIValue value) => value.ToDecimal();
        public static implicit operator DictionaryINIValue(decimal value) => new DictionaryINIValue(value.ToString());
        public static implicit operator string(DictionaryINIValue value) => value.Value;
        public static implicit operator DictionaryINIValue(string value) => new DictionaryINIValue(value);
        public static implicit operator char(DictionaryINIValue value) => value.ToChar();
        public static implicit operator DictionaryINIValue(char value) => new DictionaryINIValue(value.ToString());

        #endregion
    }
}
