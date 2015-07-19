using Domain.Models;
using System;
using System.Collections.Generic;

namespace FMS.Models
{
    public class MiscViewModel
    {
        public int Key { get; set; }
        public string Value { get; set; }
    }

    public class MiscEnumViewModel
    {
        public Enum Key { get; set; }
        public string Value { get; set; }
    }

    public class DictioanaryMiscViewModel
    {
        public IList<MiscViewModel> Dictionary { get; set; }
    }

    public class DictioanaryMiscEnumViewModel
    {
        public IList<MiscEnumViewModel> Dictionary { get; set; }
    }

    public class DictioanaryBindModel
    {
        public string Name { get; set; }
        public DocumentType? DocType { get; set; }
        public PersonCategory? Category { get; set; }
    }
}
