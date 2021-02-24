using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StoreApi.DAL;
using StoreApi.Models;
using System.Threading.Tasks;

namespace StoreApi.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class StoreController : ControllerBase, IStoreController
    {
        private readonly ILogger<StoreController> _logger;
        private readonly IDataService dataService;

        public StoreController(ILogger<StoreController> logger, IDataService dataService)
        {
            _logger = logger;
            this.dataService = dataService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("Categories")]        
        public async Task<IActionResult> GetCategoriesAsync()
        {
            try
            {
                return new JsonResult(await dataService.GetCategoriesAsync());
            }
            catch(DataServiceException ex)
            {
                _logger.Log(LogLevel.Error,ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("FeaturedProducts")]
        public async Task<IActionResult> GetFeaturedProductsAsync()
        {
            try
            {
                return new JsonResult(await dataService.GetFeaturedProductsAsync());
            }
            catch(DataServiceException ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("ProductsByCat/{category}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]       
        public async Task<IActionResult> GetProductsByCategoryAsync([FromRoute] string category)
        {
            try
            {
                if (string.IsNullOrEmpty(category))
                    return new BadRequestResult();

                var ret = await dataService.GetProductsByCategoryAsync(category);

                return !ret.isValidCategory ? new NotFoundResult() : new JsonResult(ret.products);
            }
            catch(DataServiceException ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("AddProductAsync")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]       
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddProductAsync([FromBody] Product product)
        {
            try
            {
                if(product == null)
                {
                    return new BadRequestResult();
                }

                var prodId = await dataService.AddProductAsync(product);

                return prodId == -1 ? new BadRequestObjectResult($"{product.SKU} invalid Category") : new JsonResult(prodId);
            }
            catch(DataServiceException ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
