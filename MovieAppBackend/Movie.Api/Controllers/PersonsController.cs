using Microsoft.AspNetCore.Mvc;
using Movie.Domain.DTOs;
using Movie.Infrastructure.Repositories;

namespace Movie.Api.Controllers
{
    [ApiController]
    [Route("api/persons")]
    public class PersonsController : ControllerBase
    {
        private readonly IPersonRepository _repo;
        public PersonsController(IPersonRepository repo) => _repo = repo;

        // GET /api/persons?page=1&pageSize=20&q=tom
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? q = null, CancellationToken ct = default)
        {
            var (items, total) = await _repo.SearchAsync(q, page, pageSize, ct);

            var totalPages = (int)Math.Ceiling((double)total / Math.Clamp(pageSize, 1, 100));

            var links = new
            {
                self = Url.Action(null, new { page, pageSize, q }),
                next = page < totalPages ? Url.Action(null, new { page = page + 1, pageSize, q }) : null,
                prev = page > 1 ? Url.Action(null, new { page = page - 1, pageSize, q }) : null
            };

            var data = items.Select(p => new PersonListItemDto
            {
                Nconst = p.Nconst,
                PrimaryName = p.PrimaryName,
                BirthYear = p.BirthYear,
            DeathYear = p.DeathYear,
             PrimaryProfession = p.PrimaryProfession,


                Self = $"/api/persons/{p.Nconst}"
            });

            return Ok(new { page, pageSize, total, totalPages, links, data });
        }

        // GET /api/persons/nm1234567
        [HttpGet("{nconst}")]
        public async Task<IActionResult> GetById(string nconst, CancellationToken ct = default)
        {
            var p = await _repo.GetByIdAsync(nconst, ct);
            if (p is null) return NotFound();

            var dto = new PersonDetailDto
            {
                Nconst = p.Nconst,
                PrimaryName = p.PrimaryName,
                BirthYear = p.BirthYear,
                DeathYear = p.DeathYear,
                Self = $"/api/persons/{p.Nconst}"
            };

            return Ok(dto);
        }
    }
}
