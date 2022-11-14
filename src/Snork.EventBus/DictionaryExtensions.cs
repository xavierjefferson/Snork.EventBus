using System.Collections.Generic;

namespace Snork.EventBus
{
    static class DictionaryExtensions
    {
        public static TU? Put<T, TU>(this Dictionary<T, TU> dictionary, T index, TU value) where TU : class
        {
            var result = default(TU);
            if (dictionary.ContainsKey(index)) result = dictionary[index];
            dictionary[index] = value;
            return result;
        }
    }
}