using kebabBackend.Data;
using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace kebabBackend.Repositories.Cart
{
    public class CartRepository : ICartRepository
    {
        private readonly KebabDbContext _context;

        public CartRepository(KebabDbContext context)
        {
            _context = context;
        }

        public async Task<CartResponse> CreateAsync(CartCreateRequest request)
        {


            var cartItems = await _context.cartItems
                .Where(ci => request.CartItemIds.Contains(ci.Id) && ci.CartId == null)
                .Include(ci => ci.MenuItem) 
                .ToListAsync();

            if (cartItems.Any(ci => ci.MenuItem == null))
                throw new InvalidOperationException("Jeden z CartItemów nie ma przypisanego MenuItem.");

            float total = cartItems.Sum(ci => ci.TotalPrice);

            var cart = new Models.Domain.Cart
            {
                Id = Guid.NewGuid(),
                Total = total,
                CreatedAt = DateTime.UtcNow,
                CartItems = cartItems
            };

            foreach (var item in cartItems)
            {
                item.CartId = cart.Id;
            }

            await _context.carts.AddAsync(cart);
            await _context.SaveChangesAsync();

            return new CartResponse
            {
                Id = cart.Id,
                CreatedAt = cart.CreatedAt,
                Total = cart.Total,
                CartItems = cartItems.Select(ci => new CartItemInCartResponse
                {
                    Id = ci.Id,
                    MenuItemName = ci.MenuItem.Name,
                    MeatName = ci.MeatType.Name,
                    SouceName = ci.Souce.Name,
                    ExtraNames = ci.ExtraIngredientsLinks.Select(ei => ei.ExtraIngredient.Name).ToList(),
                    TotalPrice = ci.TotalPrice
                }).ToList()
            };
        }

        public async Task<CartResponse> AddExistingItemAsync(Guid cartId, Guid cartItemId)
        {
            // Znajdź istniejący koszyk
            var cart = await _context.carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.MenuItem)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.MeatType)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Souce)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ExtraIngredientsLinks)
                        .ThenInclude(ei => ei.ExtraIngredient)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
                throw new ArgumentException("Koszyk o podanym ID nie istnieje.");

            // Znajdź CartItem do dodania
            var cartItem = await _context.cartItems
                .Include(ci => ci.MenuItem)
                .Include(ci => ci.MeatType)
                .Include(ci => ci.Souce)
                .Include(ci => ci.ExtraIngredientsLinks)
                    .ThenInclude(ei => ei.ExtraIngredient)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null)
                throw new ArgumentException("CartItem nie istnieje.");

            if (cartItem.CartId != null)
                throw new InvalidOperationException("CartItem jest już przypisany do koszyka.");

            // Przypisz item do koszyka
            cartItem.CartId = cart.Id;
            cart.CartItems.Add(cartItem);

            // Zaktualizuj cenę koszyka
            cart.Total += cartItem.TotalPrice;

            await _context.SaveChangesAsync();

            return new CartResponse
            {
                Id = cart.Id,
                CreatedAt = cart.CreatedAt,
                Total = cart.Total,
                CartItems = cart.CartItems.Select(ci => new CartItemInCartResponse
                {
                    Id = ci.Id,
                    MenuItemName = ci.MenuItem.Name,
                    MeatName = ci.MeatType.Name,
                    SouceName = ci.Souce.Name,
                    ExtraNames = ci.ExtraIngredientsLinks.Select(ei => ei.ExtraIngredient.Name).ToList(),
                    TotalPrice = ci.TotalPrice
                }).ToList()
            };
        }



        public async Task<IEnumerable<CartResponse>> GetAllAsync()
        {
            return await _context.carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.MenuItem)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CartResponse
                {
                    Id = c.Id,
                    CreatedAt = c.CreatedAt,
                    Total = c.Total,
                    CartItems = c.CartItems.Select(ci => new CartItemInCartResponse
                    {
                        Id = ci.Id,
                        MenuItemName = ci.MenuItem.Name,
                        MeatName = ci.MeatType.Name,
                        SouceName = ci.Souce.Name,
                        ExtraNames = ci.ExtraIngredientsLinks.Select(ei => ei.ExtraIngredient.Name).ToList(),
                        TotalPrice = ci.TotalPrice
                    }).ToList()
                })
                .ToListAsync();
        }


        public async Task<IEnumerable<CartResponse>> GetAllNonCompleteAsync()
        {
         return await _context.carts
         .Include(c => c.CartItems)
             .ThenInclude(ci => ci.MenuItem)
             .Where(c => !c.IsFinished && c.IsPaid)
         .OrderByDescending(c => c.CreatedAt)
         .Select(c => new CartResponse
         {
             Id = c.Id,
             CreatedAt = c.CreatedAt,
             Total = c.Total,
             CartItems = c.CartItems.Select(ci => new CartItemInCartResponse
             {
                 Id = ci.Id,
                 MenuItemName = ci.MenuItem.Name,
                 MeatName = ci.MeatType.Name,
                 SouceName = ci.Souce.Name,
                 ExtraNames = ci.ExtraIngredientsLinks.Select(ei => ei.ExtraIngredient.Name).ToList(),
                 TotalPrice = ci.TotalPrice
             }).ToList()
         })
         .ToListAsync();
        }

        public async Task<bool>ComleteCardAsync(Guid id)
        {
            var cart = await _context.carts.FindAsync(id);
            if (cart == null) return false;

            cart.IsFinished = true;
            cart.IsActive = false;
            cart.IsProcessed = true; 

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CartResponse?> GetByIdAsync(Guid id)
        {
            var cart = await _context.carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.MenuItem)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.MeatType)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Souce)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ExtraIngredientsLinks)
                        .ThenInclude(link => link.ExtraIngredient)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cart == null) return null;

            return new CartResponse
            {
                Id = cart.Id,
                CreatedAt = cart.CreatedAt,
                Total = cart.Total,
                CartItems = cart.CartItems.Select(ci => new CartItemInCartResponse
                {
                    Id = ci.Id,
                    MenuItemName = ci.MenuItem.Name,
                    MeatName = ci.MeatType.Name,
                    SouceName = ci.Souce.Name,
                    ExtraNames = ci.ExtraIngredientsLinks.Select(ei => ei.ExtraIngredient.Name).ToList(),
                    TotalPrice = ci.TotalPrice
                }).ToList()
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var cart = await _context.carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.MenuItem)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.MeatType)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Souce)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ExtraIngredientsLinks)
                        .ThenInclude(link => link.ExtraIngredient)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cart == null) return false;

            foreach (var item in cart.CartItems)
            {
                item.CartId = null;
            }

            _context.carts.Remove(cart);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCartItemCountAsync(Guid cartId)
        {
            return await _context.cartItems
                .Where(ci => ci.CartId == cartId)
                .CountAsync();
        }
    }

}
