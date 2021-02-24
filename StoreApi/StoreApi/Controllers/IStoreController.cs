using Microsoft.AspNetCore.Mvc;
using StoreApi.Models;
using System.Threading.Tasks;

namespace StoreApi.Controllers
{
    public interface IStoreController
    {
        Task<IActionResult> GetCategoriesAsync();

        Task<IActionResult> GetFeaturedProductsAsync();

        Task<IActionResult> GetProductsByCategoryAsync([FromRoute] string category);

        Task<IActionResult> AddProductAsync(Product product);
    }
}