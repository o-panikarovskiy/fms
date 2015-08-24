using System;
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

		[Index]
		[Required]
		[MaxLength(255)]
		public string Name { get; set; }

		[Index]
		[Required]
		public ParameterType Type { get; set; }
		[Index]
		[Required]
		public ParameterCategory Category { get; set; }
		[Index]
		public DocumentType? DocType { get; set; }
		[Index]
		public PersonCategory? PersonCategory { get; set; }

		public bool CanRemove { get; set; }

		public bool IsFact { get; set; }

		public float OrderIndex { get; set; }

		public int? MiscParentId { get; set; }

		public virtual MiscName MiscParent { get; set; }

		public ParameterName()
		{
			
		}
	}

}
