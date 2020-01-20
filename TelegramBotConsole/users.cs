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
            files = new HashSet<files>();
            questions_answers = new HashSet<questions_answers>();
        }

        [StringLength(8000)]
        public string id { get; set; }

        [Required]
        [StringLength(8000)]
        public string login { get; set; }

        [StringLength(8000)]
        public string fio { get; set; }

        [StringLength(12)]
        public string phone_number { get; set; }

        public int? id_last_question { get; set; }

        public int id_source { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<files> files { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<questions_answers> questions_answers { get; set; }

        public virtual sources sources { get; set; }
    }
}
