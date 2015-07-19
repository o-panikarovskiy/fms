using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class PersonParameter
    {
        [Key]
        public int Id { get; set; }
        [Index]
        public int ParameterId { get; set; }
        [Index]
        public int PersonId { get; set; }
        [Index]
        public int? IntValue { get; set; }

        [MaxLength(1024)]
        public string StringValue { get; set; }

        public DateTime? DateValue { get; set; }

        public float? FloatValue { get; set; }

        public Person Person { get; set; }

        public ParameterName Parameter { get; set; }
    }
}
