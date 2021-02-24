using StoreCore;
using StoreCore.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace StoreDAL
{
    /// <inheritdoc/>
    public class DataService : IDataService
    {
        private readonly string _connectionString;      
        private IList<string> _featuredSkus;

        public DataService(IConfigurationService configurationService)
        {
            _connectionString = configurationService.ConnectionString;
            _featuredSkus = new List<string>(configurationService.FeaturedProductSkus);
        }

        /// <inheritdoc/>
        public IList<Category> GetCategories()
        {
            List<Category> categories = new List<Category>();

            try
            {
                using (IDbConnection conn = new System.Data.SqlClient.SqlConnection(_connectionString))
                {
                    using (IDbCommand comm = conn.CreateCommand())
                    {
                        comm.CommandText = "GetCategories";
                        conn.Open();
                        comm.CommandType = CommandType.StoredProcedure;
                        using (IDataReader reader = comm.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string data = reader["Name"] as string;
                                categories.Add(new Category { Name = data });
                            }
                        }
                    }
                }

                return categories;
            }
            catch(Exception ex)
            {
                throw new DataServiceException (ex.Message);
            }
        }

        /// <inheritdoc/>
        public IList<Product> GetFeaturedProducts()
        { 
            List<Product> products = new List<Product>();

            try
            {
                using (IDbConnection conn = new System.Data.SqlClient.SqlConnection(_connectionString))
                {
                    using (IDbCommand comm = conn.CreateCommand())
                    {
                        comm.CommandText = "GetProducts";
                        conn.Open();
                        comm.CommandType = CommandType.StoredProcedure;
                        using (IDataReader reader = comm.ExecuteReader())
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
        public IList<Product> GetProductsByCategory(string category)
        {
            List<Product> products = new List<Product>();

            try
            {
                using (IDbConnection conn = new System.Data.SqlClient.SqlConnection(_connectionString))
                {
                    using (IDbCommand comm = conn.CreateCommand())
                    {
                        comm.CommandText = "GetProductsForCategory";
                        conn.Open();
                        comm.CommandType = CommandType.StoredProcedure;

                        var parameter = comm.CreateParameter();
                        parameter.ParameterName = "@category";
                        parameter.Value = category;
                        comm.Parameters.Add(parameter);

                        using (IDataReader reader = comm.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                products.Add(GetProductFromReader(reader));
                            }
                        }
                    }
                }
                return products;
            }
            catch (Exception ex)
            {
                throw new DataServiceException(ex.Message);
            }
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
