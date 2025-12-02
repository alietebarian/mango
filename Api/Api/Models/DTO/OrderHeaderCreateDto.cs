namespace Api.Models.DTO;

public class OrderHeaderCreateDto
{
    public string PickupName { get; set; }
    public string PickupPhoneNumber { get; set; }
    public string pickupEmail { get; set; }
    public string ApplicationUserId { get; set; }
    public double OrderTotal { get; set; }
    public string StripePaymentIntentID { get; set; }
    public string Status { get; set; }
    public int TotalItems { get; set; }

    public IEnumerable<OrderDetailsCreateDto> OrderDetailsDto { get; set; }
}
