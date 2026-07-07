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
    public class StudentPredmetController : ControllerBase
    {
        private readonly StudentskiContext _ctx;
        public StudentPredmetController(StudentskiContext ctx) => _ctx = ctx;

        // GET /api/StudentPredmet/{idStudenta}
        [HttpGet("{idStudenta:int}")]
        public async Task<ActionResult<IEnumerable<StudentPredmetDto>>> GetIzbornaLista(int idStudenta)
        {
            bool postoji = await _ctx.Students.AnyAsync(s => s.IdStudenta == idStudenta);
            if (!postoji)
                return NotFound(new { poruka = $"Student ID={idStudenta} nije pronađen." });

            var zapisnici    = await _ctx.Zapisniks.Where(z => z.IdStudenta == idStudenta && z.Ocena >= 6).ToListAsync();
            var ispiti       = await _ctx.Ispits.ToListAsync();
            var predmeti     = await _ctx.Predmets.ToListAsync();
            var studentPred  = await _ctx.StudentPredmets.Where(sp => sp.IdStudenta == idStudenta).ToListAsync();

            var polozeniIds = (
                from z in zapisnici
                join i in ispiti on z.IdIspita equals i.IdIspita
                select (int)i.IdPredmeta
            ).Distinct().ToList();

            var lista = (
                from sp in studentPred
                join p in predmeti on sp.IdPredmeta equals p.IdPredmeta
                orderby p.Naziv
                select new StudentPredmetDto
                {
                    IdStudenta    = sp.IdStudenta,
                    IdPredmeta    = sp.IdPredmeta,
                    SkolskaGodina = sp.SkolskaGodina,
                    NazivPredmeta = p.Naziv,
                    Espb          = p.Espb,
                    JePolozio     = polozeniIds.Contains(sp.IdPredmeta)
                }
            ).ToList();

            return Ok(lista);
        }

        // GET /api/StudentPredmet/{idStudenta}/za-polaganje
        [HttpGet("{idStudenta:int}/za-polaganje")]
        public async Task<ActionResult<IEnumerable<StudentPredmetDto>>> GetZaPolaganje(int idStudenta)
        {
            bool postoji = await _ctx.Students.AnyAsync(s => s.IdStudenta == idStudenta);
            if (!postoji)
                return NotFound(new { poruka = $"Student ID={idStudenta} nije pronađen." });

            var zapisnici   = await _ctx.Zapisniks.Where(z => z.IdStudenta == idStudenta && z.Ocena >= 6).ToListAsync();
            var ispiti      = await _ctx.Ispits.ToListAsync();
            var predmeti    = await _ctx.Predmets.ToListAsync();
            var studentPred = await _ctx.StudentPredmets.Where(sp => sp.IdStudenta == idStudenta).ToListAsync();

            var polozeniIds = (
                from z in zapisnici
                join i in ispiti on z.IdIspita equals i.IdIspita
                select (int)i.IdPredmeta
            ).Distinct().ToList();

            var zaPolaganje = (
                from sp in studentPred
                where !polozeniIds.Contains(sp.IdPredmeta)
                join p in predmeti on sp.IdPredmeta equals p.IdPredmeta
                orderby p.Naziv
                select new StudentPredmetDto
                {
                    IdStudenta    = sp.IdStudenta,
                    IdPredmeta    = sp.IdPredmeta,
                    SkolskaGodina = sp.SkolskaGodina,
                    NazivPredmeta = p.Naziv,
                    Espb          = p.Espb,
                    JePolozio     = false
                }
            ).ToList();

            return Ok(zaPolaganje);
        }

        // POST /api/StudentPredmet
        [HttpPost]
        public async Task<ActionResult<StudentPredmetDto>> DodajPredmet([FromBody] DodajPredmetDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            bool studentPostoji = await _ctx.Students.AnyAsync(s => s.IdStudenta == dto.IdStudenta);
            if (!studentPostoji)
                return NotFound(new { poruka = $"Student ID={dto.IdStudenta} nije pronađen." });

            var predmet = await _ctx.Predmets.FindAsync(dto.IdPredmeta);
            if (predmet is null)
                return NotFound(new { poruka = $"Predmet ID={dto.IdPredmeta} nije pronađen." });

            bool vecOdabran = await _ctx.StudentPredmets.AnyAsync(sp =>
                sp.IdStudenta    == dto.IdStudenta &&
                sp.IdPredmeta    == dto.IdPredmeta &&
                sp.SkolskaGodina == dto.SkolskaGodina);
            if (vecOdabran)
                return Conflict(new { poruka = "Predmet već postoji u izbornoj listi za tu školsku godinu." });

            var noviZapis = new StudentPredmet
            {
                IdStudenta    = dto.IdStudenta,
                IdPredmeta    = dto.IdPredmeta,
                SkolskaGodina = dto.SkolskaGodina
            };
            _ctx.StudentPredmets.Add(noviZapis);
            await _ctx.SaveChangesAsync();

            return Ok(new StudentPredmetDto
            {
                IdStudenta    = noviZapis.IdStudenta,
                IdPredmeta    = noviZapis.IdPredmeta,
                SkolskaGodina = noviZapis.SkolskaGodina,
                NazivPredmeta = predmet.Naziv,
                Espb          = predmet.Espb,
                JePolozio     = false
            });
        }

        // DELETE /api/StudentPredmet/{idStudenta}/{idPredmeta}/{skolskaGodina}
        [HttpDelete("{idStudenta:int}/{idPredmeta:int}/{skolskaGodina}")]
        public async Task<IActionResult> ObrisiPredmet(int idStudenta, int idPredmeta, string skolskaGodina)
        {
            var zapis = await _ctx.StudentPredmets.FirstOrDefaultAsync(sp =>
                sp.IdStudenta    == idStudenta &&
                sp.IdPredmeta    == idPredmeta &&
                sp.SkolskaGodina == skolskaGodina);

            if (zapis is null)
                return NotFound(new { poruka = "Zapis nije pronađen." });

            // Provjera: položen? (u memoriji)
            var zapisnici = await _ctx.Zapisniks
                .Where(z => z.IdStudenta == idStudenta && z.Ocena >= 6)
                .ToListAsync();
            var ispiti = await _ctx.Ispits.ToListAsync();

            bool jePolozio = (
                from z in zapisnici
                join i in ispiti on z.IdIspita equals i.IdIspita
                where (int)i.IdPredmeta == idPredmeta
                select z
            ).Any();

            if (jePolozio)
                return Conflict(new { poruka = "Nije moguće ukloniti — student je već položio ispit." });

            _ctx.StudentPredmets.Remove(zapis);
            await _ctx.SaveChangesAsync();
            return NoContent();
        }
    }
}
