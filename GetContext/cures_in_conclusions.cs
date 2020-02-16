namespace GetContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("mis.cures_in_conclusions")]
    public partial class cures_in_conclusions
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id_conclusion { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id_cure { get; set; }

        [Required]
        [StringLength(8000)]
        public string measure { get; set; }

        public int doze { get; set; }

        public int times { get; set; }

        [Required]
        [StringLength(8000)]
        public string period { get; set; }

        [StringLength(8000)]
        public string additional { get; set; }

        public virtual conclusions conclusions { get; set; }

        public virtual cures cures { get; set; }
    }
}
