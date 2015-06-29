using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Models
{
    public class GroupPersonFact
    {
        public int PersonId { get; set; }
        public int FactId { get; set; }
        public DateTime FactDate { get; set; }
    }
}
