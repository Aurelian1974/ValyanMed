using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Enums;

namespace Client.Converters;

/// <summary>
/// Custom JSON converter for TipActIdentitate enum that handles various string formats
/// </summary>
public class TipActIdentitateJsonConverter : JsonConverter<TipActIdentitate?>
{
    public override TipActIdentitate? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.String)
            return null;

        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            return null;

        // Try exact enum parsing first
        if (Enum.TryParse<TipActIdentitate>(value, true, out var result))
            return result;

        // Handle various string formats
        return value.ToUpper().Trim() switch
        {
            "CI" => TipActIdentitate.CI,
            "CARTE" => TipActIdentitate.CI,
            "CARTEIDENTITATE" => TipActIdentitate.CarteIdentitate,
            "CARTE IDENTITATE" => TipActIdentitate.CarteIdentitate,
            "CARTE DE IDENTITATE" => TipActIdentitate.CarteIdentitate,
            
            "PASAPORT" => TipActIdentitate.Pasaport,
            "PASAP" => TipActIdentitate.Pasaport,
            "PASSPORT" => TipActIdentitate.Pasaport,
            
            "PERMIS" => TipActIdentitate.Permis,
            "PERMI" => TipActIdentitate.Permis,
            "PERMISCONDUCERE" => TipActIdentitate.PermisConducere,
            "PERMIS CONDUCERE" => TipActIdentitate.PermisConducere,
            "PERMIS DE CONDUCERE" => TipActIdentitate.PermisConducere,
            
            "CERTIFICAT" => TipActIdentitate.Certificat,
            "CERTI" => TipActIdentitate.Certificat,
            "CERTIFICATNASTERE" => TipActIdentitate.CertificatNastere,
            "CERTIFICAT NASTERE" => TipActIdentitate.CertificatNastere,
            "CERTIFICAT DE NASTERE" => TipActIdentitate.CertificatNastere,
            
            "ALTUL" => TipActIdentitate.Altul,
            
            _ => null // Unknown values return null instead of throwing
        };
    }

    public override void Write(Utf8JsonWriter writer, TipActIdentitate? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}

/// <summary>
/// Custom JSON converter for StareCivila enum that handles various string formats
/// </summary>
public class StareCivilaJsonConverter : JsonConverter<StareCivila?>
{
    public override StareCivila? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.String)
            return null;

        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            return null;

        // Try exact enum parsing first
        if (Enum.TryParse<StareCivila>(value, true, out var result))
            return result;

        // Handle various string formats
        return value.ToUpper().Trim() switch
        {
            "NECASATORIT" => StareCivila.Necasatorit,
            "CELIBATAR" => StareCivila.Necasatorit,
            "NEMARITAT" => StareCivila.Necasatorit,
            
            "CASATORIT" => StareCivila.Casatorit,
            "MARIAJ" => StareCivila.Casatorit,
            "MARITAT" => StareCivila.Casatorit,
            
            "DIVORTIT" => StareCivila.Divortit,
            "DIVORTAT" => StareCivila.Divortit,
            
            "VADUV" => StareCivila.Vaduv,
            "VADOVA" => StareCivila.Vaduv,
            "VADUVE" => StareCivila.Vaduv,
            
            "CONCUBINAJ" => StareCivila.Concubinaj,
            "PARTENERIAT" => StareCivila.Concubinaj,
            "PARTENER" => StareCivila.Concubinaj,
            
            _ => null // Unknown values return null instead of throwing
        };
    }

    public override void Write(Utf8JsonWriter writer, StareCivila? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}

/// <summary>
/// Custom JSON converter for Gen enum that handles various string formats
/// </summary>
public class GenJsonConverter : JsonConverter<Gen?>
{
    public override Gen? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.String)
            return null;

        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            return null;

        // Try exact enum parsing first
        if (Enum.TryParse<Gen>(value, true, out var result))
            return result;

        // Handle various string formats
        return value.ToUpper().Trim() switch
        {
            "M" => Gen.Masculin,
            "MASCULIN" => Gen.Masculin,
            "BARBAT" => Gen.Masculin,
            "MALE" => Gen.Masculin,
            
            "F" => Gen.Feminin,
            "FEMININ" => Gen.Feminin,
            "FEMEIE" => Gen.Feminin,
            "FEMALE" => Gen.Feminin,
            
            "N" => Gen.Neprecizat,
            "NEPRECIZAT" => Gen.Neprecizat,
            
            _ => null // Unknown values return null instead of throwing
        };
    }

    public override void Write(Utf8JsonWriter writer, Gen? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}