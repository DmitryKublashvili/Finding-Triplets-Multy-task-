using System.Text;

namespace Random_Text_Generator
{
    public class RandomTextGenerator
    {
        public void Generate(Stream outPutStream, long countOfsymbols)
        {
            var random = new Random();

            for (int i = 0; i < countOfsymbols; i++)
            {
                outPutStream.WriteByte((byte)random.Next(60, 150));
            }
        }
    }
}
