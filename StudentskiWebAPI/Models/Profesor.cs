using System;
using System.Collections.Generic;

namespace StudentskiWebAPI.Models;

public partial class Profesor
{
    public int IdProfesora { get; set; }

    public string Ime { get; set; } = null!;

    public string Prezime { get; set; } = null!;

    public string Zvanje { get; set; } = null!;

    public DateOnly DatumZap { get; set; }
}
