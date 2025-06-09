using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;
using kebabBackend.Repositories.meatType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;


namespace kebabBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeatTypeController : ControllerBase
    {
        private readonly IMeatTypeRepository _repo;
        private readonly ILogger<MeatTypeController> _logger;
        private readonly IMemoryCache _cache;


        public MeatTypeController(IMeatTypeRepository repo, ILogger<MeatTypeController> logger, IMemoryCache cache)
        {
            _cache = cache;
            _repo = repo;
            _logger = logger;
        }
        const string cacheKey = "meat_types";

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                if(_cache.TryGetValue(cacheKey, out List<MeatType> cachedList))
                {
                    _logger.LogInformation("Zwracam mięsa z cache");
                    return Ok(cachedList);
                }

                var list = await _repo.GetAllAsync();
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cacheKey, list , cacheOptions);
                _logger.LogInformation("Zwracam mięsa i dodaje je do cache");
                return Ok(list);
            }
            catch (Exception ex) 
            {
                _logger.LogError($"Błąd przy MIęsach {ex.Message}");
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
                    _logger.LogWarning("Nie znaleziono rodzaju mięsa.");
                    return NotFound(new { message = "Nie znaleziono rodzaju mięsa." });
                }
                return Ok(item);
            }
            catch (Exception ex) 
            {
                _logger.LogError($"Błąd przy MIęsach {ex.Message}");
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] MeatTypeCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _repo.AddAsync(request);
                _cache.Remove(cacheKey);

                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex) 
            {
                _logger.LogError($"Błąd przy MIęsach {ex.Message}");
                return BadRequest(ex);
            }
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Update(Guid id, [FromBody] MeatTypeUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var success = await _repo.UpdateAsync(id, request);
                if (!success)
                {
                    _logger.LogWarning("Nie znaleziono rodzaju mięsa do aktualizacji.");
                    return NotFound(new { message = "Nie znaleziono rodzaju mięsa do aktualizacji." });
                }
                _cache.Remove(cacheKey);

                return Ok();
            } 
            catch (Exception ex)
            {
                _logger.LogError($"Błąd przy MIęsach {ex.Message}");
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
                    return NotFound(new { message = "Nie znaleziono rodzaju mięsa do usunięcia." });
                _cache.Remove(cacheKey);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania rodzaju mięsa.");
                return StatusCode(500, new { error = "Wewnętrzny błąd serwera." });
            }
        }
    }

}
