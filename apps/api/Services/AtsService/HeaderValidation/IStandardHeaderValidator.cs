using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Services.AtsService.HeaderValidation;

public interface IStandardHeaderValidator
{
    HeaderValidationResponse Validate(string resumeText);
}
