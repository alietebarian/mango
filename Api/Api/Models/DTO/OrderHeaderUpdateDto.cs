using System.ComponentModel.DataAnnotations;

namespace Api.Models.DTO;

public class OrderHeaderUpdateDto
{
    public int OrderHeaderId { get; set; }
    public string PickupName { get; set; }
    public string PickupPhoneNumber { get; set; }
    public string pickupEmail { get; set; }
    public string StripePaymentIntentID { get; set; }
    public string Status { get; set; }
}
