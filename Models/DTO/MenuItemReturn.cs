namespace kebabBackend.Models.DTO
{
    public class MenuItemReturn
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float BasePrice { get; set; }
        public string ImagePath { get; set; }
        public string CategoryName { get; set; }
    }
}
