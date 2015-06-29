using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class SearchQuery
    {
        [Key]
        public int Id { get; set; }
        public string Query { get; set; }
    }
}
