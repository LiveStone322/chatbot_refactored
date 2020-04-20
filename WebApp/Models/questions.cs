using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp
{
    [Table("questions")]
    class questions
    {
        public int id { get; set; }

        public bool is_biomark { get; set; }

        [StringLength(8000)]
        public string question { get; set; }

        public int? id_biomark { get; set; }

        [StringLength(8000)]
        public string info { get; set; }

        public virtual biomarks biomarks { get; set; }
    }
}
