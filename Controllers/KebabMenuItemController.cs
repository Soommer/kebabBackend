using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;
using kebabBackend.Repositories.MenuItem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace kebabBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuItemController : ControllerBase
    {
        private readonly IMenuItemRepository _repo;
        private readonly ILogger<MenuItemController> _logger;
        private readonly IMemoryCache _cache;

        public MenuItemController(IMenuItemRepository repo, ILogger<MenuItemController> logger, IMemoryCache cache)
        {
            _cache = cache;
            _repo = repo;
            _logger = logger;
        }
        const string cacheKey = "souce_types";
        [HttpPost]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> AddMenuItem([FromForm] MenuItemCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var menuItem = new menuItem
                {
                    Name = request.Name,
                    Description = request.Description,
                    BasePrice = request.BasePrice,
                    CategoryId = request.CategoryId 
                };

                var result = await _repo.AddMenuItemAsync(menuItem, request.Image);
                _cache.Remove(cacheKey);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Walidacja obrazu nie powiodła się.");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd serwera podczas dodawania menu itemu.");
                return StatusCode(500, new { error = "Wewnętrzny błąd serwera." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId)
        {
            try
            {
                if(_cache.TryGetValue(cacheKey, out List<MenuItemReturn> cachedList))
                {
                    _logger.LogInformation("Zwracam menu z cache");
                    return Ok(cachedList);
                }
                var list = await _repo.GetAllMenuItemsAsync(categoryId);
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cacheKey, list, cacheOptions);
                _logger.LogInformation("Zwracam menu z bazy i dodaję je do cache.");
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania listy menu.");
                return StatusCode(500, new { error = "Wewnętrzny błąd serwera." });
            }
        }



        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _repo.DeleteMenuItemAsync(id);
                if (!success)
                {
                    _logger.LogWarning("Próba usunięcia nieistniejącego menuItem o id {MenuItemId}", id);
                    return NotFound(new { message = "Menu item nie istnieje." });
                }
                _cache.Remove(cacheKey);

                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania menuItem o id {MenuItemId}", id);
                return StatusCode(500, new { error = "Wewnętrzny błąd serwera podczas usuwania." });
            }
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Update(Guid id, [FromForm] MenuItemUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var success = await _repo.UpdateMenuItemAsync(id, request);
                if (!success)
                {
                    _logger.LogWarning("Próba aktualizacji nieistniejącego menuItem o id {Id}", id);
                    return NotFound(new { message = "Nie znaleziono elementu menu." });
                }
                _cache.Remove(cacheKey);

                return Ok(success);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Błąd walidacji obrazu podczas aktualizacji.");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji menuItem o id {Id}", id);
                return StatusCode(500, new { error = "Wewnętrzny błąd serwera." });
            }
        }

        [HttpPut("NoPicture/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateNoPicture(Guid id, [FromForm] MenuItemUpdateNoPicture request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var success = await _repo.UpdateAsyncNoPicture(id, request);
                if (!success)
                {
                    _logger.LogWarning("Próba aktualizacji nieistniejącego menuItem o id {Id}", id);
                    return NotFound(new { message = "Nie znaleziono elementu menu." });
                }
                _cache.Remove(cacheKey);

                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Błąd podczas aktualizacji menuItem o id {Id}", id);
                return StatusCode(500, new { error = "Wewnętrzny błąd serwera" });
            }
        }
    }
}
