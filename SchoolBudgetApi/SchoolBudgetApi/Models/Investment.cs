namespace SchoolBudgetApi.Models;

public class Investment
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public string EshopLink { get; set; } = "";
    public bool IsReoccuring { get; set; }
    public bool? IsPending { get; set; }
    public decimal Cost { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}