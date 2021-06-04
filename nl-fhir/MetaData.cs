using System;
using System.Collections.Generic;
using System.Text;

namespace nl_fhir
{
    class MetaData
    {
        public bool IsPronoun { get; set; }
        public bool IsMisc { get; set; }
        public bool IsPreposition { get; set; }
        public bool IsProperSecname { get; set; }
        public bool IsProper { get; set; }
        public bool IsProperSurname { get; set; }
        public bool IsProperName { get; set; }
        public bool IsAdverb { get; set; }
        public bool IsConjunction { get; set; }
        public bool IsVerb { get; set; }
        public bool IsPersonalPronoun { get; set; }
        public bool IsNoun { get; set; }
        public bool IsUndefined { get; set; }
        public bool IsProperGeo { get; set; }
        public bool IsAdjective { get; set; }
    }
}
