using System;
using System.Collections.Generic;

namespace StudentskiWebAPI.Models;

public partial class Prijava
{
    public int Id { get; set; }

    public int IdStudenta { get; set; }

    public int IdPredmeta { get; set; }

    public int IdRoka { get; set; }

    public DateTime DatumPrijave { get; set; }

    public virtual Predmet IdPredmetaNavigation { get; set; } = null!;

    public virtual IspitniRok IdRokaNavigation { get; set; } = null!;

    public virtual Student IdStudentaNavigation { get; set; } = null!;
}
