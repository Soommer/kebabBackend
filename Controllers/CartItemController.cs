using kebabBackend.Models.DTO;
using kebabBackend.Repositories.CartItems;
using Microsoft.AspNetCore.Mvc;

namespace kebabBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartItemController : ControllerBase
    {
        private readonly ICartItemRepository _repo;
        private readonly ILogger<CartItemController> _logger;

        public CartItemController(ICartItemRepository repo, ILogger<CartItemController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CartItemCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var cartItem = await _repo.CreateAsync(request);
                var response = await _repo.GetResponseAsync(cartItem.Id);
                return CreatedAtAction(nameof(GetById), new { id = cartItem.Id }, response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Nieprawidłowe dane wejściowe");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas tworzenia CartItem");
                return StatusCode(500, new { error = "Wewnętrzny błąd serwera." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _repo.GetAllResponsesAsync();
            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _repo.GetResponseAsync(id);
            if (item == null)
                return NotFound(new { message = "Nie znaleziono CartItem." });

            return Ok(item);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _repo.DeleteAsync(id);
                if (!success)
                    return NotFound(new { message = "Nie znaleziono CartItem do usunięcia." });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania CartItem");
                return StatusCode(500, new { error = "Wewnętrzny błąd serwera." });
            }
        }
    }
}
