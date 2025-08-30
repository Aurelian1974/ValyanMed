using System.Text.Json;
using System.Text.Json.Serialization;
using Client.Converters;

namespace Client.Services;

public interface IJsonService
{
    JsonSerializerOptions Options { get; }
    T? Deserialize<T>(string json);
    string Serialize<T>(T value);
}

public class JsonService : IJsonService
{
    public JsonSerializerOptions Options { get; }

    public JsonService()
    {
        Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        
        Options.Converters.Add(new JsonStringEnumConverter());
        Options.Converters.Add(new TipActIdentitateJsonConverter());
        Options.Converters.Add(new StareCivilaJsonConverter());
        Options.Converters.Add(new GenJsonConverter());
    }

    public T? Deserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, Options);
    }
}