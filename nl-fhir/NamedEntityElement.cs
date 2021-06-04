using System;
using System.Collections.Generic;
using System.Text;

namespace nl_fhir
{
    class NamedEntityElement
    {
        public string Type { get; set; }
        public string Name { get; set; }

        public NamedEntityElement(string type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}
