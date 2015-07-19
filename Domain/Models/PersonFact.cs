using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{  
    public class PersonFact
    {
        [Key]
        public int Id { get; set; }
        [Index]
        public int FactId { get; set; }
        [Index]
        public int PersonId { get; set; }
        [Index]
        public DateTime FactDate { get; set; }
        [Index]
        public int? IntValue { get; set; }

        [MaxLength(1024)]
        public string StringValue { get; set; }

        public DateTime? DateValue { get; set; }

        public float? FloatValue { get; set; }

        public Person Person { get; set; }

        public ParameterName Fact { get; set; }
    }
}
