namespace Loca.Domain.Entities;

public class UserInterest
{
    public Guid UserId { get; set; }
    public string Interest { get; set; } = string.Empty;
}

public class UserPurpose
{
    public Guid UserId { get; set; }
    public string Purpose { get; set; } = string.Empty;
}
