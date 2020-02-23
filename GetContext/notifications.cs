namespace GetContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("public.notifications")]
    public partial class notifications
    {
        public int id { get; set; }

        public int id_user { get; set; }

        public DateTime? on_time { get; set; }

        public int? is_read { get; set; }

        [Required]
        [StringLength(8000)]
        public string message { get; set; }

        public virtual users users { get; set; }
    }
}
