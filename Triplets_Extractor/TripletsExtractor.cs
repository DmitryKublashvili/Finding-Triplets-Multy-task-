using Displacement_Collection;
using System.Collections.Concurrent;
using System.Text;

namespace Triplets_Extractor
{
    public delegate OutT OutputFormat<OutT>(byte firstChar, byte secondChar, byte thirdChar, int count);

    /// <summary>
    /// Triplets exstracor type. Provides ability to extract and return triplets from stream according settings.
    /// </summary>
    /// <typeparam name="OutT">Out format depending on appropriate delegate</typeparam>
    public class TripletsExtractor<OutT>
    {
        private readonly Stream _inputStream;
        private readonly Stream _outputStream;
        private readonly OutputFormat<OutT> _outputFormat;
        private readonly float _coefficientOfProcessorUsing = 0.6f;

        /// <summary>
        /// Initialize instance of TripletsExtractor.
        /// </summary>
        /// <param name="inputStream">Input stream</param>
        /// <param name="outputStream">Output stream</param>
        /// <param name="count">Count of triplets</param>
        /// <param name="format">Delegate for output format.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TripletsExtractor(Stream inputStream, Stream outputStream, int count, OutputFormat<OutT> format)
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
        public TripletsExtractor(Stream inputStream, Stream outputStream, int count, OutputFormat<OutT> format, float coefficientOfProcessorUsing) 
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
            Output(outputItems);
        }

        /// <summary>
        /// Commits triplets extraction in multytask scenario.
        /// </summary>
        public void DoInMultytaskScenario()
        {
            List<Task<Dictionary<(byte, byte, byte), int>>> tasks = new();
            
            long lengthOfStream = _inputStream.Length;
            int partOfBytesLength = (int)(lengthOfStream / (Environment.ProcessorCount * _coefficientOfProcessorUsing));
            byte[] partOfBytes = new byte[partOfBytesLength];

            while (_inputStream.Position < lengthOfStream)
            {
                if (lengthOfStream - _inputStream.Position < partOfBytesLength) 
                    partOfBytes = new byte[lengthOfStream - _inputStream.Position];

                _inputStream.Read(partOfBytes);

                var cloneBytesArray = (byte[])partOfBytes.Clone();

                tasks.Add(Task.Run(() => FillPartialDictionary(cloneBytesArray)));

                _inputStream.Position = _inputStream.Position >= lengthOfStream - 1 ? lengthOfStream : _inputStream.Position - 2;
            }
            var r = tasks[0].Result;

            var dictionaries = Task.WhenAll(tasks).Result;

            Dictionary<(byte, byte, byte), int> commonDictionary = new();

            foreach (var item in dictionaries)
            {
                commonDictionary.Concat(item);
            }

            IEnumerable<OutT> outputItems = SelectConcreteCountOfTriplets(commonDictionary);

            Output(outputItems);
        }

        private IEnumerable<OutT> SelectConcreteCountOfTriplets(Dictionary<(byte, byte, byte), int> dictionary)
        {
            DisplacementCollection<(byte, byte, byte), int> resultCollection = new(Count, Comparer<int>.Default.Compare);

            foreach (var item in dictionary)
            {
                resultCollection.Insert(item.Key, item.Value);
            }

            IEnumerable<OutT> outputItems = resultCollection
                .GetCollection()
                .Select(x => _outputFormat(x.Key.Item1, x.Key.Item2, x.Key.Item3, x.Value));
            return outputItems;
        }

        private void Output(IEnumerable<OutT> outPutItems)
        {
            foreach (var item in outPutItems)
            {
                byte[] bytesResult = Encoding.Default.GetBytes(item?.ToString() ?? string.Empty);
                _outputStream.Write(bytesResult);
            }
        }

        private Dictionary<(byte, byte, byte), int> FillDictionary()
        {
            Dictionary<(byte, byte, byte), int> dictionary = new();

            byte firstLetter = default;
            byte secondLetter = default;
            byte lettersInRow = 0;
            int currentByte;

            while ((currentByte = _inputStream.ReadByte()) > 0)
            {
                if (char.IsLetter((char)currentByte))
                {
                    lettersInRow++;

                    switch (lettersInRow)
                    {
                        case 1:
                            firstLetter = (byte)currentByte;
                            break;
                        case 2:
                            secondLetter = (byte)currentByte;
                            break;
                        case 3:
                            if (dictionary.ContainsKey((firstLetter, secondLetter, (byte)currentByte)))
                            {
                                dictionary[(firstLetter, secondLetter, (byte)currentByte)] += 1;
                            }
                            else
                            {
                                dictionary.Add((firstLetter, secondLetter, (byte)currentByte), 1);
                            }

                            (firstLetter, secondLetter) = (secondLetter, (byte)currentByte);
                            lettersInRow = 2;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    lettersInRow = 0;
                }
            }

            return dictionary;
        }

        private Dictionary<(byte, byte, byte), int> FillPartialDictionary(byte[] bytes)
        {
            Dictionary<(byte, byte, byte), int> dictionary = new();

            byte firstLetter = default;
            byte secondLetter = default;
            byte lettersInRow = 0;
            int bytesLength = bytes.Length;

            for (int i = 0; i < bytesLength; i++)
            {
                byte currentByte = bytes[i];

                if (char.IsLetter((char)currentByte))
                {
                    lettersInRow++;

                    switch (lettersInRow)
                    {
                        case 1:
                            firstLetter = currentByte;
                            break;
                        case 2:
                            secondLetter = currentByte;
                            break;
                        case 3:
                            if (dictionary.ContainsKey((firstLetter, secondLetter, currentByte)))
                            {
                                dictionary[(firstLetter, secondLetter, currentByte)] += 1;
                            }
                            else
                            {
                                dictionary.TryAdd((firstLetter, secondLetter, currentByte), 1);
                            }

                            (firstLetter, secondLetter) = (secondLetter, currentByte);
                            lettersInRow = 2;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    lettersInRow = 0;
                }
            }

            return dictionary;
        }
    }
}
