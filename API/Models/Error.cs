namespace API.Models
{
    public sealed record Error(string? StatusCode, string? Description = null)
    {
        public static readonly Error None = new(string.Empty);
        public static implicit operator Result(Error error) => Result.Failure(error);
    }
}
