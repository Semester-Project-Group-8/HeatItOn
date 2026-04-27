using Frontend.Data.CSV;
using Frontend.Data;

namespace FrontendTest;

public class CsvTest
{
    private readonly HttpClient _client = 
        new HttpClient() { 
            BaseAddress = new Uri("http://localhost:8080/") 
        };
}