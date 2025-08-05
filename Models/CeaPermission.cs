using System;
using System.Collections.Generic;

namespace CEA_API.Models;

public partial class CeaPermission
{
    public int IdPermission { get; set; }

    public string CodePermission { get; set; } = null!;

    public int IdSystemPermission { get; set; }

    public virtual ICollection<CeaPermissionsGranted> CeaPermissionsGranteds { get; set; } = new List<CeaPermissionsGranted>();

    public virtual CeaSystem IdSystemPermissionNavigation { get; set; } = null!;
}
