using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyProductSearchAPI.Models;
using System.Data;
using System.Drawing;
using System.Text.Json;
using System.Web.Http;

namespace MyProductSearchAPI.Controllers
{
    [ApiController]
    [System.Web.Http.Route("[Controller]")]
    public class ProductsController : ControllerBase
    {
        #region Global variable declarations
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string ProductsUrl = "https://pastebin.com/raw/JucRNpWs";
        #endregion

        #region Constructor
        public ProductsController(IConfiguration configuration, ILogger<ProductsController> logger)
        {
            _logger = logger;
            _configuration = configuration;

        }
        #endregion

        #region Endpoints
        /// <summary>
        /// Endpoint to retrieve, filter and highlight the products
        /// </summary>
        /// <param name="minPrice"></param>
        /// <param name="maxPrice"></param>
        /// <param name="size"></param>
        /// <param name="highlight"></param>
        /// <returns></returns>
        [Microsoft.AspNetCore.Mvc.HttpGet("ProductFilter")]

        public async Task<IActionResult> GetFilteredProducts(
                [FromQuery] decimal? minPrice,
                [FromQuery] decimal? maxPrice,
                [FromQuery] string? size,
                [FromQuery] string? highlight)
        {
            _logger.LogInformation("API requested with parameters ", DateTime.UtcNow.ToLongTimeString());


            List<Products> products = await RetrieveProductsAsync(ProductsUrl);
            var filteredProducts = FilterProducts(products, minPrice, maxPrice, size, highlight);

            // Highlight words in descriptions
            if (!string.IsNullOrWhiteSpace(highlight))
            {
                var wordsToHighlight = highlight.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var product in filteredProducts)
                {
                    foreach (var word in wordsToHighlight)
                    {
                        product.description = product.description.Replace(word.Trim(), $"<em>{word.Trim()}</em>", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            var filterInfo = new FilterInfo
            {
                MinPrice = products.Min(p => p.price),
                MaxPrice = products.Max(p => p.price),
                //Sizes = products.Select(p => p.sizes),
                CommonWords = GetCommonWords(products.Select(p => p.description).ToList())
            };

            return Ok(new FilteredResponse
            {
                Products = filteredProducts.ToList(),
                Filter = filterInfo
            });

        }
        #endregion

        #region Methods
        /// <summary>
        /// Async method to access the URL and return the produts list 
        /// </summary>
        /// <param name="ProductsUrl" required></param>
        /// <returns>List<Products></returns>
        private async Task<List<Products>> RetrieveProductsAsync(string ProductsUrl)
        {
            var response = await _httpClient.GetStringAsync(ProductsUrl);
            var jsondt = JsonSerializer.Deserialize<jsonData>(response);
            List<Products> products = new List<Products>();
            
            if (jsondt == null || jsondt.products.Count == 0)
                _logger.LogInformation("No data received ", DateTime.UtcNow.ToLongTimeString());
            else
            {
                products = jsondt.products;
                _logger.LogInformation("Data received - Count " + products.Count.ToString(), DateTime.UtcNow.ToLongTimeString());
            }
            return products;
        }


        /// <summary>
        /// Filter the products based on the given parameters and return the filtered products list
        /// </summary>
        /// <param name="products" required></param>
        /// <param name="minPrice" optional></param>
        /// <param name="maxPrice" optional></param>
        /// <param name="size" optional></param>
        /// <param name="highlight" optional></param>
        /// <returns>List<Products></returns>
        private IQueryable<Products> FilterProducts(List<Products> products, decimal? minPrice,
            decimal? maxPrice,
            string? size,
            string? highlight)
        {
            // Get filter info

            // Filter products
            var filteredProducts = products.AsQueryable();

            if (minPrice.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.price <= maxPrice.Value);
            }
            if (!string.IsNullOrWhiteSpace(size))
            {
                foreach (string s in size.Split(','))
                {
                    filteredProducts = filteredProducts.Where(p => p.sizes.Contains(s));
                }
            }
            _logger.LogInformation("Filtered products - Count " + filteredProducts.Count().ToString(), DateTime.UtcNow.ToLongTimeString());
            return filteredProducts;
        }

        /// <summary>
        /// To get the comming words from description of all products
        /// </summary>
        /// <param name="descriptions" required></param>
        /// <returns>string[]</returns>
        private string[] GetCommonWords(List<string> descriptions)
        {
            var wordFrequency = new Dictionary<string, int>();

            foreach (var description in descriptions)
            {
                var words = description.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    var lowerWord = word.ToLower();
                    if (wordFrequency.ContainsKey(lowerWord))
                    {
                        wordFrequency[lowerWord]++;
                    }
                    else
                    {
                        wordFrequency[lowerWord] = 1;
                    }
                }
            }

            return wordFrequency
                .OrderByDescending(w => w.Value)
                .Skip(5) // Skip the most common 5
                .Take(10) // Take the next 10
                .Select(w => w.Key)
                .ToArray();
        }
        #endregion
    }
}


