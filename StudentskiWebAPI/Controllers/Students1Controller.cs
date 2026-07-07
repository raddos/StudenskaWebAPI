//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using StudentskiWebAPI.Data;
//using StudentskiWebAPI.DTOs;

//namespace StudentskiWebAPI.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Produces("application/json")]
//    public class StudentsController : ControllerBase
//    {
//        private readonly StudentskiContext _ctx;
//        public StudentsController(StudentskiContext ctx) => _ctx = ctx;

//        // GET /api/Students
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<StudentListDto>>> GetStudents()
//        {
//            var studenti = await _ctx.Students
//                .OrderBy(s => s.Prezime).ThenBy(s => s.Ime)
//                .Select(s => new StudentListDto
//                {
//                    IdStudenta = s.IdStudenta,
//                    Ime = s.Ime,
//                    Prezime = s.Prezime,
//                    // Broj je short, GodinaUpisa je string
//                    BrojIndeksa = $"{s.Smer}-{s.Broj}/{s.GodinaUpisa}"
//                })
//                .ToListAsync();
//            return Ok(studenti);
//        }

//        // GET /api/Students/{id}
//        [HttpGet("{id:int}")]
//        public async Task<ActionResult<StudentListDto>> GetStudent(int id)
//        {
//            var s = await _ctx.Students.FindAsync(id);
//            if (s is null)
//                return NotFound(new { poruka = $"Student ID={id} nije pronađen." });
//            return Ok(new StudentListDto
//            {
//                IdStudenta = s.IdStudenta,
//                Ime = s.Ime,
//                Prezime = s.Prezime,
//                BrojIndeksa = $"{s.Smer}-{s.Broj}/{s.GodinaUpisa}"
//            });
//        }

//        // GET /api/Students/pretraga?q=...&smer=...&godinaUpisa=...
//        [HttpGet("pretraga")]
//        public async Task<ActionResult<IEnumerable<StudentListDto>>> Pretraga(
//            [FromQuery] string? q = null,
//            [FromQuery] string? smer = null,
//            [FromQuery] string? godinaUpisa = null,  // string jer je CHAR(4)
//            [FromQuery] short? broj = null)
//        {
//            var upit = _ctx.Students.AsQueryable();

//            if (!string.IsNullOrWhiteSpace(q))
//            {
//                var pojam = q.ToLower().Trim();
//                upit = upit.Where(s =>
//                    s.Ime.ToLower().Contains(pojam) ||
//                    s.Prezime.ToLower().Contains(pojam) ||
//                    s.Smer.ToLower().Contains(pojam) ||
//                    s.GodinaUpisa.Contains(pojam));
//            }
//            if (!string.IsNullOrWhiteSpace(smer))
//                upit = upit.Where(s => s.Smer.ToLower() == smer.ToLower().Trim());
//            if (!string.IsNullOrWhiteSpace(godinaUpisa))
//                upit = upit.Where(s => s.GodinaUpisa == godinaUpisa.Trim());
//            if (broj.HasValue)
//                upit = upit.Where(s => s.Broj == broj.Value);

//            var rezultat = await upit
//                .OrderBy(s => s.Prezime).ThenBy(s => s.Ime)
//                .Select(s => new StudentListDto
//                {
//                    IdStudenta = s.IdStudenta,
//                    Ime = s.Ime,
//                    Prezime = s.Prezime,
//                    BrojIndeksa = $"{s.Smer}-{s.Broj}/{s.GodinaUpisa}"
//                })
//                .ToListAsync();
//            return Ok(rezultat);
//        }

//        // PUT /api/Students/{id}/licni-podaci
//        [HttpPut("{id:int}/licni-podaci")]
//        public async Task<ActionResult<StudentListDto>> IzmeniLicnePodatke(
//            int id, [FromBody] StudentUpdateDto dto)
//        {
//            if (!ModelState.IsValid) return BadRequest(ModelState);

//            var student = await _ctx.Students.FindAsync(id);
//            if (student is null)
//                return NotFound(new { poruka = $"Student ID={id} nije pronađen." });

//            student.Ime = dto.Ime.Trim();
//            student.Prezime = dto.Prezime.Trim();
//            await _ctx.SaveChangesAsync();

//            return Ok(new StudentListDto
//            {
//                IdStudenta = student.IdStudenta,
//                Ime = student.Ime,
//                Prezime = student.Prezime,
//                BrojIndeksa = $"{student.Smer}-{student.Broj}/{student.GodinaUpisa}"
//            });
//        }

//        // GET /api/Students/{id}/polozeni-predmeti
//        [HttpGet("{id:int}/polozeni-predmeti")]
//        public async Task<ActionResult<StudentProsekDto>> GetPolozeniPredmeti(int id)
//        {
//            var student = await _ctx.Students.FindAsync(id);
//            if (student is null)
//                return NotFound(new { poruka = $"Student ID={id} nije pronađen." });

//            // Ocena je float u bazi — položen ako >= 6
//            // Zapisnik nema navigation properties → eksplicitni join
//            var polozeni = await (
//                from z in _ctx.Zapisniks
//                where z.IdStudenta == id && z.Ocena >= 6
//                join i in _ctx.Ispits on z.IdIspita equals i.IdIspita
//                join p in _ctx.Predmets on i.IdPredmeta equals p.IdPredmeta
//                join r in _ctx.IspitniRoks on i.IdRoka equals r.IdRoka
//                select new PolozeniPredmetDto
//                {
//                    NazivPredmeta = p.Naziv,
//                    Espb = p.Espb,
//                    Ocena = z.Ocena,
//                    Bodovi = z.Bodovi,
//                    NazivRoka = r.Naziv,
//                    SkolskaGod = r.SkolskaGod,
//                    Datum = i.Datum.ToString()
//                }
//            ).ToListAsync();

//            double prosek = polozeni.Count > 0
//                ? polozeni.Average(x => x.Ocena)
//                : 0.0;

//            return Ok(new StudentProsekDto
//            {
//                IdStudenta = student.IdStudenta,
//                Ime = student.Ime,
//                Prezime = student.Prezime,
//                BrojIndeksa = $"{student.Smer}-{student.Broj}/{student.GodinaUpisa}",
//                ProsekOcena = Math.Round(prosek, 2),
//                BrojPolozenih = polozeni.Count,
//                Predmeti = polozeni
//            });
//        }
//    }
//}
