using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp
{
    [Table("knowledge_base")]
    class knowledge_base
    {
        public int id { get; set; }

        [StringLength(8000)]
        public string activity { get; set; }

        [StringLength(8000)]
        public string entity { get; set; }

        [StringLength(8000)]
        public string output { get; set; }
    }
}