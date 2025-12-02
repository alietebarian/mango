using Api.Data;
using Api.Models;
using Api.Models.DTO;
using Api.utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrderController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(string? userId)
    {
        IQueryable<OrderHeader> query = _context.OrderHeaders
            .Include(x => x.OrderDetails)
            .ThenInclude(x => x.MenuItem)
            .OrderByDescending(x => x.OrderHeaderId);

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(x => x.ApplicationUserId == userId);
        }

        var result = await query.ToListAsync();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        if (id == 0)
            return BadRequest("Id cannot be 0");

        var order = await _context.OrderHeaders
            .Include(x => x.OrderDetails)
            .ThenInclude(x => x.MenuItem)
            .FirstOrDefaultAsync(x => x.OrderHeaderId == id);

        if (order == null)
            return NotFound("Order not found");

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderHeaderCreateDto orderHeaderDto)
    {
        OrderHeader order = new()
        {
            ApplicationUserId = orderHeaderDto.ApplicationUserId,
            pickupEmail = orderHeaderDto.pickupEmail,
            PickupName = orderHeaderDto.PickupName,
            PickupPhoneNumber = orderHeaderDto.PickupPhoneNumber,
            OrderTotal = orderHeaderDto.OrderTotal,
            OrderDate = DateTime.Now,
            StripePaymentIntentID = orderHeaderDto.StripePaymentIntentID,
            TotalItems = orderHeaderDto.TotalItems,
            Status = String.IsNullOrEmpty(orderHeaderDto.Status)? SD.Status_pending : orderHeaderDto.Status,
        };

        if (ModelState.IsValid)
        {
            _context.OrderHeaders.Add(order);   
            await _context.SaveChangesAsync();

            foreach(var orderDetailsDto in orderHeaderDto.OrderDetailsDto)
            {
                OrderDetails orderDetails = new()
                {
                    OrderHeaderId = order.OrderHeaderId,
                    ItemName = orderDetailsDto.ItemName,
                    MenuItemId = orderDetailsDto.MenuItemId,
                    Price = orderDetailsDto.Price,
                    Quantity = orderDetailsDto.Quantity,
                };

                _context.OrderDetails.Add(orderDetails);
            }
                await _context.SaveChangesAsync();
        }

        return Ok(order);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrderHeader(int id, [FromBody] OrderHeaderUpdateDto orderHeaderUpdateDto)
    {
        if (orderHeaderUpdateDto == null || id != orderHeaderUpdateDto.OrderHeaderId)
        {
            return BadRequest();
        }

        OrderHeader orderFromDb = _context.OrderHeaders.FirstOrDefault(z => z.OrderHeaderId == id);

        if (orderFromDb == null)
            return BadRequest();

        if(!string.IsNullOrEmpty(orderHeaderUpdateDto.PickupName))
            orderFromDb.PickupName = orderHeaderUpdateDto.PickupName;
        if (!string.IsNullOrEmpty(orderHeaderUpdateDto.PickupPhoneNumber))
            orderFromDb.PickupPhoneNumber = orderHeaderUpdateDto.PickupPhoneNumber;
        if (!string.IsNullOrEmpty(orderHeaderUpdateDto.pickupEmail))
            orderFromDb.pickupEmail = orderHeaderUpdateDto.pickupEmail;
        if (!string.IsNullOrEmpty(orderHeaderUpdateDto.Status))
            orderFromDb.Status = orderHeaderUpdateDto.Status;
        if (!string.IsNullOrEmpty(orderHeaderUpdateDto.StripePaymentIntentID))
            orderFromDb.StripePaymentIntentID = orderHeaderUpdateDto.StripePaymentIntentID;

        await _context.SaveChangesAsync();

        return Ok(orderFromDb);
    }

}
