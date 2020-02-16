namespace GetContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("public.questions_answers")]
    public partial class questions_answers
    {
        public int id { get; set; }

        [Required]
        [StringLength(8000)]
        public string id_user { get; set; }

        public int id_question { get; set; }

        [StringLength(8000)]
        public string value { get; set; }

        public DateTime date_time { get; set; }

        public virtual questions questions { get; set; }

        public virtual users1 users1 { get; set; }
    }
}
