using System;
using System.Collections.Generic;

namespace StudentskiWebAPI.Models;

public partial class Student
{
    public int IdStudenta { get; set; }

    public string Ime { get; set; } = null!;

    public string Prezime { get; set; } = null!;

    public string Smer { get; set; } = null!;

    public short Broj { get; set; }

    public string GodinaUpisa { get; set; } = null!;

    public virtual ICollection<PrijavaIspitum> PrijavaIspita { get; set; } = new List<PrijavaIspitum>();
}
