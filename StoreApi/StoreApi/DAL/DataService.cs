
using Microsoft.Extensions.Configuration;
using StoreApi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Linq;

namespace StoreApi.DAL
{
    /// <inheritdoc/>
    public class DataService : IDataService
    {
        private readonly string _connectionString;        
        private string[] _featuredSkus;

        public DataService(IConfiguration configuration)
        {
            _connectionString = configuration["conn"];
            _featuredSkus = configuration["Featured"].Split(' ');            
        }

        /// <inheritdoc/>
        public async Task<IList<Category>> GetCategoriesAsync()
        {
            List<Category> categories = new List<Category>();

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    using (var comm = new SqlCommand())
                    {
                        comm.CommandText = "GetCategories";
                        comm.Connection = conn;
                        conn.Open();
                        comm.CommandType = CommandType.StoredProcedure;
                        using (var reader = await comm.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                string data = reader["Name"] as string;
                                categories.Add(new Category { Name = data });
                            }
                        }
                    }
                    conn.Close();
                }

                return categories;
            }
            catch(Exception ex)
            {
                throw new DataServiceException (ex.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<IList<Product>> GetFeaturedProductsAsync()
        { 
            List<Product> products = new List<Product>();

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    using (var comm = new SqlCommand())
                    {
                        comm.CommandText = "GetProducts";
                        comm.Connection = conn;
                        conn.Open();
                        comm.CommandType = CommandType.StoredProcedure;
                        using (var reader = await comm.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                var sku = reader["SKU"] as string;
                                if (_featuredSkus.Contains(sku[0].ToString()))
                                {
                                    products.Add(GetProductFromReader(reader));
                                }
                            }
                        }
                        conn.Close();
                    }
                }

                return products;
            }
            catch (Exception ex)
            {
                throw new DataServiceException(ex.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<int> AddProductAsync(Product product)
        {
            try
            {
                var iD = -1;
                using (var conn = new SqlConnection(_connectionString))
                {
                    using (var comm = new SqlCommand())
                    {
                        comm.CommandText = "InsertProduct";
                        comm.Connection = conn;
                        conn.Open();
                        comm.CommandType = CommandType.StoredProcedure;

                        product.AddParam(comm, "@SKU", product.SKU);
                        product.AddParam(comm, "@Name", product.Name);
                        product.AddParam(comm, "@Description", product.Description);
                        product.AddParam(comm, "@Price", product.Price);   
                        var outputParam = product.AddOutParam(comm, "@Id", SqlDbType.Int);
                        await comm.ExecuteNonQueryAsync();
                        iD = int.Parse(outputParam.Value.ToString());
                        conn.Close();
                    }
                }

                return iD;
            }
            catch (Exception ex)
            {
                throw new DataServiceException(ex.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<(bool isValidCategory, IList<Product> products)> GetProductsByCategoryAsync(string category)
        {
            List<Product> products = new List<Product>();

            try
            {
                var cats = await GetCategoriesAsync();
                if (!cats.Any(c=>c.Name==category))
                {
                    return (false, null);
                }

                return await GetProductsByCategoryInnerAsync(category, products);
            }
            catch (Exception ex)
            {
                throw new DataServiceException(ex.Message);
            }
        }

        private async Task<(bool isValidCategory, IList<Product> products)> GetProductsByCategoryInnerAsync(string category, List<Product> products)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var comm = new SqlCommand())
                {
                    comm.CommandText = "GetProductsForCategory";
                    comm.Connection = conn;
                    conn.Open();
                    comm.CommandType = CommandType.StoredProcedure;

                    var parameter = comm.CreateParameter();
                    parameter.ParameterName = "@category";
                    parameter.Value = category;
                    comm.Parameters.Add(parameter);

                    using (var reader = await comm.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            products.Add(GetProductFromReader(reader));
                        }
                    }
                }
            }
            return new(true, products);
        }

        private Product GetProductFromReader(IDataReader reader)
        {
            return new Product
            {
                SKU = (reader["SKU"] as string).Trim(),
                Name = (reader["Name"] as string).Trim(),
                Description = (reader["Description"] as string).Trim(),
                Price = (decimal)reader["Price"]
            };            
        }
    }
}
