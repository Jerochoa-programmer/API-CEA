using System;
using System.Collections.Generic;

namespace CEA_API.Models;

public partial class CeaTraceability
{
    public int IdTraceability { get; set; }

    public int IdTraceabilityUser { get; set; }

    public short IdTraceabilityMovementTraceability { get; set; }

    public DateOnly TraceabilityDate { get; set; }

    public TimeOnly TraceabilityTime { get; set; }

    public virtual CeaMovementTraceability IdTraceabilityMovementTraceabilityNavigation { get; set; } = null!;
}
