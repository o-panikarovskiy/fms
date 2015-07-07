﻿using System;
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
        [MaxLength(255)]
        [Index("IX_NameAndBirthday", 2, IsUnique = true)]
        public string Name { get; set; }

        [Required]
        [Index("IX_NameAndBirthday", 1, IsUnique = true)]
        public DateTime Birthday { get; set; }

        [Required]
        public PersonCategory Category { get; set; }

        [Required]
        public PersonType Type { get; set; }
    }
}
