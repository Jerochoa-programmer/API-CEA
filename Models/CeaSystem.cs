using System;
using System.Collections.Generic;

namespace CEA_API.Models;

public partial class CeaSystem
{
    public int IdSystem { get; set; }

    public string NameSystem { get; set; } = null!;

    public string? LinkSystems { get; set; }

    public virtual ICollection<CeaPermission> CeaPermissions { get; set; } = new List<CeaPermission>();
}
