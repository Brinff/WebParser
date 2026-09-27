using System.Text;
using FluentValidation;
using WebParser.Models;
using System.Security.Cryptography;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Configuration;
using Dapper;
using Npgsql;
using System.Linq;

namespace WebParser.Services;

public class Service
{
    private readonly IValidator<RequestModel> _validator;
    private readonly string _connectionString;

    public Service(IValidator<RequestModel> validator, IConfiguration configuration)
    {
        _validator = validator;
        _connectionString = configuration.GetConnectionString("Postgres") 
                            ?? throw new InvalidOperationException("Connection string 'Postgres' not found");
    }

    public async Task<ResponseModel> ProcessAsync(RequestModel request)
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

        try
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                response.IsError = 1;
                response.ErrorCode = "VALIDATION_ERROR";
                response.ErrorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return response;
            }

            string decodedUrl;
            try
            {
                var urlBytes = Convert.FromBase64String(request.UrlB64);
                decodedUrl = Encoding.UTF8.GetString(urlBytes);
            }
            catch (FormatException)
            {
                response.IsError = 1;
                response.ErrorCode = "URL_BASE64_DECODE_ERROR";
                return response;
            }

            string decodedPage;
            try
            {
                var pageBytes = Convert.FromBase64String(request.PageB64);
                decodedPage = Encoding.UTF8.GetString(pageBytes);
            }
            catch (FormatException)
            {
                response.IsError = 1;
                response.ErrorCode = "PAGE_BASE64_DECODE_ERROR";
                return response;
            }

            response.Url = decodedUrl;

            byte[] keyBytes;
            byte[] encryptedBytes;
            try
            {
                keyBytes = Convert.FromBase64String(request.KeyBytesB64);
                encryptedBytes = Convert.FromBase64String(request.EncryptedTextBytesB64);
            }
            catch (FormatException)
            {
                response.IsError = 1;
                response.ErrorCode = "AES_BASE64_DECODE_ERROR";
                return response;
            }

            string decryptedText;
            try
            {
                using var aes = Aes.Create();
                aes.Key = keyBytes;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.None;

                using var decryptor = aes.CreateDecryptor();
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                decryptedText = Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (CryptographicException)
            {
                response.IsError = 1;
                response.ErrorCode = "DECRYPTION_ERROR";
                return response;
            }

            response.DecryptedPlainText = decryptedText;

            var parser = new HtmlParser();
            var document = parser.ParseDocument(decodedPage);

            var elements = document.QuerySelectorAll(request.Selector);

            var elementsAttrList = new List<string>();
            var elementsHtmlList = new List<string>();

            foreach (var element in elements)
            {
                var attrValue = element.GetAttribute(request.Attribute);
                elementsAttrList.Add(attrValue ?? string.Empty);
                elementsHtmlList.Add(element.OuterHtml);
            }

            response.ElementsCount = elementsAttrList.Count;
            response.ElementsAttrList = elementsAttrList;

            var emailsList = EmailExtractor.ExtractEmails(decodedPage);
            response.EmailsCount = emailsList.Count;
            response.EmailsList = emailsList;

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await connection.ExecuteAsync(@"
                    CREATE TABLE IF NOT EXISTS elements (
                        id BIGSERIAL PRIMARY KEY,
                        attribute_value TEXT,
                        html_content TEXT
                    );");

                var recordsToInsert = elementsAttrList
                    .Zip(elementsHtmlList, (attrValue, html) => new { AttributeValue = attrValue, HtmlContent = html })
                    .ToList();

                await connection.ExecuteAsync(
                    "INSERT INTO elements (attribute_value, html_content) VALUES (@AttributeValue, @HtmlContent)",
                    recordsToInsert);
            }
            catch (NpgsqlException)
            {
                response.IsError = 1;
                response.ErrorCode = "DATABASE_ERROR";
                return response;
            }

            return response;
        }
        catch (Exception ex)
        {
            response.IsError = 1;
            response.ErrorCode = "UNKNOWN_ERROR";
            response.ErrorMessage = ex.Message;
            return response;
        }
    }
}