using System;
using System.Collections.Generic;

namespace CEA_API.Models;

public partial class CeaRolesUser
{
    public short IdRolUser { get; set; }

    public string NameRolUser { get; set; } = null!;

    public virtual ICollection<CeaUser> CeaUsers { get; set; } = new List<CeaUser>();
}
