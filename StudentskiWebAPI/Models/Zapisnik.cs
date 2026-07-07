using System;
using System.Collections.Generic;

namespace StudentskiWebAPI.Models;

public partial class Zapisnik
{
    public int IdStudenta { get; set; }

    public int IdIspita { get; set; }

    public float Ocena { get; set; }

    public string Bodovi { get; set; } = null!;
}
