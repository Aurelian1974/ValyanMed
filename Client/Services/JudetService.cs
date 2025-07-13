using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Shared.DTOs;

public class JudetService
{
    private readonly HttpClient _http;
    public JudetService(HttpClient http) => _http = http;

    public async Task<List<JudetDto>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<JudetDto>>("api/judet");
}