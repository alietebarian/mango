using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class OrderHeader
{
    [Key]
    public int OrderHeaderId { get; set; }
    public string PickupName { get; set; }
    public string PickupPhoneNumber { get; set; }
    public string pickupEmail { get; set; }
    public string ApplicationUserId { get; set; }
    public ApplicationUser User { get; set; }
    public double OrderTotal { get; set; }

    public DateTime OrderDate { get; set; }
    public string StripePaymentIntentID { get; set; }
    public string Status { get; set; }
    public int TotalItems { get; set; }

    public IEnumerable<OrderDetails> OrderDetails { get; set; }
}
