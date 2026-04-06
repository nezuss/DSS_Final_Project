namespace Backend.Models.Controllers {
  public class ErrorModel
  {
    public string Type { get; set; }
    public string Tittle { get; set; }
    public int StatusCode { get; set; }
    public ErrorDetails Errors { get; set; }
  }

  public class ErrorDetails
  {
    public string[] Title { get; set; }
  }
}
