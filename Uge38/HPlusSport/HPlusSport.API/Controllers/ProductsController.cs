using HPlusSport.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HPlusSport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ShopContext _context;

        public ProductsController(ShopContext context)
        {
            _context = context;

            _context.Database.EnsureCreated();
        }

        //[HttpGet]
        //public IEnumerable<Product> GetAllProducts()
        //{
        //    return _context.Products.ToArray();
        //}

        //[HttpGet]
        //public ActionResult GetAllProducts()
        //{
        //    return Ok(_context.Products.ToArray());
        //}

        [HttpGet]
        public async Task<ActionResult> GetAllProducts()
        {
            return Ok(await _context.Products.ToArrayAsync());
        }

        //[HttpGet("{id}")]
        //public ActionResult GetProducts(int id)
        //{
        //    var product = _context.Products.Find(id);
        //    return Ok(product);
        //}

        [HttpGet("{id}")]
        public async Task<ActionResult> GetProducts(int id)
        {
            var product = _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }


    }
}
