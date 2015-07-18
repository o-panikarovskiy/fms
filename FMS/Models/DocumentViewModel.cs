using Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Models
{

    public class DocumentViewModel
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public DocumentType Type { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public Person CorrPerson { get; set; }
        public IList<ParameterViewModel> Parameters { get; set; }
    }

    public class DocumentBindModel
    {
        [Required]
        public DocumentType Type { get; set; }
        [Required]
        public int PersonFromId { get; set; }
        public int? PersonToId { get; set; }
    }
}
