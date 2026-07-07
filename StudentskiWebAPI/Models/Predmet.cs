using System;
using System.Collections.Generic;

namespace StudentskiWebAPI.Models;

public partial class Predmet
{
    public int IdPredmeta { get; set; }

    public short IdProfesora { get; set; }

    public string Naziv { get; set; } = null!;

    public short Espb { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<PrijavaIspitum> PrijavaIspita { get; set; } = new List<PrijavaIspitum>();
}
