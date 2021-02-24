
using StoreApi.DAL;
using StoreApi.Models;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;

namespace ConsoleApp
{
    class Program
    {
        private static async System.Threading.Tasks.Task Main()
        {
            try
            {
            
                var baseUri = ConfigurationManager.AppSettings["StartupUri"];

                var httpClient = new HttpClient() { BaseAddress = new Uri($"{baseUri}api/v1/Store/") };

                var categories = await httpClient.GetFromJsonAsync<Category[]> ("Categories");

                Console.WriteLine("***** These are the categories *****");
                foreach (var category in categories)
                {
                    Console.WriteLine(category.Name);
                }

                Console.WriteLine($"\n***** These are the the featured products *****");

                var featuredProducts = await httpClient.GetFromJsonAsync<Product[]>("FeaturedProducts");

                foreach (var product in featuredProducts)
                {
                    PrintProduct(product);
                }

                Console.WriteLine($"\n***** These are the the products in category {categories[0].Name} *****");

                var catProducts = await httpClient.GetFromJsonAsync<Product[]>($"ProductsByCat/{categories[0].Name}");

                foreach (var product in catProducts)
                {
                    PrintProduct(product);
                }

                Console.ReadLine();
            }
            catch(DataServiceException e)
            {
                Console.WriteLine($" Exception occurred: {e.InnerException}");
            }            
        }

        private static void PrintProduct(Product product)
        {
            Console.WriteLine($"SKU {product.SKU} \tName {product.Name} \tDescription {product.Description} \tPrice {product.Price}");
        }
    }
}
