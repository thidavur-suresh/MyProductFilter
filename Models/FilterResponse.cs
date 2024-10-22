namespace MyProductSearchAPI.Models
{
    public class FilteredResponse
    {
        public List<Products> Products { get; set; }
        public FilterInfo Filter { get; set; }
    }

    public class FilterInfo
    {
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public List<string> Sizes { get; set; }
        public string[] CommonWords { get; set; }
    }
}
