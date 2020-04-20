namespace GetContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("public.system_messages")]
    public partial class system_messages
    {
        public int id { get; set; }

        [Required]
        [StringLength(8000)]
        public string message { get; set; }
    }
}
