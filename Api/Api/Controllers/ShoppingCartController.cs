using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ShoppingCartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetShoppingCart(string userId)
        {

            ShoppingCart shoppingCart;

            if (string.IsNullOrEmpty(userId))
                shoppingCart = new();

            else
            {
                shoppingCart = _context.shoppingCarts
                    .Include(x => x.CartItems)
                    .ThenInclude(x => x.MenuItem)
                    .FirstOrDefault(x => x.UserId == userId);
            }

            if(shoppingCart.CartItems != null && shoppingCart.CartItems.Count > 0)
            {
                shoppingCart.CartTotal = shoppingCart.CartItems.Sum(x => x.Quantity * x.MenuItem.Price);
            }

            return Ok(shoppingCart);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateItemInCart(string userId, int menuItemId, int updateQuantityBy)
        {
            ShoppingCart shoppingCart = _context.shoppingCarts.FirstOrDefault(x => x.UserId == userId);
            MenuItem menuItem = _context.MenuItems.FirstOrDefault(x => x.Id == menuItemId);

            if (menuItem == null)
                return BadRequest("menuitem is empty");
            
            if(shoppingCart == null && updateQuantityBy > 0)
            {
                //create shopping cart and add item

                ShoppingCart newCart = new() { UserId = userId };

                _context.shoppingCarts
                    .Add(newCart);
                await _context.SaveChangesAsync();

                CartItem newCartItem = new()
                {
                    MenuItemId = menuItemId,
                    Quantity = updateQuantityBy,
                    ShoppingCartId = newCart.Id,
                    MenuItem = null
                };

                _context.CartItems.Add(newCartItem);
                await _context.SaveChangesAsync();
            }
            else
            {
                CartItem cartItemInCart = shoppingCart.CartItems
                    .FirstOrDefault(x => x.MenuItemId == menuItemId);

                if(cartItemInCart == null)
                {
                    CartItem newCartItem = new()
                    {
                        MenuItemId = menuItemId,
                        Quantity = updateQuantityBy,
                        ShoppingCartId = shoppingCart.Id,
                        MenuItem = null
                    };

                    _context.CartItems.Add(newCartItem);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    int newQuantity = cartItemInCart.Quantity + updateQuantityBy;

                    if(updateQuantityBy < 0 || newQuantity <= 0)
                    {
                        _context.CartItems.Remove(cartItemInCart);

                        if(shoppingCart.CartItems.Count == 1) 
                            _context.shoppingCarts.Remove(shoppingCart);

                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        cartItemInCart.Quantity = newQuantity;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            return Ok(shoppingCart);
        }
    }
}
