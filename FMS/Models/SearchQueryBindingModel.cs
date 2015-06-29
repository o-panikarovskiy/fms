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
    }
    public class SearchQueryBindingModel
    {
        public PersonQueryBindingModel Person { get; set; }
    }
}
