using Pullenti;
using Pullenti.Ner;
using Pullenti.Morph;
using CsvHelper;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using CsvHelper.Configuration;
using System;

namespace MLDataGenerator
{
    class Data
    {
        public string type { get; set; }
        public string text { get; set; }
    }

    class Program
    {
        static Processor processor;
        static void Main(string[] args)
        {
            processor = InitializeProcessor();

            var listData = new List<Data>();

            // указываем путь к файлу csv
            string input = @"D:\studying\FHIR\chatbot\nl-fhirML.ConsoleApp\Data\data.csv";
            string output = @"D:\studying\FHIR\chatbot\nl-fhirML.ConsoleApp\Data\norm-data.csv";

            using (var reader = new StreamReader(input))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    NewLine = Environment.NewLine,
                    Delimiter = ",",
                    HeaderValidated = null,
                    MissingFieldFound = null
                };
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<Data>();
                    foreach(var r in records)
                    {
                        listData.Add(new Data { type = r.type, text = ProcessMessage(r.text) });
                    }
                }
            }

            using (var writer = new StreamWriter(output))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(listData);
                }
            }

        }

        private static string ProcessMessage(string message)
        {
            return GetNormalizedMessage(
                processor.Process(
                    new SourceOfAnalysis(
                        StopWord.StopWordsExtension.RemoveStopWords(message, "ru")
                        )
                    )
                );
        }

        private static string GetNormalizedMessage(Token initialT)
        {
            var result = "";
            for (Token t = initialT; t != null; t = t.Next)
            {
                if (t is TextToken) result += t.GetNormalCaseText() + " ";
            }
            return result.Trim();
        }

        private static string GetNormalizedMessage(AnalysisResult initialT)
        {
            return GetNormalizedMessage(initialT.FirstToken);
        }

        private static Processor InitializeProcessor()
        {
            Sdk.Initialize(MorphLang.RU);
            var proc = ProcessorService.CreateProcessor();

            // баганутый
            proc.AddAnalyzer(new Pullenti.Ner.Measure.MeasureAnalyzer());

            return proc;
            /*
             * Для пустого процессора
             * 
            processor.AddAnalyzer(new Pullenti.Ner.Address.AddressAnalyzer());
            processor.AddAnalyzer(new Pullenti.Ner.Date.DateAnalyzer());
            processor.AddAnalyzer(new Pullenti.Ner.Money.MoneyAnalyzer());
            processor.AddAnalyzer(new Pullenti.Ner.Named.NamedEntityAnalyzer());
            processor.AddAnalyzer(new Pullenti.Ner.Person.PersonAnalyzer());
            processor.AddAnalyzer(new Pullenti.Ner.Phone.PhoneAnalyzer());
            processor.AddAnalyzer(new Pullenti.Ner.Measure.MeasureAnalyzer());
            */
        }
    }
}
