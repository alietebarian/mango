using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class OrderDetails
{
    [Key]
    public int OrderDetailId { get; set; }
    public int OrderHeaderId { get; set; }
    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; }

    public int Quantity { get; set; }
    public string ItemName { get; set; }
    public double Price { get; set; }
}
