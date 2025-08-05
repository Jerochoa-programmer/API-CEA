using System;
using System.Collections.Generic;

namespace CEA_API.Models;

public partial class CeaPermissionsGranted
{
    public int IdPermissionGranted { get; set; }

    public int IdPermissionGrantedUser { get; set; }

    public int IdPermissionGrantedPermission { get; set; }

    public virtual CeaPermission IdPermissionGrantedPermissionNavigation { get; set; } = null!;

    public virtual CeaUser IdPermissionGrantedUserNavigation { get; set; } = null!;
}
