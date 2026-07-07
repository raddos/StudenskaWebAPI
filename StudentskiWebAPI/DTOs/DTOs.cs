using System.ComponentModel.DataAnnotations;

namespace StudentskiWebAPI.DTOs
{
    // ── STUDENT ───────────────────────────────────────────────────────────────
    public class StudentListDto
    {
        public int    IdStudenta  { get; set; }
        public string Ime         { get; set; } = string.Empty;
        public string Prezime     { get; set; } = string.Empty;
        // smer-broj/godinaUpisa  (GodinaUpisa je string u bazi)
        public string BrojIndeksa { get; set; } = string.Empty;
    }

    public class StudentUpdateDto
    {
        [Required(ErrorMessage = "Ime je obavezno.")]
        [MaxLength(25)]
        [RegularExpression(@"^[A-Za-zÀ-žА-Яа-яĐđŠšČčĆćŽž\s\-']+$",
            ErrorMessage = "Ime smije sadržati samo slova.")]
        public string Ime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [MaxLength(40)]
        [RegularExpression(@"^[A-Za-zÀ-žА-Яа-яĐđŠšČčĆćŽž\s\-']+$",
            ErrorMessage = "Prezime smije sadržati samo slova.")]
        public string Prezime { get; set; } = string.Empty;
    }

    // ── PREDMET ───────────────────────────────────────────────────────────────
    public class PredmetDto
    {
        public int    IdPredmeta { get; set; }
        public string Naziv      { get; set; } = string.Empty;
        public short  Espb       { get; set; }
        public string Status     { get; set; } = string.Empty;
        public string Profesor   { get; set; } = string.Empty;
    }

    // ── ISPITNI ROK ───────────────────────────────────────────────────────────
    public class IspitniRokDto
    {
        public int    IdRoka     { get; set; }
        public string Naziv      { get; set; } = string.Empty;
        public string SkolskaGod { get; set; } = string.Empty;
        public string StatusRoka { get; set; } = string.Empty;
    }

    // ── ISPIT ─────────────────────────────────────────────────────────────────
    public class IspitDto
    {
        public int     IdIspita      { get; set; }
        public short   IdRoka        { get; set; }
        public short   IdPredmeta    { get; set; }
        public string  NazivPredmeta { get; set; } = string.Empty;
        public string  NazivRoka     { get; set; } = string.Empty;
        public string  SkolskaGod    { get; set; } = string.Empty;
        public string? Datum         { get; set; }  // DateOnly → string
    }

    // ── STUDENT–PREDMET ───────────────────────────────────────────────────────
    public class StudentPredmetDto
    {
        public int    IdStudenta    { get; set; }
        public int    IdPredmeta    { get; set; }
        public string SkolskaGodina { get; set; } = string.Empty;
        public string NazivPredmeta { get; set; } = string.Empty;
        public short  Espb          { get; set; }
        public bool   JePolozio     { get; set; }
    }

    public class DodajPredmetDto
    {
        [Required] public int    IdStudenta    { get; set; }
        [Required] public int    IdPredmeta    { get; set; }
        [Required] public string SkolskaGodina { get; set; } = string.Empty;
    }

    // ── PRIJAVA ISPITA (PrijavaIspitum) ───────────────────────────────────────
    // Tabela: PrijavaIspita → student prijavljuje predmet u određenom roku
    public class PrijavaCreateDto
    {
        [Required(ErrorMessage = "IdStudenta je obavezno.")]
        public int IdStudenta { get; set; }

        [Required(ErrorMessage = "IdPredmeta je obavezno.")]
        public int IdPredmeta { get; set; }

        [Required(ErrorMessage = "IdRoka je obavezno.")]
        public int IdRoka { get; set; }
    }

    public class PrijavaDto
    {
        public int      Id            { get; set; }
        public int      IdStudenta    { get; set; }
        public string   ImeStudenta   { get; set; } = string.Empty;
        public string   BrojIndeksa   { get; set; } = string.Empty;
        public int      IdPredmeta    { get; set; }
        public string   NazivPredmeta { get; set; } = string.Empty;
        public int      IdRoka        { get; set; }
        public string   NazivRoka     { get; set; } = string.Empty;
        public string   SkolskaGod    { get; set; } = string.Empty;
        public DateTime DatumPrijave  { get; set; }
    }

    // ── POLOŽENI PREDMETI + PROSEK ────────────────────────────────────────────
    public class PolozeniPredmetDto
    {
        public string  NazivPredmeta { get; set; } = string.Empty;
        public short   Espb          { get; set; }
        public float   Ocena         { get; set; }
        public string  Bodovi        { get; set; } = string.Empty;
        public string  NazivRoka     { get; set; } = string.Empty;
        public string  SkolskaGod    { get; set; } = string.Empty;
        public string? Datum         { get; set; }
    }

    public class StudentProsekDto
    {
        public int                     IdStudenta    { get; set; }
        public string                  Ime           { get; set; } = string.Empty;
        public string                  Prezime       { get; set; } = string.Empty;
        public string                  BrojIndeksa   { get; set; } = string.Empty;
        public double                  ProsekOcena   { get; set; }
        public int                     BrojPolozenih { get; set; }
        public List<PolozeniPredmetDto> Predmeti     { get; set; } = new();
    }
}
