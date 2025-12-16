using Microsoft.AspNetCore.Mvc;
using Movie.Domain.DTOs;
using Movie.Infrastructure.Repositories;

namespace Movie.Api.Controllers
{
    [ApiController]
    [Route("api/users/{userId:int}/ratings")]
    public class UserRatingsController : ControllerBase
    {
        private readonly IUserActivityRepository _repo;

        public UserRatingsController(IUserActivityRepository repo)
        {
            _repo = repo;
        }

        // TITLE RATINGS 

        // POST /api/users/id/ratings/titles
        [HttpPost("titles")]
        public async Task<IActionResult> RateTitle(
            int userId,
            [FromBody] RateTitleRequestDto dto,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Tconst))
                return BadRequest("tconst is required");

            if (dto.Rating < 1 || dto.Rating > 10)
                return BadRequest("rating must be between 1 and 10");

            var rating = await _repo.AddOrUpdateTitleRatingAsync(
                userId,
                dto.Tconst,
                dto.Rating,
                ct);

            var result = new TitleRatingDto
            {
                Tconst = rating.Tconst,
                Rating = rating.Rating,
                RatedAt = rating.RatedAt
            };

            return Ok(result);
        }

        // GET /api/users/user id /ratings/titles
        [HttpGet("titles")]
        public async Task<IActionResult> GetTitleRatings(
            int userId,
            CancellationToken ct = default)
        {
            var items = await _repo.GetTitleRatingsAsync(userId, ct);

            var result = items.Select(x => new TitleRatingDto
            {
                Tconst = x.Tconst,
                Rating = x.Rating,
                RatedAt = x.RatedAt
            });

            return Ok(result);
        }

        // PERSON RATINGS

        // POST /api/users/user id /ratings/persons
        [HttpPost("persons")]
        public async Task<IActionResult> RatePerson(
            int userId,
            [FromBody] RatePersonRequestDto dto,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Nconst))
                return BadRequest("nconst is required");

            if (dto.Rating < 1 || dto.Rating > 10)
                return BadRequest("rating must be between 1 and 10");

            var rating = await _repo.AddOrUpdatePersonRatingAsync(
                userId,
                dto.Nconst,
                dto.Rating,
                ct);

            var result = new PersonRatingDto
            {
                Nconst = rating.Nconst,
                Rating = rating.Rating,
                RatedAt = rating.RatedAt
            };

            return Ok(result);
        }

        // GET /api/users/user id /ratings/persons
        [HttpGet("persons")]
        public async Task<IActionResult> GetPersonRatings(
            int userId,
            CancellationToken ct = default)
        {
            var items = await _repo.GetPersonRatingsAsync(userId, ct);

            var result = items.Select(x => new PersonRatingDto
            {
                Nconst = x.Nconst,
                Rating = x.Rating,
                RatedAt = x.RatedAt
            });

            return Ok(result);
        }
    }
}
