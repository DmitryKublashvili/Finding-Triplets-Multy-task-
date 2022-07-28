using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Globalization;
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
            CultureInfo cultureInfo = CultureInfo.InvariantCulture;

            var filePath = Configuration["sourceFilePath"];

            if (!ExistenceCheck(filePath))
            {
                Console.WriteLine("Sorce file is not exist");
                return;
            }

            if (!int.TryParse(Configuration["count"], out int countOfTriplets))
            {
                Console.WriteLine("Count value has invalid format");
                return;
            }

            if (!float.TryParse(Configuration["coefficientOfProcessorUsing"], NumberStyles.Float, cultureInfo, out float coefficientOfProcessorUsing))
            {
                Console.WriteLine("Coefficient Of Processor Using has invalid format");
                return;
            }

            using var inputStream = new StreamReader(filePath);
            using var outputStream = new MemoryStream();
            OutputFormat<string> outputStringFormatDelegate = OutputFormat;

            // setting and creation of extractor using fluent builder
            var extractor = AppServiceProvider.GetService<ITripletsExtractorBuilder<string>>()
                .SetInputStream(inputStream)
                .SetOutputStream(outputStream)
                .SetCount(countOfTriplets)
                .SetFormat(outputStringFormatDelegate)
                .SetCoefficientOfProcessorUsing(coefficientOfProcessorUsing)
                .Build();

            Stopwatch stopwatch = Stopwatch.StartNew();

            var mode = Environment.GetEnvironmentVariable("mode");

            Task task;
            switch (mode)
            {
                case "multy":
                    task = Task.Run(() => {
                            Console.WriteLine("Multytask scenario");
                            extractor.DoInMultytaskScenario();
                    });
                    break;
                default:
                    task = Task.Run(() => {
                        Console.WriteLine("Single thread scenario");
                        extractor.Do();
                    });
                    break;
            }

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
                    Console.WriteLine("The result is recorded in the file");
                    break;
                //
                // some more options
                //
                default:
                    break;
            }

            Console.WriteLine("Extraction done");
            stopwatch.Stop();
            Console.WriteLine($"Execution time is: {stopwatch.ElapsedMilliseconds} ms");
        }

        private static string OutputFormat(char firstChar, char secondChar, char thirdChar, int count)
        {
            return new string(new char[] { firstChar, secondChar, thirdChar })
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