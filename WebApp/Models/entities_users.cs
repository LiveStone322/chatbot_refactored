using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp
{
    [Table("entities_users")]
    class entities_users
    {
        [Key]
        public int id { get; set; }

        public int? id_user { get; set; }

        public int? id_entity { get; set; }

        [StringLength(8000)]
        public string vale { get; set; }

        public DateTime date_time { get; set; }

        public virtual entities entities { get; set; }

        public virtual users users { get; set; }
    }
}
