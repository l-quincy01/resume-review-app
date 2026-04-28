namespace ResumeReview.Api.Models ;

public class AtsContent
{
    public AtsHeading Heading { get; set; } = new();
    public string ResumeName { get; set; } = string.Empty;
    public List<AtsSectionContent> Content { get; set; } = [];
}