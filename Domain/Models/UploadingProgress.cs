using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class UploadingProgress
    {
        [Key]
        public int Id { get; set; }
        public float Percent { get; set; }
        public int CurrentRow { get; set; }
        public int TotalRows { get; set; }
        [Index]
        public Guid FileId { get; set; }
        [Index]
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Index]
        public Guid UserId { get; set; }
        public bool HasErrors { get; set; }

        [MaxLength(1024)]
        public string ExceptionMessage { get; set; }
        public bool IsCompleted { get; set; }
    }


}
