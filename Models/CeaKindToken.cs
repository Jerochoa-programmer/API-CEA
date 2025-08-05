using System;
using System.Collections.Generic;

namespace CEA_API.Models;

public partial class CeaKindToken
{
    public short IdKindToken { get; set; }

    public string NameKindToken { get; set; } = null!;

    public virtual ICollection<CeaToken> CeaTokens { get; set; } = new List<CeaToken>();
}
