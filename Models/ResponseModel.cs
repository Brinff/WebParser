namespace WebParser.Models;

public class ResponseModel
{
    public int IsError { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }

    public int ElementsCount { get; set; }
    public int EmailsCount { get; set; }

    public string Url { get; set; }
    public string DecryptedPlainText { get; set; }

    public List<string> ElementsAttrList { get; set; } = new();
    public List<string> EmailsList { get; set; } = new();
}