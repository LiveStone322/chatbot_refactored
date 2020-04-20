using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp
{
    [Table("biomarks")]
    class biomarks
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public biomarks()
        {
            questions = new HashSet<questions>();
            questions_answers = new HashSet<questions_answers>();
            users_biomarks = new HashSet<users_biomarks>();
        }

        public int id { get; set; }

        [Required]
        [StringLength(8000)]
        public string name { get; set; }

        [StringLength(8000)]
        public string format { get; set; }

        [StringLength(8000)]
        public string splitter { get; set; }

        public bool scalable { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<questions> questions { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<questions_answers> questions_answers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<users_biomarks> users_biomarks { get; set; }
    }
}
