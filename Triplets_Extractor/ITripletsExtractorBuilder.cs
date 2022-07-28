namespace Triplets_Extractor
{
    public interface ITripletsExtractorBuilder<OutT>
    {
        TripletsExtractor<OutT> Build();
        TripletsExtractorBuilder<OutT> SetCount(int count);
        TripletsExtractorBuilder<OutT> SetFormat(OutputFormat<OutT> format);
        TripletsExtractorBuilder<OutT> SetInputStream(StreamReader stream);
        TripletsExtractorBuilder<OutT> SetOutputStream(Stream stream);
        TripletsExtractorBuilder<OutT> SetCoefficientOfProcessorUsing(float coefficient);
    }
}