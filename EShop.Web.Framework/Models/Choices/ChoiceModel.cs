namespace EShop.Web.Common.Models.Choices;

public abstract class ChoiceModel : BaseModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string TextPrompt { get; set; }
    
    public string Alias { get; set; }
    public bool IsPreSelected { get; set; }
    public bool IdDisabled { get; set; }
    public bool IsUnavailable{ get; set; }
    public bool IsActive { get; set; }
   
}