using System.ComponentModel.DataAnnotations;

namespace Api.Models.DTO;

public class OrderDetailsCreateDto
{   public int MenuItemId { get; set; }
    public int Quantity { get; set; }
    public string ItemName { get; set; }
    public double Price { get; set; }
}
