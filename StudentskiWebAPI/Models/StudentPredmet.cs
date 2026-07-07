using System;
using System.Collections.Generic;

namespace StudentskiWebAPI.Models;

public partial class StudentPredmet
{
    public int IdStudenta { get; set; }

    public int IdPredmeta { get; set; }

    public string SkolskaGodina { get; set; } = null!;
}
