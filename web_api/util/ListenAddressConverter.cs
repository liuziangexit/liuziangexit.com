using System;
using System.Collections.Generic;
using Newtonsoft.Json;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    2/4/2019
 * 
 * ListenAddressConverter
 * 
 */

public class ListenAddressConverter : JsonConverter<GameDbCache.ListenAddress>
{
    public override GameDbCache.ListenAddress ReadJson(JsonReader reader, Type objectType, GameDbCache.ListenAddress existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var result = new GameDbCache.ListenAddress();
        if (reader.TokenType != JsonToken.Null)
        {
            string tmp = reader.Value as string;
            if (tmp != null)
            {
                var columns = tmp.Split(':');
                if (columns.Length == 2)
                {
                    UInt16 parseResult = 0;
                    if (UInt16.TryParse(columns[1].Trim(), out parseResult))
                    {
                        result.ip = columns[0].Trim();
                        result.port = parseResult;
                    }
                }
            }
        }
        return result;
    }

    public override void WriteJson(JsonWriter writer, GameDbCache.ListenAddress value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}