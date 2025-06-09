using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;
using kebabBackend.Repositories.Souces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace kebabBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SouceController : ControllerBase
    {
        private readonly ISouceRepository _repo;
        private readonly ILogger<SouceController> _logger;
        private readonly IMemoryCache _cache;

        public SouceController(ISouceRepository repo, ILogger<SouceController> logger, IMemoryCache cache)
        {
            _cache = cache;
            _repo = repo;
            _logger = logger;
        }
        const string cacheKey = "souce_types";

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // Próbujemy pobrać z cache
                if (_cache.TryGetValue(cacheKey, out List<Souce> cachedList))
                {
                    _logger.LogInformation("Zwraca sosy z cache.");
                    return Ok(cachedList);
                }

                // Jeśli nie ma w cache – pobierz z repozytorium
                var list = await _repo.GetAllAsync();

                // Ustaw cache na 10 minut (lub jak chcesz)
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cacheKey, list, cacheOptions);

                _logger.LogInformation("Zwraca sosy z bazy i dodaję do cache.");
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd przy pobieraniu sosów: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var item = await _repo.GetByIdAsync(id);
                if (item == null)
                {
                    _logger.LogWarning("Brak sosu");
                    return NotFound(new { message = "Nie znaleziono sosu." });
                }
                return Ok(item);
            }
            catch(Exception ex)  
            {
                _logger.LogError($"{ex.Message}");
                return BadRequest(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SouceCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _repo.AddAsync(request);
                _cache.Remove(cacheKey);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch(Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                return BadRequest(ex);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SouceUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var success = await _repo.UpdateAsync(id, request);
                if (!success)
                {
                    _logger.LogWarning("Nie znaleziono sosu do aktualizacji.");
                    return NotFound(new { message = "Nie znaleziono sosu do aktualizacji." });
                }
                _cache.Remove(cacheKey);
                return Ok(success);
            }
            catch (Exception ex) 
            {
                _logger.LogError($"{ex.Message}");
                return BadRequest(ex);
            }

        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _repo.DeleteAsync(id);
                if (!success)
                    return NotFound(new { message = "Nie znaleziono sosu do usunięcia." });

                _cache.Remove(cacheKey);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania sosu.");
                return StatusCode(500, new { error = "Wewnętrzny błąd serwera." });
            }
        }
    }

}
