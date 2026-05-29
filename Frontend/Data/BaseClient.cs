using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Frontend.Data;


public abstract class BaseClient
{
    protected readonly HttpClient _client;
    protected readonly PopupHub _popupHub;

    protected BaseClient(HttpClient client, PopupHub popupHub)
    {
        _client = client;
        _popupHub = popupHub;
    }

    protected async Task<bool> HandleError(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return false;

        var content = await response.Content.ReadAsStringAsync();

        ErrorResponse? errorObj = null;

        try
        {
            errorObj = JsonSerializer.Deserialize<ErrorResponse>(content);
        }
        catch
        {
            // ignore JSON issues
        }

        var statusCode = (int)response.StatusCode;
        var statusText = response.ReasonPhrase;
        string message = $"{statusCode}: Unknown error";

        if (errorObj?.Errors != null &&
            errorObj.Errors.ContainsKey("Name") &&
            errorObj.Errors["Name"].Length > 0)
        {
            var nameError = errorObj.Errors["Name"][0];

            message = $"{statusCode}: {nameError}";
        }
        else if (statusText is not null)
        {
            message = $"{statusCode}: {statusText}";
        }

        _popupHub.Send(message);

        return true;
    }
}
