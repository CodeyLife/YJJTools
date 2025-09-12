using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class Vector3Converter : JsonConverter<Vector3>
{
    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
    {
        JObject obj = new JObject
        {
            ["x"] = value.x,
            ["y"] = value.y,
            ["z"] = value.z
        };
        obj.WriteTo(writer);
    }

    public override Vector3 ReadJson(JsonReader reader, Type objectType, [AllowNull] Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        return new Vector3(
            (float)obj["x"],
            (float)obj["y"],
            (float)obj["z"]
        );
    }

}