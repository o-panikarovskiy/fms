using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public enum DocumentType : byte
    {
        AdministrativePractice = 1,
        TemporaryResidencePermit = 2,
        Residence = 3,
        Citizenship = 4,
        MigrationRegistration = 5
    }

    public class Document
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Number { get; set; }

        [Index]
        public DocumentType Type { get; set; }
        [Index]
        public int? ApplicantPersonId { get; set; }
        [Index]
        public int? HostPersonId { get; set; }

        public string CreatedById { get; set; }

        public string UpdatedById { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public Person ApplicantPerson { get; set; }
        public Person HostPerson { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public ApplicationUser UpdatedBy { get; set; }                
        public virtual ICollection<DocumentParameter> Parameters { get; set; }
    }
}
