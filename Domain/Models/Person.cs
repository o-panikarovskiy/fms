using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{  /// <summary>
   /// Физическое/юридическое лицо
   /// </summary>
    public enum PersonCategory : byte
    {
        Individual = 1,
        Legal = 2
    }
    /// <summary>
    /// Соискатель/принимающая сторона
    /// </summary>
    public enum PersonType : byte
    {
        Applicant = 1,
        Host = 2
    }

    public class Person
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Index]
        [MaxLength(255)]    
        public string Name { get; set; }
        
        [Index]
        public DateTime? Birthday { get; set; }

        [MaxLength(50)]
        [Index]
        public string Code { get; set; }

        [Required]
        [Index]
        public PersonCategory Category { get; set; }

        [Required]
        [Index]
        public PersonType Type { get; set; }
    }
}
