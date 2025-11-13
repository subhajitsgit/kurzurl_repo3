namespace KurzUrl.Repository.Models
{
  public class UpdateLinkRequest
  {
    public int Id { get; set; }
    public string MainUrl { get; set; }
    public string Title { get; set; }
  }
}