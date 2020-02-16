namespace GetContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("mis.cures")]
    public partial class cures
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public cures()
        {
            cures_in_conclusions = new HashSet<cures_in_conclusions>();
        }

        public int id { get; set; }

        [Required]
        [StringLength(8000)]
        public string name { get; set; }

        [Required]
        [StringLength(8000)]
        public string developer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cures_in_conclusions> cures_in_conclusions { get; set; }
    }
}
