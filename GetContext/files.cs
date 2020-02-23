namespace GetContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("public.files")]
    public partial class files
    {
        public int id { get; set; }

        [Required]
        [StringLength(8000)]
        public string content_hash { get; set; }

        [Required]
        [StringLength(8000)]
        public string directory { get; set; }

        public int id_user { get; set; }

        [Required]
        [StringLength(255)]
        public string file_name { get; set; }

        [StringLength(10)]
        public string file_format { get; set; }

        public DateTime time_created { get; set; }

        public DateTime time_edited { get; set; }

        public int id_source { get; set; }

        public virtual sources sources { get; set; }

        public virtual users users { get; set; }
    }
}
