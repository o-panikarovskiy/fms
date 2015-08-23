using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
	public class DocumentOnControl
	{
		public DocumentType DocType { get; set; }
		public int PersonId { get; set; }
		public int DocId { get; set; }
		public string DocNo { get; set; }
		public int? DaysCount { get; set; }
		public string Note { get; set; }
		public string Name { get; set; }
    }
}
