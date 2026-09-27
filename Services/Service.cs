using FluentValidation;
using WebParser.Models;

namespace WebParser.Services;

public class Service
{
    private readonly IValidator<RequestModel> _validator;

    public Service(IValidator<RequestModel> validator)
    {
        _validator = validator;
    }

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

        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid)
        {
            response.IsError = 1;
            response.ErrorCode = "VALIDATION_ERROR";
            response.ErrorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));

            return Task.FromResult(response);
        }

        return Task.FromResult(response);
    }
}