namespace UserAuthenticationAPI.DTOs.ResultModel;
public class ResultModel
{
    public bool IsSuccess { get; set; }
    public int Code { get; set; }
    public object? Data { get; set; }
    public object? ResponseFailed { get; set; }
    public string? Message { get; set; }
}