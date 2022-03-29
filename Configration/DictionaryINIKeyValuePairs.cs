using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YCabz.Configration
{
    public class DictionaryINIKeyValuePairs
    {
        internal Dictionary<DictionaryINIString, DictionaryINIValue> Dictionary { get; } = new Dictionary<DictionaryINIString, DictionaryINIValue>();

        internal DictionaryINIKeyValuePairs()
        {
        }

        public DictionaryINIValue this[DictionaryINIString key]
        {
            get
            {
                if (Dictionary.ContainsKey(key) == false)
                {
                    Dictionary.Add(key, string.Empty);
                }
                return Dictionary[key];
            }
            set
            {
                Dictionary[key] = value;
            }
        }
    }
}
