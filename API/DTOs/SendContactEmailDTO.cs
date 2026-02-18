using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public sealed class SendContactEmailDTO
    {
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(150)]
        public string Subject { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; }

    }
}
