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
using Pullenti.Semantic;
using Pullenti.Ner.Keyword;

namespace nl_fhir
{
    public struct Keyword
    {
        public string Value;
        public int Position;
    }

    public class NLModel
    {
        Processor processor;
        ConsumeModel model;

        public NLModel()
        {
            processor = InitializeProcessor();
            model = new ConsumeModel();
        }

        public NLResult[] GetActionsFromText(string text)
        {
            text = "запиши давление 75 на 80 и еще температуру 35,5, настроение плохое. Еще добавь что-нибудь о моем тетрохлобине. Добавь пациента Олега Хуева. Ну и введи токен приложения 14asd-1as45d1-a65s1d51";
            var infos = GetTextInfos(text);
            var listResults = new List<NLResult>();

            foreach(var i in infos)
            {
                listResults.Add(new NLResult(ConsumeModel.Predict(new ModelInput() { Text = i.Item1 }), i.Item3.Trim(), i.Item2));
            }

            return listResults.ToArray();
        }

        public Tuple<string, Keyword[], string>[] GetTextInfos(string text)
        {
            var ar = processor.Process( new SourceOfAnalysis(StopWord.StopWordsExtension.RemoveStopWords(text, "ru")));
            var sem = SemanticService.Process(ar);
            var listIntents = new List<Tuple<string, Keyword[], string>>();

            foreach (var b in sem.Blocks)
                foreach (var f in b.Fragments)
                {
                    listIntents.Add(GetTextInfoInFragment(f));
                }
            return listIntents.ToArray();
        }

        private static Tuple<string, Keyword[], string> GetTextInfoInFragment(SemFragment frag, List<Keyword> keywords = null)
        {
            if (keywords == null) keywords = new List<Keyword>();
            var added = false;
            var value = "";
            var result = "";
            var resultForEntities = "";
            var skipping = ((ReferentToken)frag.BeginToken.Kit.FirstToken).BeginToken != frag.BeginToken;
            if (frag.BeginToken != null && frag.BeginToken.Kit != null && frag.BeginToken.Kit.FirstToken != null)
            {
                for (var t = frag.BeginToken.Kit.FirstToken; t != frag.EndToken.Next && t != null; t = t.Next)
                {
                    if (skipping)
                    {
                        if (t.Next is ReferentToken) skipping = ((ReferentToken)t.Next).BeginToken != frag.BeginToken;
                        else skipping = t.Next != frag.BeginToken;
                        continue;
                    }
                    var refer = t.GetReferent();
                    if (refer is KeywordReferent && ((KeywordReferent)refer).ChildWords < 2)
                        if ((refer as KeywordReferent).Typ == KeywordType.Object)
                        {
                            value = (refer as KeywordReferent).Value;
                            added = true;
                        }

                    if (t is NumberToken || t is ReferentToken || t is TextToken)
                    {
                        if (added)
                        {
                            keywords.Add(new Keyword() { Value = value, Position = resultForEntities.Length });
                            added = false;
                        }
                        if (t is TextToken && resultForEntities.Length > 0 && resultForEntities[resultForEntities.Length - 1] == ' ')
                            resultForEntities = resultForEntities.Substring(0, resultForEntities.Length - 1);
                        resultForEntities += (t is TextToken && t.Previous != null && t.Next != null && (t.Previous.GetType() == t.Next.GetType())) 
                            ? t.GetNormalCaseText() 
                            : t.GetNormalCaseText() + " ";
                    }

                    
                }
            }
            resultForEntities = resultForEntities.Trim();
            return new Tuple<string, Keyword[], string>(result, keywords.ToArray(), resultForEntities);
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

            // баганутый немного. Аккуратно
            proc.AddAnalyzer(new Pullenti.Ner.Measure.MeasureAnalyzer());
            proc.AddAnalyzer(new Pullenti.Ner.Keyword.KeywordAnalyzer());
            proc.AddAnalyzer(new Pullenti.Ner.Keyword.KeywordAnalyzer());

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
