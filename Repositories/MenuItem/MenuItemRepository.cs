using Azure.Storage.Blobs;
using kebabBackend.Data;
using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace kebabBackend.Repositories.MenuItem
{
    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly KebabDbContext DbContext;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BlobServiceClient _blobServiceClient;

        public MenuItemRepository(KebabDbContext dbContext, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, BlobServiceClient blobServiceClient)
        {
            DbContext = dbContext;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _blobServiceClient = blobServiceClient;
        }

        public async Task<menuItem> AddMenuItemAsync(menuItem menuItem, IFormFile imageFile)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(imageFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Nieobsługiwany format pliku.");

            if (imageFile.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("Plik jest zbyt duży.");

            var imageId = Guid.NewGuid();
            var fileName = $"{imageId}{extension}";

            // Upload to Blob
            var blobClient = _blobServiceClient
                .GetBlobContainerClient("images")
                .GetBlobClient(fileName);

            await using var stream = imageFile.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            var blobUrl = blobClient.Uri.ToString();

            var kebabImage = new kebabImage
            {
                Id = imageId,
                Name = imageFile.FileName,
                FileExtention = extension,
                FileSizeInBytes = imageFile.Length,
                FilePath = blobUrl
            };

            var category = await DbContext.menuItemCategories.FindAsync(menuItem.CategoryId)
                           ?? throw new InvalidOperationException("Nie znaleziono kategorii.");

            menuItem.Id = Guid.NewGuid();
            menuItem.ImageId = kebabImage.Id;
            menuItem.Image = kebabImage;
            menuItem.Category = category;

            await DbContext.kebabImages.AddAsync(kebabImage);
            await DbContext.menuItems.AddAsync(menuItem);
            await DbContext.SaveChangesAsync();

            return menuItem;
        }


        public async Task<IEnumerable<MenuItemReturn>> GetAllMenuItemsAsync(Guid? categoryId = null)
        {
            var query = DbContext.menuItems
                .Include(m => m.Image)
                .Include(m => m.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(m => m.CategoryId == categoryId);
            }

            return await query
                .OrderBy(m => m.Name)
                .Select(m => new MenuItemReturn
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    BasePrice = m.BasePrice,
                    ImagePath =$"https://blobkebab.blob.core.windows.net/images/{m.Image.FilePath}",
                    CategoryName = m.Category.Name,
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteMenuItemAsync(Guid id)
        {
            var menuItem = await DbContext.menuItems
                .Include(m => m.Image)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null)
                return false;

            var filePath = Path.Combine(_env.ContentRootPath, "Images", $"{menuItem.Image.Id}{menuItem.Image.FileExtention}");
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (IOException ex)
                {
                    throw new Exception((ex.ToString()));
                }
            }

            DbContext.kebabImages.Remove(menuItem.Image);
            DbContext.menuItems.Remove(menuItem);

            await DbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateMenuItemAsync(Guid id, MenuItemUpdateRequest request)
        {
            var menuItem = await DbContext.menuItems
                .Include(m => m.Image)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null)
                return false;

            menuItem.Name = request.Name;
            menuItem.Description = request.Description;
            menuItem.BasePrice = request.BasePrice;

            if (request.NewImage is not null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(request.NewImage.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    throw new InvalidOperationException("Nieobsługiwany format pliku.");

                if (request.NewImage.Length > 5 * 1024 * 1024)
                    throw new InvalidOperationException("Plik jest zbyt duży.");

                var oldPath = Path.Combine(_env.ContentRootPath, "Images", $"{menuItem.Image.Id}{menuItem.Image.FileExtention}");
                if (File.Exists(oldPath))
                {
                    try { File.Delete(oldPath); } catch { }
                }

                var newImageId = Guid.NewGuid();
                var fileName = $"{newImageId}{extension}";
                var folder = Path.Combine(_env.ContentRootPath, "Images");
                Directory.CreateDirectory(folder);
                var newFilePath = Path.Combine(folder, fileName);

                await using var stream = new FileStream(newFilePath, FileMode.Create);
                await request.NewImage.CopyToAsync(stream);

                var urlPath = $"{_httpContextAccessor.HttpContext!.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}/Images/{fileName}";

                menuItem.ImageId = newImageId;
                menuItem.Image = new kebabImage
                {
                    Id = newImageId,
                    Name = request.NewImage.FileName,
                    FileExtention = extension,
                    FileSizeInBytes = request.NewImage.Length,
                    FilePath = urlPath
                };
            }

            await DbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool>UpdateAsyncNoPicture(Guid id, MenuItemUpdateNoPicture request)
        {
            var entity = await DbContext.menuItems.FindAsync(id);
            if (entity == null) return false;

            entity.BasePrice = request.BasePrice;
            entity.Name = request.Name; 
            entity.Description = request.Description;

            await DbContext.SaveChangesAsync();
            return true;
        }
    }
}
