using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentskiWebAPI.Data;
using StudentskiWebAPI.DTOs;

namespace StudentskiWebAPI.Controllers
{
    // ── PREDMETS ──────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class PredmetsController : ControllerBase
    {
        private readonly StudentskiContext _ctx;
        public PredmetsController(StudentskiContext ctx) => _ctx = ctx;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Učitaj u memoriju pa joini – izbjegava EF type-cast problem
            var predmeti  = await _ctx.Predmets.ToListAsync();
            var profesori = await _ctx.Profesors.ToListAsync();

            var rezultat = predmeti
                .OrderBy(p => p.Naziv)
                .Select(p =>
                {
                    var pr = profesori.FirstOrDefault(x => x.IdProfesora == (int)p.IdProfesora);
                    return new PredmetDto
                    {
                        IdPredmeta = p.IdPredmeta,
                        Naziv      = p.Naziv,
                        Espb       = p.Espb,
                        Status     = p.Status,
                        Profesor   = pr is not null ? $"{pr.Ime} {pr.Prezime}" : "—"
                    };
                })
                .ToList();

            return Ok(rezultat);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _ctx.Predmets.FindAsync(id);
            if (p is null)
                return NotFound(new { poruka = $"Predmet ID={id} nije pronađen." });

            var pr = await _ctx.Profesors.FindAsync((int)p.IdProfesora);
            return Ok(new PredmetDto
            {
                IdPredmeta = p.IdPredmeta,
                Naziv      = p.Naziv,
                Espb       = p.Espb,
                Status     = p.Status,
                Profesor   = pr is not null ? $"{pr.Ime} {pr.Prezime}" : "—"
            });
        }
    }

    // ── ISPITNI ROKOVI ────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class IspitniRoksController : ControllerBase
    {
        private readonly StudentskiContext _ctx;
        public IspitniRoksController(StudentskiContext ctx) => _ctx = ctx;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rokovi = await _ctx.IspitniRoks
                .OrderByDescending(r => r.SkolskaGod)
                .ThenBy(r => r.Naziv)
                .Select(r => new IspitniRokDto
                {
                    IdRoka     = r.IdRoka,
                    Naziv      = r.Naziv,
                    SkolskaGod = r.SkolskaGod,
                    StatusRoka = r.StatusRoka
                })
                .ToListAsync();
            return Ok(rokovi);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var r = await _ctx.IspitniRoks.FindAsync(id);
            return r is null
                ? NotFound(new { poruka = $"Rok ID={id} nije pronađen." })
                : Ok(new IspitniRokDto
                {
                    IdRoka     = r.IdRoka,
                    Naziv      = r.Naziv,
                    SkolskaGod = r.SkolskaGod,
                    StatusRoka = r.StatusRoka
                });
        }
    }

    // ── ISPITI ────────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class IspitsController : ControllerBase
    {
        private readonly StudentskiContext _ctx;
        public IspitsController(StudentskiContext ctx) => _ctx = ctx;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Učitaj sve u memoriju pa joini – izbjegava short/int mismatch
            var ispiti    = await _ctx.Ispits.ToListAsync();
            var predmeti  = await _ctx.Predmets.ToListAsync();
            var rokovi    = await _ctx.IspitniRoks.ToListAsync();

            var rezultat = ispiti
                .Select(i =>
                {
                    var p = predmeti.FirstOrDefault(x => x.IdPredmeta == (int)i.IdPredmeta);
                    var r = rokovi.FirstOrDefault(x => x.IdRoka       == (int)i.IdRoka);
                    return new IspitDto
                    {
                        IdIspita      = i.IdIspita,
                        IdRoka        = i.IdRoka,
                        IdPredmeta    = i.IdPredmeta,
                        NazivPredmeta = p?.Naziv      ?? "—",
                        NazivRoka     = r?.Naziv      ?? "—",
                        SkolskaGod    = r?.SkolskaGod ?? "—",
                        Datum         = i.Datum.ToString()
                    };
                })
                .OrderByDescending(i => i.SkolskaGod)
                .ThenBy(i => i.NazivPredmeta)
                .ToList();

            return Ok(rezultat);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var i = await _ctx.Ispits.FindAsync(id);
            if (i is null)
                return NotFound(new { poruka = $"Ispit ID={id} nije pronađen." });

            var p = await _ctx.Predmets.FindAsync((int)i.IdPredmeta);
            var r = await _ctx.IspitniRoks.FindAsync((int)i.IdRoka);

            return Ok(new IspitDto
            {
                IdIspita      = i.IdIspita,
                IdRoka        = i.IdRoka,
                IdPredmeta    = i.IdPredmeta,
                NazivPredmeta = p?.Naziv      ?? "—",
                NazivRoka     = r?.Naziv      ?? "—",
                SkolskaGod    = r?.SkolskaGod ?? "—",
                Datum         = i.Datum.ToString()
            });
        }
    }

    // ── ZAPISNICI ─────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class ZapisniksController : ControllerBase
    {
        private readonly StudentskiContext _ctx;
        public ZapisniksController(StudentskiContext ctx) => _ctx = ctx;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var zapisnici = await _ctx.Zapisniks
                .Select(z => new { z.IdIspita, z.IdStudenta, z.Ocena, z.Bodovi })
                .ToListAsync();
            return Ok(zapisnici);
        }
    }
}

    // ── DIJAGNOSTIKA (privremeno) ─────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class DiagController : ControllerBase
    {
        private readonly StudentskiContext _ctx;
        public DiagController(StudentskiContext ctx) => _ctx = ctx;

        [HttpGet]
        public async Task<IActionResult> Test()
        {
            var rezultat = new Dictionary<string, object>();

            try { rezultat["students_count"]    = await _ctx.Students.CountAsync(); }
            catch (Exception e) { rezultat["students_error"]    = e.Message + " | " + e.InnerException?.Message; }

            try { rezultat["predmets_count"]    = await _ctx.Predmets.CountAsync(); }
            catch (Exception e) { rezultat["predmets_error"]    = e.Message + " | " + e.InnerException?.Message; }

            try { rezultat["rokovi_count"]      = await _ctx.IspitniRoks.CountAsync(); }
            catch (Exception e) { rezultat["rokovi_error"]      = e.Message + " | " + e.InnerException?.Message; }

            try { rezultat["ispiti_count"]      = await _ctx.Ispits.CountAsync(); }
            catch (Exception e) { rezultat["ispiti_error"]      = e.Message + " | " + e.InnerException?.Message; }

            try { rezultat["zapisnici_count"]   = await _ctx.Zapisniks.CountAsync(); }
            catch (Exception e) { rezultat["zapisnici_error"]   = e.Message + " | " + e.InnerException?.Message; }

            try { rezultat["studpred_count"]    = await _ctx.StudentPredmets.CountAsync(); }
            catch (Exception e) { rezultat["studpred_error"]    = e.Message + " | " + e.InnerException?.Message; }

            try { rezultat["prijave_count"]     = await _ctx.PrijavaIspita.CountAsync(); }
            catch (Exception e) { rezultat["prijave_error"]     = e.Message + " | " + e.InnerException?.Message; }

            try { rezultat["profesori_count"]   = await _ctx.Profesors.CountAsync(); }
            catch (Exception e) { rezultat["profesori_error"]   = e.Message + " | " + e.InnerException?.Message; }

            return Ok(rezultat);
        }
    }
