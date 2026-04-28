using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ResumeReview.Api.Dtos.Requests;

public class ResumeReviewRequest
{
    [Required]
    public string AiModel { get; set; } = string.Empty;

    [Required]
    public IFormFile Resume { get; set; } = default!;

    public string? JobDescription { get; set; }
}