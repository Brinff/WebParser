using WebParser.Models;

namespace WebParser.Services;

public class Service
{
    public Task<ResponseModel> ProcessAsync(RequestModel request)
    {
        var response = new ResponseModel
        {
            IsError = 0,
            ErrorCode = null,
            ErrorMessage = null,
            ElementsCount = 0,
            EmailsCount = 0,
            Url = null,
            DecryptedPlainText = null,
            ElementsAttrList = new List<string>(),
            EmailsList = new List<string>()
        };

        return Task.FromResult(response);
    }
}