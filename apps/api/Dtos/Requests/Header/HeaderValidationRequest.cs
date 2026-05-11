using Microsoft.AspNetCore.Mvc;

namespace ResumeReview.Api.Dtos.Requests;

public sealed class HeaderValidationRequest
{
    [FromForm(Name = "resume")]
    public IFormFile Resume { get; set; } = default!;
}
