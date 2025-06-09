using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;
using kebabBackend.Repositories.Ingridients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace kebabBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExtraIngredientController : ControllerBase
    {
        private readonly IExtraIngredientRepository _repo;
        private readonly ILogger<ExtraIngredientController> _logger;
        private readonly IMemoryCache _cache;

        public ExtraIngredientController(IExtraIngredientRepository repo, ILogger<ExtraIngredientController> logger, IMemoryCache cache)
        {
            _cache = cache;
            _repo = repo;
            _logger = logger;
        }
        const string cacheKey = "extra_types";
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                if (_cache.TryGetValue(cacheKey, out List<extraIgredients> cachedList))
                {
                    _logger.LogInformation("Zwraca extra z cache");
                    return Ok(cachedList);
                }

                var list = await _repo.GetAllAsync();
                
                var cacheOption = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cacheKey, list, cacheOption);
                _logger.LogInformation("Zwraca exra z bazy i dodaję do cache.");

                return Ok(list);
            }
            catch (Exception ex) 
            {
                _logger.LogError($"Błąd przy Extra {ex.Message}");
                return BadRequest(ex);
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
                    _logger.LogWarning("Nie znaleziono dodatku.");
                    return NotFound(new { message = "Nie znaleziono dodatku." });
                }
                return Ok(item);
            }
            catch(Exception ex)  
            {
                _logger.LogError($"Błąd przy Extra {ex.Message}");
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] ExtraIngredientCreateRequest request)
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
                _logger.LogError($"Błąd przy Extra {ex.Message}");
                return BadRequest(ex);
            }

        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Update(Guid id, [FromBody] ExtraIngredientUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var success = await _repo.UpdateAsync(id, request);
                if (!success)
                {
                    _logger.LogWarning("Nie znaleziono dodatku do aktualizacji.");
                    return NotFound(new { message = "Nie znaleziono dodatku do aktualizacji." });
                }
                _cache.Remove(cacheKey);

                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd przy Extra {ex.Message}");
                return BadRequest(ex);
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _repo.DeleteAsync(id);
                if (!success)
                    return NotFound(new { message = "Nie znaleziono dodatku do usunięcia." });
                _cache.Remove(cacheKey);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania dodatku.");
                return StatusCode(500, new { error = "Wewnętrzny błąd serwera." });
            }
        }
    }

}
