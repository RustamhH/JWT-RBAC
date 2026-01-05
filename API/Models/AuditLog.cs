namespace API.Models
{
    public class AuditLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserEmail { get; set; }
        public string EntityName { get; set; }
        public string Action { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Changes { get; set; }
    }
}
