using System.Collections.Generic;

namespace FMS.Models
{
    public class MiscViewModel
    {
        public int Key { get; set; }
        public string Value { get; set; }
    }

    public class DictioanryMiscViewModel
    {
        public IList<MiscViewModel> Dictionary { get; set; }
    }
}
