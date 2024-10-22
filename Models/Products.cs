namespace MyProductSearchAPI.Models
{
    public class Products
    {
        public string? title { get; set; }
        public decimal price { get; set; }
        public List<string>? sizes { get; set; }
        public string? description { get; set; }
    }
    //public enum Size
    //{
    //  small,
    //  medium,
    //  large
    //}
    public class APIKey
    {
        public string? primary { get; set; }
        public string? secondary { get; set; }
    }

    public class jsonData
    {
        public List<Products>? products { get; set; }
        public APIKey? APIkey { get; set; }
    }
}
