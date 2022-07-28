using System;

namespace Displacement_Collection
{
    /// <summary>
    /// Provides the ability to collect fixed amount of elements according specified comparator.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TComparableValue"></typeparam>
    public class DisplacementCollection<TKey, TComparableValue> //where TValue : IComparable
    {
        private readonly KeyValuePair<TKey, TComparableValue>[] _storage;
        private readonly Comparison<TComparableValue> _comparer;

        private int _worstCandidateIndex = 0;

        /// <summary>
        /// Initialize instance of DisplacementCollection.
        /// </summary>
        /// <param name="size">Size of collection.</param>
        /// <param name="comparer">Comparer for selection.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public DisplacementCollection(int size, Comparison<TComparableValue> comparer)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            Size = size;
            _storage = new KeyValuePair<TKey, TComparableValue>[size];
        }

        public int Size { get; private set; }

        /// <summary>
        /// Inserts element in the collection if elements value responds current collection condition.
        /// If insertion succeed "worst" existing element of collection will be removed.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>True if insertion succeed, otherwise - false.</returns>
        public bool Insert(TKey key, TComparableValue value)
        {
            if (_comparer(value, _storage[_worstCandidateIndex].Value) <= 0)
            {
                return false;
            }

            for (int i = 0; i < Size; i++)
            {
                if (_comparer(value, _storage[i].Value) >= 0)
                {
                    _storage[_worstCandidateIndex] = KeyValuePair.Create(key, value);
                    break;
                }
            }

            SetWorstCondidateIndex();

            return true;
        }

        /// <summary>
        /// Gets collection in current state.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<TKey, TComparableValue>> GetCollection()
        {
            foreach (var item in _storage)
            {
                yield return item;
            }
        }

        private void SetWorstCondidateIndex()
        {
            _worstCandidateIndex = 0;

            for (int i = 1; i < Size; i++)
            {
                if (_comparer(_storage[_worstCandidateIndex].Value, _storage[i].Value) > 0)
                {
                    _worstCandidateIndex = i;
                }
            }
        }
    }
}
