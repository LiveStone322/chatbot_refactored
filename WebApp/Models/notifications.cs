using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp
{

    [Table("public.notifications")]
    class notifications
    {
        public int id { get; set; }

        [StringLength(8000)]
        public string id_user { get; set; }

        public DateTime? on_time { get; set; }

        public int? is_read { get; set; }

        [Required]
        [StringLength(8000)]
        public string message { get; set; }
    }
}
