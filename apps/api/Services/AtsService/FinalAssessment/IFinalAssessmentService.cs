using ResumeReview.Api.Dtos.Requests;
using ResumeReview.Api.Dtos.Responses;

namespace ResumeReview.Api.Services.AtsService.FinalAssessment;

public interface IFinalAssessmentService
{
    FinalAssessmentResponse Assess(FinalAssessmentRequest request);
}
