using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Data;
using ProdutosAPI.Models;

namespace ProdutosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly DataContext _context;

        public ProductsController(DataContext context)
        {
            _context = context;
        }

        // POST: Criar Produto
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Nome))
                return BadRequest(new { error = "O nome não pode ser vazio." });

            if (product.Preco <= 0)
                return BadRequest(new { error = "O preço deve ser maior que zero." });

            product.Nome = product.Nome.ToUpper();
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // GET: Obter Produtos
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] string? nome, [FromQuery] string? ordem = "asc")
        {
            var products = await _context.Products.ToListAsync();

            foreach (var product in products)
            {
                if (product.Nome.Contains("PROMOÇÃO", StringComparison.OrdinalIgnoreCase))
                    product.Nome += " [Em Promoção]";
            }

            if (!string.IsNullOrEmpty(nome))
                products = products.Where(p => p.Nome.Contains(nome, StringComparison.OrdinalIgnoreCase)).ToList();

            products = ordem?.ToLower() == "desc"
                ? products.OrderByDescending(p => p.Preco).ToList()
                : products.OrderBy(p => p.Preco).ToList();

            return Ok(products);
        }

        // GET: Obter Produto por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // PUT: Atualizar Produto
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
                return NotFound();

            if (product.Preco < 0)
                return BadRequest(new { error = "O preço deve ser maior que zero." });

            if (!string.IsNullOrWhiteSpace(product.Nome))
                existingProduct.Nome = char.ToUpper(product.Nome[0]) + product.Nome.Substring(1).ToLower();

            existingProduct.Preco = product.Preco;

            _context.Products.Update(existingProduct);
            await _context.SaveChangesAsync();

            return Ok(existingProduct);
        }

        // DELETE: Excluir Produto
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
