using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public enum ParameterType { Misc = 1, Str = 2, Date = 3, Float = 4 }
    public enum ParameterCategory { Document = 1, Person = 2 }
    public class ParameterName
    {
        [Key]
        public int Id { get; set; }
      
        [MaxLength(255)]
        public string Name { get; set; }

        public ParameterType Type { get; set; }

        public ParameterCategory Category { get; set; }

        public DocumentType? DocType { get; set; }

        public PersonCategory? PersonCategory { get; set; }

        public bool IsFact { get; set; }

        public float OrderIndex { get; set; }

        public int? MiscParentId { get; set; }

        public virtual MiscName MiscParent { get; set; }
    }

}
