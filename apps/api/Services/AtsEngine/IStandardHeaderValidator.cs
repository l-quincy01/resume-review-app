using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Services.AtsEngine;

public interface IStandardHeaderValidator
{
    HeaderValidationResponse Validate(string resumeText);
}
