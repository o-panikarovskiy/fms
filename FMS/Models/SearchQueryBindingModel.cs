using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Models
{
    public class PersonQueryBindingModel
    {
        public string Name { get; set; }
        public DateTime? Birthday { get; set; }
        public PersonType? Type { get; set; }
        public PersonCategory? Category { get; set; }
        public int? Citizenship { get; set; }
        public string Address { get; set; }
        public int? DocType { get; set; }
        public string DocNo { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class DocumentBindingModel
    {
        public string DocNo { get; set; }
        public bool IsChecked { get; set; }
    }

    public class AdminPracticeDocBindingModel : DocumentBindingModel
    {        
        public DateTime? StDateCreate { get; set; }
        public DateTime? EndDateCreate { get; set; }
        public int? Article { get; set; }
        public int? CrimeType { get; set; }
        public int? StateDepartment { get; set; }
        public int? DocStatus { get; set; }
        public DateTime? StDecreeDate { get; set; }
        public DateTime? EndDecreeDate { get; set; }
        public int? DecreeStr { get; set; }
        public int? PenaltyType { get; set; }
    }

    public class SearchDocumnet
    {
        public AdminPracticeDocBindingModel Ap { get; set; }
    }

    public class SearchQueryBindingModel
    {
        public PersonQueryBindingModel Person { get; set; } 
        public SearchDocumnet Docs { get; set; }
    }
}
