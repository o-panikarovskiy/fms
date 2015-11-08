using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Models
{
    public class SearchPersonBindingModel
    {
        public string Name { get; set; }
        public DateTime? StBirthday { get; set; }
		public DateTime? EndBirthday { get; set; }
		public PersonType? Type { get; set; }
        public PersonCategory? Category { get; set; }
        public int? Citizenship { get; set; }
        public string Address { get; set; }
        public int? DocType { get; set; }
        public string DocNo { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

	public class SearchDocParamBindingModel
	{
		public object Value { get; set; }
		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }
		public ParameterType Type { get; set; }
	}


	public class SearchDocsBindingModel
	{
		public bool IsChecked { get; set; }
		public string DocNo { get; set; }        
		public Dictionary<int, SearchDocParamBindingModel> DocParams { get; set; }
	}    


	public class SearchQueryBindingModel
    {
        public SearchPersonBindingModel Person { get; set; }
        public Dictionary<DocumentType, SearchDocsBindingModel> Docs { get; set; }
		
	}
}
