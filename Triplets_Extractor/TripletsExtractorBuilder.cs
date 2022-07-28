using System;

#pragma warning disable CS8604
namespace Triplets_Extractor
{
    public class TripletsExtractorBuilder<OutT> : ITripletsExtractorBuilder<OutT>
    {
        private StreamReader _inputStream = null!;
        private Stream _outputStream = null!;
        private int _count = 0!;
        private OutputFormat<OutT>? _outputFormat;
        float _coefficient = default;

        public TripletsExtractorBuilder<OutT> SetInputStream(StreamReader stream)
        {
            _inputStream = stream ?? throw new ArgumentNullException(nameof(stream));
            return this;
        }

        public TripletsExtractorBuilder<OutT> SetOutputStream(Stream stream)
        {
            _outputStream = stream ?? throw new ArgumentNullException(nameof(stream));
            return this;
        }

        public TripletsExtractorBuilder<OutT> SetCount(int count)
        {
            _count = count > 0 ? count : throw new ArgumentNullException(nameof(count));
            return this;
        }

        public TripletsExtractorBuilder<OutT> SetFormat(OutputFormat<OutT> format)
        {
            _outputFormat = format ?? throw new ArgumentNullException(nameof(format));
            return this;
        }

        public TripletsExtractorBuilder<OutT> SetCoefficientOfProcessorUsing(float coefficient)
        {
            if (coefficient <= 0 || coefficient > 0.9)
            {
                throw new ArgumentOutOfRangeException("Coefficient was less or equal zeo or more then 0.9", nameof(coefficient));
            }

            _coefficient = coefficient;
            return this;
        }

        public TripletsExtractor<OutT> Build()
        {
            if (_inputStream is not null && _outputStream is not null && _count > 0)
            {
                return _coefficient == default ? new TripletsExtractor<OutT>(_inputStream, _outputStream, _count, _outputFormat)
                    : new TripletsExtractor<OutT>(_inputStream, _outputStream, _count, _outputFormat, _coefficient);
            }

            throw new InvalidOperationException("The object is not configured");
        }
    }
}
