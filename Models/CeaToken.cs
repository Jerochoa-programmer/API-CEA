using System;
using System.Collections.Generic;

namespace CEA_API.Models;

public partial class CeaToken
{
    public int IdToken { get; set; }

    public int IdTokenUser { get; set; }

    public long Token { get; set; }

    public short IdTokenKindToken { get; set; }

    public virtual CeaKindToken IdTokenKindTokenNavigation { get; set; } = null!;

    public virtual CeaUser IdTokenUserNavigation { get; set; } = null!;
}
