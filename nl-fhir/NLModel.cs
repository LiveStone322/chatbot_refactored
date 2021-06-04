using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Pullenti;
using Pullenti.Ner;
using Pullenti.Morph;
using Nl_fhirML.Model;

namespace nl_fhir
{
    public class NLModel
    {
        Processor processor;

        public NLModel()
        {
            processor = InitializeProcessor();
        }

        // static List<NamedEntity> listNamedEntities = new List<NamedEntity>();

        public ActionsEnum.Actions GetActionFromText(string text)
        {
            var normMessage = GetNormalizedText(text);

            return ActionsEnum.Actions.ADD_USER;
        }

        public string GetNormalizedText(string text)
        {
            return GetNormalizedMessage(
                processor.Process(
                    new SourceOfAnalysis(
                        StopWord.StopWordsExtension.RemoveStopWords(text, "ru")
                        )
                    )
                );
        }

        private static string GetNormalizedMessage(Token initialT)
        {
            var result = "";
            for (Token t = initialT; t != null; t = t.Next)
            {
                // несуществительные игнорируем
                //if (!t.Morph.Class.IsNoun && !t.Morph.Class.IsVerb) continue;

                //пропускаем именованные сущности
                // var referent = t.GetReferent();
                // if (listNamedEntities.Any(e => e.Referent == referent)) continue;

                if (t is TextToken) result += t.GetNormalCaseText() + " ";

                //пропускаем именованные сущности
                // if (t is MetaToken) Print(((MetaToken)t).BeginToken);
            }
            return result.Trim();
        }

        private static string GetNormalizedMessage(AnalysisResult initialT)
        {
            return GetNormalizedMessage(initialT.FirstToken);
        }

        private static void Print(Token initialT)
        {
            for (Token t = initialT; t != null; t = t.Next)
            {
                // несуществительные игнорируем
                //if (!t.Morph.Class.IsNoun && !t.Morph.Class.IsVerb) continue;
                // получаем нормализованное значение

                if (t is TextToken)
                {
                    string norm = t.GetNormalCaseText();
                    string type = t.Morph.Class.ToString();
                    Console.WriteLine($"{type}: {norm} ");
                    continue;
                }
                if (t is MetaToken)
                {
                    Print(((MetaToken)t).BeginToken);
                    continue;
                }
            }
        }

        private static void Print(AnalysisResult result)
        {
            Print(result.FirstToken);
        }

        /*
        private static void FillLists(List<Referent> entities)
        {
            foreach(var e in entities)
            {
                listNamedEntities.Add(new NamedEntity(e, e.Slots
                    .Select(t => new NamedEntityElement(t.TypeName, t.Value.ToString())).ToArray()));
            }
        }
        */

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

        private static void ConsoleWriteNextQuery()
        {
            ConsoleWriteNextQuestion("Введите следующий запрос...");
        }

        private static void ConsoleWriteNextQuestion(string message)
        {
            Console.Write($"{message}\n > ");
        }
    }
}
