using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

public class QuaternionConverter : JsonConverter<Quaternion>
{
    public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
    {
        JObject obj = new JObject
        {
            ["x"] = value.x,
            ["y"] = value.y,
            ["z"] = value.z,
            ["w"] = value.w
        };
        obj.WriteTo(writer);
    }

    public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        return new Quaternion(
            (float)obj["x"],
            (float)obj["y"],
            (float)obj["z"],
            (float)obj["w"]
        );
    }
}