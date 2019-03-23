using Nest;
public class CustomerModel
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public int LocationId { get; set; }
    public string LocationName { get; set; }

    public GeoLocation Location { get; set; }
}
public class Message
{
    public GeoLocation Location { get; set; }
}