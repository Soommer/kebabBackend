using kebabBackend.Data;
using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System;

namespace kebabBackend.Repositories.CartItems
{
    public class CartItemRepository : ICartItemRepository
    {
        private readonly KebabDbContext _context;

        public CartItemRepository(KebabDbContext context)
        {
            this._context = context;
        }

        public async Task<CartItem> CreateAsync(CartItemCreateRequest request)
        {
            var menuItem = await _context.menuItems.FindAsync(request.MenuItemId)
                          ?? throw new ArgumentException("Nie znaleziono pozycji menu.");

            var meatType = await _context.meatTypes.FindAsync(request.MeatTypeId)
                           ?? throw new ArgumentException("Nie znaleziono typu mięsa.");

            var souce = await _context.souces.FindAsync(request.SouceId)
                         ?? throw new ArgumentException("Nie znaleziono sosu.");

            var extras = await _context.extraIgredients
                .Where(e => request.ExtraIngredientIds.Contains(e.Id))
                .ToListAsync();

            int sizePrice = 0;

            switch(request.Size)
            {
                case "small":
                    sizePrice = 0; break;
                case "medium":
                    sizePrice = 4; break;
                case "big":
                    sizePrice = 8; break;
            }

            float total = menuItem.BasePrice + meatType.ExtraPrice + extras.Sum(e => e.price) + sizePrice;

            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                MenuItemId = menuItem.Id,
                MenuItem = menuItem,
                MeatTypeId = meatType.Id,
                MeatType = meatType,
                SouceId = souce.Id,
                Souce = souce,
                Size = request.Size,
                CreatedAt = DateTime.UtcNow,
                TotalPrice = total
            };

            foreach (var extra in extras)
            {
                cartItem.ExtraIngredientsLinks.Add(new CartItemExtraIngredient
                {
                    Id = Guid.NewGuid(),
                    ExtraIngredientId = extra.Id,
                    ExtraIngredient = extra,
                    CartItemId = cartItem.Id,
                    CartItem = cartItem
                });
            }

            await _context.cartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();

            return cartItem;
        }

        public async Task<CartItem?> GetByIdAsync(Guid id)
        {
            return await _context.cartItems
                .Include(ci => ci.MenuItem)
                .Include(ci => ci.MeatType)
                .Include(ci => ci.Souce)
                .Include(ci => ci.ExtraIngredientsLinks)
                    .ThenInclude(link => link.ExtraIngredient)
                .FirstOrDefaultAsync(ci => ci.Id == id);
        }

        public async Task<IEnumerable<CartItem>> GetAllAsync()
        {
            return await _context.cartItems
                .Include(ci => ci.MenuItem)
                .Include(ci => ci.MeatType)
                .Include(ci => ci.Souce)
                .Include(ci => ci.ExtraIngredientsLinks)
                    .ThenInclude(link => link.ExtraIngredient)
                .OrderByDescending(ci => ci.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var item = await _context.cartItems.FindAsync(id);
            if (item == null) return false;

            _context.cartItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CartItemResponse?> GetResponseAsync(Guid id)
        {
            var item = await _context.cartItems
                .Include(ci => ci.MenuItem)
                .Include(ci => ci.MeatType)
                .Include(ci => ci.Souce)
                .Include(ci => ci.ExtraIngredientsLinks)
                    .ThenInclude(link => link.ExtraIngredient)
                .FirstOrDefaultAsync(ci => ci.Id == id);

            if (item == null) return null;

            return new CartItemResponse
            {
                Id = item.Id,
                MenuItemName = item.MenuItem.Name,
                MeatTypeName = item.MeatType.Name,
                SouceName = item.Souce.Name,
                TotalPrice = item.TotalPrice,
                ExtraIngredientNames = item.ExtraIngredientsLinks
                    .Select(x => x.ExtraIngredient.Name)
                    .ToList()
            };
        }

        public async Task<IEnumerable<CartItemResponse>> GetAllResponsesAsync()
        {
            return await _context.cartItems
                .Include(ci => ci.MenuItem)
                .Include(ci => ci.MeatType)
                .Include(ci => ci.Souce)
                .Include(ci => ci.ExtraIngredientsLinks)
                    .ThenInclude(link => link.ExtraIngredient)
                .OrderByDescending(ci => ci.CreatedAt)
                .Select(item => new CartItemResponse
                {
                    Id = item.Id,
                    MenuItemName = item.MenuItem.Name,
                    MeatTypeName = item.MeatType.Name,
                    SouceName = item.Souce.Name,
                    TotalPrice = item.TotalPrice,
                    ExtraIngredientNames = item.ExtraIngredientsLinks
                        .Select(x => x.ExtraIngredient.Name)
                        .ToList()
                })
                .ToListAsync();
        }
    }
}
