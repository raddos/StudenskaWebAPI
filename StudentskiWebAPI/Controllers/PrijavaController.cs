using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentskiWebAPI.Data;
using StudentskiWebAPI.DTOs;
using StudentskiWebAPI.Models;

namespace StudentskiWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PrijavaController : ControllerBase
    {
        private readonly StudentskiContext _ctx;
        public PrijavaController(StudentskiContext ctx) => _ctx = ctx;

        // POST /api/Prijava
        [HttpPost]
        public async Task<ActionResult<PrijavaDto>> PrijaviIspit([FromBody] PrijavaCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var student = await _ctx.Students.FindAsync(dto.IdStudenta);
            if (student is null)
                return NotFound(new { poruka = $"Student ID={dto.IdStudenta} nije pronađen." });

            var predmet = await _ctx.Predmets.FindAsync(dto.IdPredmeta);
            if (predmet is null)
                return NotFound(new { poruka = $"Predmet ID={dto.IdPredmeta} nije pronađen." });

            var rok = await _ctx.IspitniRoks.FindAsync(dto.IdRoka);
            if (rok is null)
                return NotFound(new { poruka = $"Rok ID={dto.IdRoka} nije pronađen." });

            bool imaUListi = await _ctx.StudentPredmets.AnyAsync(sp =>
                sp.IdStudenta == dto.IdStudenta &&
                sp.IdPredmeta == dto.IdPredmeta);
            if (!imaUListi)
                return BadRequest(new { poruka = $"Predmet \"{predmet.Naziv}\" nije u izbornoj listi." });

            bool jePolozio = await (
                from z in _ctx.Zapisniks
                where z.IdStudenta == dto.IdStudenta && z.Ocena >= 6
                join i in _ctx.Ispits on z.IdIspita equals i.IdIspita
                where (int)i.IdPredmeta == dto.IdPredmeta
                select z
            ).AnyAsync();
            if (jePolozio)
                return Conflict(new { poruka = $"Predmet \"{predmet.Naziv}\" je već položen." });

            bool vecPrijavljen = await _ctx.PrijavaIspita.AnyAsync(p =>
                p.IdStudenta == dto.IdStudenta &&
                p.IdPredmeta == dto.IdPredmeta &&
                p.IdRoka     == dto.IdRoka);
            if (vecPrijavljen)
                return Conflict(new { poruka = "Student je već prijavljen za ovaj predmet u ovom roku." });

            var prijava = new PrijavaIspitum
            {
                IdStudenta   = dto.IdStudenta,
                IdPredmeta   = dto.IdPredmeta,
                IdRoka       = dto.IdRoka,
                DatumPrijave = DateTime.Now
            };
            _ctx.PrijavaIspita.Add(prijava);
            await _ctx.SaveChangesAsync();

            return Ok(new PrijavaDto
            {
                Id            = prijava.Id,
                IdStudenta    = student.IdStudenta,
                ImeStudenta   = $"{student.Ime} {student.Prezime}",
                BrojIndeksa   = student.Smer + "-" + student.Broj + "/" + student.GodinaUpisa,
                IdPredmeta    = predmet.IdPredmeta,
                NazivPredmeta = predmet.Naziv,
                IdRoka        = rok.IdRoka,
                NazivRoka     = rok.Naziv,
                SkolskaGod    = rok.SkolskaGod,
                DatumPrijave  = prijava.DatumPrijave
            });
        }

        // GET /api/Prijava/ispit/{idPredmeta}/{idRoka}
        [HttpGet("ispit/{idPredmeta:int}/{idRoka:int}")]
        public async Task<ActionResult<IEnumerable<PrijavaDto>>> GetPrijaveZaIspit(
            int idPredmeta, int idRoka)
        {
            var prijave = await (
                from p  in _ctx.PrijavaIspita
                where p.IdPredmeta == idPredmeta && p.IdRoka == idRoka
                join s  in _ctx.Students    on p.IdStudenta equals s.IdStudenta
                join pr in _ctx.Predmets    on p.IdPredmeta equals pr.IdPredmeta
                join r  in _ctx.IspitniRoks on p.IdRoka     equals r.IdRoka
                orderby s.Prezime, s.Ime
                select new PrijavaDto
                {
                    Id            = p.Id,
                    IdStudenta    = p.IdStudenta,
                    ImeStudenta   = s.Ime + " " + s.Prezime,
                    BrojIndeksa   = s.Smer + "-" + s.Broj + "/" + s.GodinaUpisa,
                    IdPredmeta    = p.IdPredmeta,
                    NazivPredmeta = pr.Naziv,
                    IdRoka        = p.IdRoka,
                    NazivRoka     = r.Naziv,
                    SkolskaGod    = r.SkolskaGod,
                    DatumPrijave  = p.DatumPrijave
                }
            ).ToListAsync();
            return Ok(prijave);
        }

        // GET /api/Prijava/student/{idStudenta}
        [HttpGet("student/{idStudenta:int}")]
        public async Task<ActionResult<IEnumerable<PrijavaDto>>> GetPrijaveStudenta(int idStudenta)
        {
            bool postoji = await _ctx.Students.AnyAsync(s => s.IdStudenta == idStudenta);
            if (!postoji)
                return NotFound(new { poruka = $"Student ID={idStudenta} nije pronađen." });

            var prijave = await (
                from p  in _ctx.PrijavaIspita
                where p.IdStudenta == idStudenta
                join s  in _ctx.Students    on p.IdStudenta equals s.IdStudenta
                join pr in _ctx.Predmets    on p.IdPredmeta equals pr.IdPredmeta
                join r  in _ctx.IspitniRoks on p.IdRoka     equals r.IdRoka
                orderby p.DatumPrijave descending
                select new PrijavaDto
                {
                    Id            = p.Id,
                    IdStudenta    = p.IdStudenta,
                    ImeStudenta   = s.Ime + " " + s.Prezime,
                    BrojIndeksa   = s.Smer + "-" + s.Broj + "/" + s.GodinaUpisa,
                    IdPredmeta    = p.IdPredmeta,
                    NazivPredmeta = pr.Naziv,
                    IdRoka        = p.IdRoka,
                    NazivRoka     = r.Naziv,
                    SkolskaGod    = r.SkolskaGod,
                    DatumPrijave  = p.DatumPrijave
                }
            ).ToListAsync();
            return Ok(prijave);
        }

        // DELETE /api/Prijava/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> OdjaviIspit(int id)
        {
            var prijava = await _ctx.PrijavaIspita.FindAsync(id);
            if (prijava is null)
                return NotFound(new { poruka = $"Prijava ID={id} nije pronađena." });
            _ctx.PrijavaIspita.Remove(prijava);
            await _ctx.SaveChangesAsync();
            return NoContent();
        }
    }
}
