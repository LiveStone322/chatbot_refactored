using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramBotConsole
{
    [Table("users")]
    class users
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public users()
        {
            biomarkers_values = new HashSet<questions_answers>();
        }

        [Required]
        public int id { get; set; }

        [Required]
        [StringLength(8000)]
        public string login { get; set; }

        [StringLength(8000)]
        public string fio { get; set; }

        [StringLength(12)]
        public string phone_number { get; set; }
        public int? id_last_question { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<questions_answers> biomarkers_values { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<files> files { get; set; }
    }
}
