
using StoreCore.Models;
using System.Collections.Generic;

namespace StoreDAL
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
        public IList<Category> GetCategories();

        /// <summary>
        /// Get products belonging to a specified category
        /// </summary>
        /// <param name="category">The category</param>
        /// <returns>A list of Products</returns>
        public IList<Product> GetProductsByCategory(string category);

        /// <summary>
        /// Get featured products
        /// </summary>
        /// <returns>A list of Products</returns>
        public IList<Product> GetFeaturedProducts();
    }
}
