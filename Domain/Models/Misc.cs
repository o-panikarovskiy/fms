using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class MiscName
    {
        [Key]
        public int Id { get; set; }

        [Index("MiscNameIndex", IsUnique = true)]
        [MaxLength(255)]
        public string NameRu { get; set; }

        [Index("MiscNameEnIndex", IsUnique = true)]
        [MaxLength(255)]
        public string Name { get; set; }
    }

    public class Misc
    {
        [Key]
        public int Id { get; set; }
        public int MiscId { get; set; }

        [MaxLength(1024)]
        public string MiscValue { get; set; }
        public MiscName MiscParent { get; set; }

    }
}
