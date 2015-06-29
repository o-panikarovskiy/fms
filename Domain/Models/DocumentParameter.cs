using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
      public class DocumentParameter
    {
        [Key]
        public int Id { get; set; }

        public int ParameterId { get; set; }

        public int DocumentId { get; set; }

        public int? IntValue { get; set; }

        [MaxLength(1024)]
        public string StringValue { get; set; }

        public DateTime? DateValue { get; set; }

        public float? FloatValue { get; set; }

        public Document Document { get; set; }

        public PrmFactName Parameter { get; set; }
    }
}
