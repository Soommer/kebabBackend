using kebabBackend.Models.DTO;
using kebabBackend.Repositories.menuItemCat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace kebabBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuItemCategoryController : ControllerBase
    {
        private readonly IMenuItemCategoryRepository _repo;
        private readonly ILogger<MenuItemCategoryController> _logger;

        public MenuItemCategoryController(IMenuItemCategoryRepository repo, ILogger<MenuItemCategoryController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _repo.GetAllAsync();
            return Ok(categories);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null)
                return NotFound(new { message = "Nie znaleziono kategorii." });

            return Ok(category);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Create([FromBody] MenuItemCategoryCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _repo.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Update(Guid id, [FromBody] MenuItemCategoryCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _repo.UpdateAsync(id, request);
            if (!updated)
                return NotFound(new { message = "Nie znaleziono kategorii do aktualizacji." });

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _repo.DeleteAsync(id);
                if (!success)
                    return NotFound(new { message = "Nie znaleziono kategorii do usunięcia." });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania kategorii.");
                return StatusCode(500, new { error = "Wewnętrzny błąd serwera." });
            }
        }
    }

}
