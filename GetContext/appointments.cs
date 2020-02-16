namespace GetContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("mis.appointments")]
    public partial class appointments
    {
        public int id { get; set; }

        public int id_doctor { get; set; }

        public DateTime date_time { get; set; }

        public virtual doctors doctors { get; set; }
    }
}
