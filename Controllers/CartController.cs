using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;
using kebabBackend.Repositories.Cart;
using kebabBackend.Repositories.CartItems;
using Microsoft.AspNetCore.Mvc;

namespace kebabBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _repo;
        private readonly ICartItemRepository _itemRepository;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartRepository repo, ILogger<CartController> logger, ICartItemRepository cartItemRepository)
        {
            _repo = repo;
            _itemRepository = cartItemRepository;
            _logger = logger;
        }


        [HttpPut]
        public async Task<IActionResult> MarkAsComplete(Guid id)
        {
            try
            {
                var success = await _repo.ComleteCardAsync(id);
                if (!success)
                {
                    _logger.LogWarning("Nie znaleziono koszyka.");
                    return NotFound(new { message = "Nie znaleziono koszyka." });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd przy koszyku {ex.Message}");
                return BadRequest(ex);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CartCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _repo.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Błąd walidacji Cart");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd serwera przy tworzeniu Cart");
                return StatusCode(500, new { error = "Wewnętrzny błąd serwera." });
            }
        }

        [HttpGet("{id}/count")]
        public async Task<IActionResult> GetCartItemCount(Guid id)
        {
            try
            {
                var count = await _repo.GetCartItemCountAsync(id);
                return Ok(new { Count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd przy liczeniu");
                return StatusCode(400, new { Error = ex.Message });
            }
        }


        [HttpPost]
        [Route("addToCart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCardRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var cartItemRequest = new CartItemCreateRequest
                {
                    Size = request.Size,
                    MenuItemId = request.MenuItemId,
                    SouceId = request.SouceId,
                    MeatTypeId = request.MeatTypeId,
                    ExtraIngredientIds = request.ExtraIngredientIds
                };

                var createdCartItem = await _itemRepository.CreateAsync(cartItemRequest);

                if (request.CartId.HasValue)
                {
                    var updatedCart = await _repo.AddExistingItemAsync(request.CartId.Value, createdCartItem.Id);
                    return Ok(new { cartId = updatedCart.Id });
                }
                else
                {
                    var newCartRequest = new CartCreateRequest
                    {
                        CartItemIds = new List<Guid> { createdCartItem.Id }
                    };

                    var newCart = await _repo.CreateAsync(newCartRequest);
                    return Ok(new { cartId = newCart.Id });
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Błąd walidacji danych wejściowych.");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd serwera przy tworzeniu koszyka.");
                return StatusCode(500, new { error = "Wewnętrzny błąd serwera." });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var carts = await _repo.GetAllAsync();
            return Ok(carts);
        }

        [HttpGet]
        [Route("ReturnUnfinished")]
        public async Task<IActionResult> GetUnfinished()
        {
            try
            {
                var carts = await _repo.GetAllNonCompleteAsync();
                return Ok(carts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas get koszyków");
                return StatusCode(500, new { error = "Wewnętrzny błąd serwera." });
            }

        }


        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var cart = await _repo.GetByIdAsync(id);
            if (cart == null) 
            {
                _logger.LogError("Nie znaleziono koszyka. ");
                return NotFound(new { message = "Nie znaleziono koszyka." });
            }
            return Ok(cart);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _repo.DeleteAsync(id);
                if (!success)
                {
                    _logger.LogError($"Could not delete {id}");
                    return NotFound(new { message = "Nie znaleziono koszyka do usunięcia." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania koszyka");
                return StatusCode(500, new { error = "Wewnętrzny błąd serwera." });
            }
        }
    }

}
