using System;
using System.Collections.Generic;

namespace CEA_API.Models;

public partial class CeaMovementTraceability
{
    public short IdMovement { get; set; }

    public string NameMovement { get; set; } = null!;

    public virtual ICollection<CeaTraceability> CeaTraceabilities { get; set; } = new List<CeaTraceability>();
}
