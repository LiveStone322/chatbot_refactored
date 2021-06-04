using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp
{
    [Table("entities_actions")]
    class entities_actions
    {
        [Key]
        public int id { get; set; }

        public int? id_action { get; set; }

        public int? id_entity { get; set; }

        public virtual actions actions { get; set; }

        public virtual entities entities { get; set; }
    }
}
