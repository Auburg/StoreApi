
using StoreApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StoreApi.DAL
{
    /// <summary>
    /// Interface for the Data Service
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Get the categories
        /// </summary>
        /// <returns>A list of Category items</returns>
        Task<IList<Category>> GetCategoriesAsync();

        /// <summary>
        /// Get products belonging to a specified category
        /// </summary>
        /// <param name="category">The category</param>
        /// <returns>A list of Products</returns>
        Task<(bool isValidCategory, IList<Product> products)> GetProductsByCategoryAsync(string category);

        /// <summary>
        /// Adds a product to the list of products. The product SKU must be valid (i.e. must refer to an existing category)
        /// </summary>
        /// <param name="product">The <see cref="Product"/> to add</param>
        /// <returns>The id of the product that was added or -1 if the product SKU was invalid</returns>
        Task<int> AddProductAsync(Product product);

        /// <summary>
        /// Get featured products
        /// </summary>
        /// <returns>A list of Products</returns>
        Task<IList<Product>> GetFeaturedProductsAsync();       
    }
}
