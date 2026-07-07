using System;
using System.Collections.Generic;

namespace StudentskiWebAPI.Models;

public partial class Ispit
{
    public int IdIspita { get; set; }

    public short IdRoka { get; set; }

    public short IdPredmeta { get; set; }

    public DateOnly Datum { get; set; }
}
