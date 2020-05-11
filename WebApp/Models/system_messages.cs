using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp
{
    [Table("system_messages")]
    public class system_messages
    {
        public int id { get; set; }

        [Required]
        [StringLength(8000)]
        public string message { get; set; }
    }
}
