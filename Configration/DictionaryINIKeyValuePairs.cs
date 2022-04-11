using System.Collections.Generic;
using INIString = YCabz.Configration.DictionaryINIString;
using INIValue = YCabz.Configration.DictionaryINIValue;

namespace YCabz.Configration
{
    public class DictionaryINIKeyValuePairs
    {
        internal Dictionary<INIString, INIValue> Dictionary { get; } = new Dictionary<INIString, INIValue>();

        internal DictionaryINIKeyValuePairs()
        {
        }

        public INIValue this[INIString key]
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
