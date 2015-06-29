using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public enum PrmFactType { Misc = 1, Str = 2, Date = 3, Float = 4 }
    public enum PrmFactCategory { Document = 1, Person = 2 }
    public class PrmFactName
    {
        [Key]
        public int Id { get; set; }

        [Index("PrmFactNameRuIndex", IsUnique = true)]
        [MaxLength(255)]
        public string NameRu { get; set; }

        [Index("PrmFactNameEnIndex", IsUnique = true)]
        [MaxLength(255)]
        public string Name { get; set; }

        public PrmFactType Type { get; set; }

        public PrmFactCategory Category { get; set; }
    }

}
