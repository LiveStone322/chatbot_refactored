using Nl_fhirML.Model;
using System;
using System.Linq;

namespace nl_fhir
{
    public class NLResult
    {
        public ActionsEnum.Actions Result { get; set; }
        public Keyword[] Keywords { get; set; }
        public string Text { get; set; }
        public double Score { get; set; }
        public double ConfidenceScore { get; set; }

        public NLResult(ModelOutput res, string text, Keyword[] keywords = null)
        {
            Result = ActionsEnum.GetEnumFromText(res.Prediction);
            Text = text;

            Keywords = keywords != null 
                ? keywords.ToArray()
                : new Keyword[] { };

            double[] sortedRes = new double[res.Score.Length];
            res.Score.CopyTo(sortedRes, 0);
            Array.Sort(sortedRes, (a, b) => b.CompareTo(a));

            if (sortedRes.Length > 0) Score = sortedRes[0];
            else Score = 0;

            ConfidenceScore = (Score - (sortedRes.Length > 1 ? sortedRes[1] : 0)) / Score;
        }

        public NLResult(ActionsEnum.Actions action, string text, double score, double conf = 1)
        {
            Result = action;

            Score = score;

            Text = text;

            ConfidenceScore = conf;
        }
    }
}
