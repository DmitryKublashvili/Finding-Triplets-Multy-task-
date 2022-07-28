using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8604
namespace Triplets_Extractor
{
    public class TripletsExtractorBuilder<OutT> : ITripletsExtractorBuilder<OutT>
    {
        private Stream _inputStream = null!;
        private Stream _outputStream = null!;
        private int _count = 0!;
        private OutputFormat<OutT>? _outputFormat;

        public TripletsExtractorBuilder<OutT> SetInputStream(Stream stream)
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

        public TripletsExtractor<OutT> Build()
        {
            if (_inputStream is not null && _outputStream is not null && _count > 0)
            {
                return new TripletsExtractor<OutT>(_inputStream, _outputStream, _count, _outputFormat);
            }

            throw new InvalidOperationException("The object is not configured");
        }
    }
}
