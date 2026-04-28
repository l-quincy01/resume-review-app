
namespace ResumeReview.Api.Services.Schemas;

public static class JobRecommendationSchema
{
    public static object Schema => new
    {
        type = "object",
        additionalProperties = false,
        properties = new
        {
            yearsExperience = new { type = "string" },
            jobTitles = new { type = "array", items = new { type = "string" } },
            responsibilities = new { type = "array", items = new { type = "string" } },
            seniority = new { type = "array", items = new { type = "string" } },
            industry = new { type = "array", items = new { type = "string" } },
            hardSkills = new { type = "array", items = new { type = "string" } },
            softSkills = new { type = "array", items = new { type = "string" } },
            companySizeFit = new { type = "array", items = new { type = "string" } },
            careerTrack = new { type = "array", items = new { type = "string" } }
        },
        required = new[]
             {
                "yearsExperience",
                "jobTitles",
                "responsibilities",
                "seniority",
                "industry",
                "hardSkills",
                "softSkills",
                "companySizeFit",
                "careerTrack"
            }
    };

}