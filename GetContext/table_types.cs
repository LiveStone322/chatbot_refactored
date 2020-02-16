namespace GetContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("public.table_types")]
    public partial class table_types
    {
        public int id { get; set; }

        [Required]
        [StringLength(8000)]
        public string message { get; set; }
    }
}
