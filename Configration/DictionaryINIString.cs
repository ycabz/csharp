﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YCabz.Configration
{
    public class DictionaryINIString
    {
        /// <summary>
        /// INI파일에 저장되는/저장되어있는 문자열
        /// </summary>
        public string RealString
        {
            get => _RealString;
            set
            {
                _RealString = value;
                KeyString = value?.ToUpper();
            }
        }
        private string _RealString;

        /// <summary>
        /// INI 검색 시, 대소문자를 구분하지 않기위해 사용하는 Key
        /// </summary>
        public string KeyString { get; private set; }

        internal DictionaryINIString(string value)
        {
            RealString = value;
        }

        public override bool Equals(object obj)
        {
            var iniString = obj as DictionaryINIString;
            if (iniString != null)
            {
                return KeyString == iniString.KeyString;
            }
            return false;
        }
        public override string ToString() => RealString;
        public override int GetHashCode() => KeyString.GetHashCode();

        public static implicit operator string(DictionaryINIString value) => value?.RealString;
        public static implicit operator DictionaryINIString(string value) => new DictionaryINIString(value);
    }
}
