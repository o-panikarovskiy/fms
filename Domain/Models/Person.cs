using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public enum PersonCategory : byte
    {
        Individual = 1,
        Legal = 2
    }
    public enum PersonType : byte
    {
        Applicant = 1,
        Host = 2
    }

    public class Person
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public DateTime Birthday { get; set; }
        /// <summary>
        /// Физическое/юридическое лицо
        /// </summary>
        public PersonCategory Category { get; set; }
        /// <summary>
        /// Соискатель/принимающая сторона
        /// </summary>
        public PersonType Type { get; set; }
    }
}
