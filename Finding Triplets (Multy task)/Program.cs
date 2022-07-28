using Microsoft.Extensions.DependencyInjection;
using Random_Text_Generator;
using System.Diagnostics;
using System.Text;
using Triplets_Extractor;
using static Finding_Triplets__Multy_task_.StartUp;

#pragma warning disable CS8602
namespace Finding_Triplets__Multy_task_
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //using var generatedTextFileStream = File.Open(@"test1.txt", FileMode.OpenOrCreate);
            //new RandomTextGenerator().Generate(generatedTextFileStream, 1_000_000_000);

            //Console.WriteLine("Generation done.");
            //Console.ReadLine();
            //generatedTextFileStream.Close();

            var filePath = Configuration["sourceFilePath"];

            if (!ExistenceCheck(filePath))
            {
                Console.WriteLine("Sorce file is not exist");
                return;
            }

            if (!int.TryParse(Configuration["count"], out int countOfTriplets))
            {
                Console.WriteLine("Count was less or equals zero");
                return;
            }

            using var inputStream = new FileStream(filePath, FileMode.Open);
            using var outputStream = new MemoryStream();
            OutputFormat<string> outputStringFormatDelegate = OutputFormat;

            // setting and creation of extractor using fluent builder
            var extractor = AppServiceProvider.GetService<ITripletsExtractorBuilder<string>>()
                .SetInputStream(inputStream)
                .SetOutputStream(outputStream)
                .SetCount(countOfTriplets)
                .SetFormat(outputStringFormatDelegate)
                .Build();

            Stopwatch stopwatch = Stopwatch.StartNew();

            //var task = Task.Run(() => extractor.Do());
            var task = Task.Run(() => extractor.DoInMultytaskScenario());

            var taskContinuation = task.ContinueWith(t => Task.Run(async () => {
                await SendResults(outputStream, stopwatch, countOfTriplets);
            }));

            // perform some work after extractor's starting and during its working
            for (int i = 0; !taskContinuation.IsCompleted; i++)
            {
                Thread.Sleep(300);  // freezing for demonstration purposes
                Console.Write("-");
            }
        }

        private static async Task SendResults(MemoryStream outputStream, Stopwatch stopwatch, int tripletsCount)
        {
            Console.WriteLine($"\nMost popular {tripletsCount} triplets in file:");
            switch (Configuration["output"])
            {
                case "console":
                    var str = Encoding.Default.GetString(outputStream.ToArray());
                    Console.Write(str);
                    break;
                case "file":
                    await File.WriteAllBytesAsync(Configuration["resultFilePath"], outputStream.ToArray());
                    break;
                //
                // some more options
                //
                default:
                    break;
            }

            Console.WriteLine("extraction done");
            stopwatch.Stop();
            Console.WriteLine($"execution time is: {stopwatch.ElapsedMilliseconds} ms");
        }

        private static string OutputFormat(byte firstChar, byte secondChar, byte thirdChar, int count)
        {
            return new string(new char[] { (char)firstChar, (char)secondChar, (char)thirdChar })
                + " " + count
                + Environment.NewLine;
        }

        private static bool ExistenceCheck(string filePath)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);

            if (!File.Exists(fullPath))
            {
                Console.WriteLine("File not found.");
                return false;
            }

            return true;
        }
    }
}