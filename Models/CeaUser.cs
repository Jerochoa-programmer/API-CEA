using System;
using System.Collections.Generic;

namespace CEA_API.Models;

public partial class CeaUser
{
    public int IdUser { get; set; }

    public string NameUser { get; set; } = null!;

    public string EmailUser { get; set; } = null!;

    public string PassUser { get; set; } = null!;

    public short IdUsersRolUser { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool? EmailState { get; set; }

    public virtual ICollection<CeaPermissionsGranted> CeaPermissionsGranteds { get; set; } = new List<CeaPermissionsGranted>();

    public virtual ICollection<CeaToken> CeaTokens { get; set; } = new List<CeaToken>();

    public virtual CeaRolesUser IdUsersRolUserNavigation { get; set; } = null!;
}
