using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Models
{
    public class PersonViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        public PersonCategory Category { get; set; }
        public PersonType Type { get; set; }
        public IDictionary<string, FactViewModel> Facts { get; set; }
        public IDictionary<string, ParameterViewModel> Parameters { get; set; }
        public IDictionary<string, int> DocsCount { get; set; }

        public PersonViewModel()
        {
            DocsCount = new Dictionary<string, int>();
        }
        public PersonViewModel(Person p) : this()
        {
            Id = p.Id;
            Name = p.Name;
            Birthday = p.Birthday;
            Category = p.Category;
            Type = p.Type;
        }
    }
}
