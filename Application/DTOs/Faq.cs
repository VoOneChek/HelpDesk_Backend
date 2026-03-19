
namespace Application.DTOs.Faq
{
    public class FaqDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
    }

    public class CreateFaqDto
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
    }
}
