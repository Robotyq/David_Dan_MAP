namespace David_Dan_MAP.Models
{
    public class Book
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public Author? Author { get; set; }
        public int? AuthorID { get; set; }
        public decimal Price { get; set; }
        public int? GenreID { get; set; }
        public Genre? Genre { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
