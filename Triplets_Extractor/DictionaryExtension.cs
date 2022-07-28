namespace Triplets_Extractor
{
    static class DictionaryExtension
    {
        /// <summary>
        /// Performs the merging of two dictionaries in first dictionary.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="commonDictionary"></param>
        /// <param name="additionalDictionary"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Concat<TKey>(this Dictionary<TKey, int> commonDictionary, Dictionary<TKey, int> additionalDictionary)
        {
            if (commonDictionary is null)
            {
                throw new ArgumentNullException(nameof(commonDictionary));
            }

            if (additionalDictionary is null)
            {
                throw new ArgumentNullException(nameof(additionalDictionary));
            }

            foreach (var pair in additionalDictionary)
            {
                if (commonDictionary.ContainsKey(pair.Key))
                {
                    commonDictionary[pair.Key] += pair.Value;
                }
                else
                {
                    commonDictionary.Add(pair.Key, pair.Value);
                }
            }
        }
    }
}
