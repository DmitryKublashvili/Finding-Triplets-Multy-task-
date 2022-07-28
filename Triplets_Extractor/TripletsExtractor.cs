using Displacement_Collection;
using System.Collections.Concurrent;
using System.Text;

namespace Triplets_Extractor
{
    public delegate OutT OutputFormat<OutT>(char firstChar, char secondChar, char thirdChar, int count);

    /// <summary>
    /// Triplets exstracor type. Provides ability to extract and return triplets from stream according settings.
    /// </summary>
    /// <typeparam name="OutT">Out format depending on appropriate delegate</typeparam>
    public class TripletsExtractor<OutT>
    {
        const int MinFileSizeForMultytaskScenario = 100;

        private readonly StreamReader _inputStream;
        private readonly Stream _outputStream;
        private readonly OutputFormat<OutT> _outputFormat;
        private readonly float _coefficientOfProcessorUsing = 0.6f; // value by default

        /// <summary>
        /// Initialize instance of TripletsExtractor.
        /// </summary>
        /// <param name="inputStream">Input stream</param>
        /// <param name="outputStream">Output stream</param>
        /// <param name="count">Count of triplets</param>
        /// <param name="format">Delegate for output format.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TripletsExtractor(StreamReader inputStream, Stream outputStream, int count, OutputFormat<OutT> format)
        {
            _inputStream = inputStream ?? throw new ArgumentNullException(nameof(inputStream));
            _outputStream = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
            Count = count;
            _outputFormat = format;
        }

        /// <summary>
        /// Initialize instance of TripletsExtractor using coefficient of processor using for multytask scenario.
        /// </summary>
        /// <param name="inputStream">Input stream</param>
        /// <param name="outputStream">Output stream</param>
        /// <param name="count">Count of triplets</param>
        /// <param name="format">Delegate for output format.</param>
        /// <param name="coefficientOfProcessorUsing">Coefficient of processor using for multytask scenario.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TripletsExtractor(StreamReader inputStream, Stream outputStream, int count, OutputFormat<OutT> format, float coefficientOfProcessorUsing) 
            : this(inputStream, outputStream, count, format)
        {
            _coefficientOfProcessorUsing = coefficientOfProcessorUsing;
        }

        /// <summary>
        /// Most popular triplets count.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Commits triplets extraction.
        /// </summary>
        public void Do()
        {
            var dictionary = FillDictionary();
            var outputItems = SelectConcreteCountOfTriplets(dictionary);
            WriteResultToOutputStream(outputItems);
        }

        /// <summary>
        /// Commits triplets extraction in multytask scenario.
        /// </summary>
        public void DoInMultytaskScenario()
        {
            List<Task<Dictionary<(char, char, char), int>>> tasks = new();

            long lengthOfStream = _inputStream.BaseStream.Length;

            if (lengthOfStream < MinFileSizeForMultytaskScenario)
            {
                Do();
                return;
            }

            long charsArrayLength = (int)(lengthOfStream / (Environment.ProcessorCount * _coefficientOfProcessorUsing));
            char[] buffer = new char[charsArrayLength];
            long position = 0;
            char last = default;
            char preLast = default;

            while (!_inputStream.EndOfStream)
            {
                char[] bufferCopy = new char[buffer.Length + 2];
                bufferCopy[0] = preLast;
                bufferCopy[1] = last;

                if (lengthOfStream - position < charsArrayLength)
                {
                    charsArrayLength = lengthOfStream - position;
                    buffer = new char[charsArrayLength];
                }

                _inputStream.ReadBlock(buffer, 0, buffer.Length);

                preLast = buffer[^2];
                last = buffer[^1];

                position += charsArrayLength;

                buffer.CopyTo(bufferCopy, 2);
                tasks.Add(Task.Run(() => FillPartialDictionary(bufferCopy)));
            }

            var dictionaries = Task.WhenAll(tasks).Result;

            Dictionary<(char, char, char), int> commonDictionary = new();

            foreach (var item in dictionaries)
            {
                commonDictionary.Concat(item);
            }

            IEnumerable<OutT> outputItems = SelectConcreteCountOfTriplets(commonDictionary);

            WriteResultToOutputStream(outputItems);
        }

        private IEnumerable<OutT> SelectConcreteCountOfTriplets(Dictionary<(char, char, char), int> dictionary)
        {
            DisplacementCollection<(char, char, char), int> resultCollection = new(Count, Comparer<int>.Default.Compare);

            foreach (var item in dictionary)
            {
                resultCollection.Insert(item.Key, item.Value);
            }

            return resultCollection
                    .GetCollection()
                    .OrderByDescending(x => x.Value)
                    .Select(x => _outputFormat(x.Key.Item1, x.Key.Item2, x.Key.Item3, x.Value));
        }

        private void WriteResultToOutputStream(IEnumerable<OutT> outPutItems)
        {
            foreach (var item in outPutItems)
            {
                byte[] bytesResult = Encoding.Default.GetBytes(item?.ToString() ?? string.Empty);
                _outputStream.Write(bytesResult);
            }
        }

        private Dictionary<(char, char, char), int> FillDictionary()
        {
            Dictionary<(char, char, char), int> dictionary = new();

            char firstLetter = default;
            char secondLetter = default;
            ushort lettersInRow = 0;
            int currentCharInt;

            while ((currentCharInt = _inputStream.Read()) > 0)
            {
                char currentChar = char.ToLowerInvariant((char)currentCharInt);
                CheckChar(dictionary, ref firstLetter, ref secondLetter, ref lettersInRow, currentChar);
            }

            return dictionary;
        }

        private Dictionary<(char, char, char), int> FillPartialDictionary(char[] chars)
        {
            Dictionary<(char, char, char), int> dictionary = new();

            char firstLetter = default;
            char secondLetter = default;
            ushort lettersInRow = 0;
            int bytesLength = chars.Length;

            for (int i = 0; i < bytesLength; i++)
            {
                char currentChar = char.ToLowerInvariant(chars[i]);
                CheckChar(dictionary, ref firstLetter, ref secondLetter, ref lettersInRow, currentChar);
            }

            return dictionary;
        }

        private static void CheckChar(Dictionary<(char, char, char), int> dictionary, ref char firstLetter, ref char secondLetter, ref ushort lettersInRow, char currentChar)
        {
            if (char.IsLetter(currentChar))
            {
                lettersInRow++;

                switch (lettersInRow)
                {
                    case 1:
                        firstLetter = currentChar;
                        break;
                    case 2:
                        secondLetter = currentChar;
                        break;
                    case 3:
                        if (dictionary.ContainsKey((firstLetter, secondLetter, currentChar)))
                        {
                            dictionary[(firstLetter, secondLetter, currentChar)] += 1;
                        }
                        else
                        {
                            dictionary.Add((firstLetter, secondLetter, currentChar), 1);
                        }

                        (firstLetter, secondLetter) = (secondLetter, currentChar);
                        lettersInRow = 2;
                        break;
                    default:
                        throw new IndexOutOfRangeException(nameof(lettersInRow));
                }
            }
            else
            {
                lettersInRow = 0;
            }
        }
    }
}
