namespace GetContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("mis.users")]
    public partial class users
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public users()
        {
            conclusions = new HashSet<conclusions>();
        }

        public int id { get; set; }

        [StringLength(8000)]
        public string telegram_name { get; set; }

        [StringLength(8000)]
        public string viber_name { get; set; }

        [StringLength(8000)]
        public string phone { get; set; }

        [StringLength(8000)]
        public string name { get; set; }

        [StringLength(8000)]
        public string surname { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<conclusions> conclusions { get; set; }
    }
}
