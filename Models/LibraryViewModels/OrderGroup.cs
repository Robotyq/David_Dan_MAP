using System.ComponentModel.DataAnnotations;

namespace David_Dan_MAP.Models.LibraryViewModels
{
    public class OrderGroup
    {
        [DataType(DataType.Date)]
        public DateTime? OrderDate { get; set; }
        public int BookCount { get; set; }
    }
}
