using System.Collections.Generic;

namespace FMS.Models
{
    public class UserInfoModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public IList<string> Roles { get; set; }
    }
}
