using Pullenti.Ner;
using System;
using System.Collections.Generic;
using System.Text;

namespace nl_fhir
{
    class NamedEntity
    {
        public Referent Referent { get; set; }
        public NamedEntityElement[] Elements { get; set; }

        public NamedEntity(Referent refer, NamedEntityElement[] elements)
        {
            Referent = refer;
            Elements = elements;
        }
    }
}
