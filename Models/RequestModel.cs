namespace WebParser.Models;
using FluentValidation;
using System.Text.Json.Serialization;

public class RequestModel
{
    [JsonPropertyName("selector")]
    public string Selector { get; set; }

    [JsonPropertyName("attribute")]
    public string Attribute { get; set; }

    [JsonPropertyName("url_b64")]
    public string UrlB64 { get; set; }

    [JsonPropertyName("encrypted_text_bytes_b64")]
    public string EncryptedTextBytesB64 { get; set; }

    [JsonPropertyName("key_bytes_b64")]
    public string KeyBytesB64 { get; set; }

    [JsonPropertyName("page_b64")]
    public string PageB64 { get; set; }
}

public class ProcessRequestValidator : AbstractValidator<RequestModel>
{
    public ProcessRequestValidator()
    {
        RuleFor(x => x.Selector)
            .NotEmpty().WithMessage("Selector is required and cannot be empty");

        RuleFor(x => x.Attribute)
            .NotEmpty().WithMessage("Attribute is required and cannot be empty");

        RuleFor(x => x.UrlB64)
            .NotEmpty().WithMessage("url_b64 is required");

        RuleFor(x => x.PageB64)
            .NotEmpty().WithMessage("page_b64 is required");

        RuleFor(x => x.EncryptedTextBytesB64)
            .NotEmpty().WithMessage("encrypted_text_bytes_b64 is required");

        RuleFor(x => x.KeyBytesB64)
            .NotEmpty().WithMessage("key_bytes_b64 is required");
    }
}