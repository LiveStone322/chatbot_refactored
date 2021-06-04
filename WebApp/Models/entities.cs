using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp
{
    [Table("entities")]
    class entities
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public entities()
        {
            questions = new HashSet<questions>();
            entities_users = new HashSet<entities_users>();
            entities_actions = new HashSet<entities_actions>();
        }

        [Key]
        public int id { get; set; }

        [StringLength(8000)]
        public string name { get; set; }

        [StringLength(8000)]
        public string format { get; set; }

        [StringLength(8000)]
        public string splitter { get; set; }

        public bool? scalable { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<questions> questions { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<entities_users> entities_users { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<entities_actions> entities_actions { get; set; }
    }
}
