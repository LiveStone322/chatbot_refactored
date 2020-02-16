namespace GetContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("mis.conclusions")]
    public partial class conclusions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public conclusions()
        {
            cures_in_conclusions = new HashSet<cures_in_conclusions>();
        }

        public int id { get; set; }

        public int id_user { get; set; }

        public int id_doctor { get; set; }

        public int? date { get; set; }

        [StringLength(8000)]
        public string recommendations { get; set; }

        public virtual doctors doctors { get; set; }

        public virtual users users { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cures_in_conclusions> cures_in_conclusions { get; set; }
    }
}
