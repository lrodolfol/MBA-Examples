﻿using System.ComponentModel;
using System.Reflection;

namespace Core.Helpers;

public static class Extensions
{
    public static string Serialize<T>(this T objectToSerialize) =>
        System.Text.Json.JsonSerializer.Serialize<T>(objectToSerialize);

    public static T? Deserialize<T>(this string jsonText) => System.Text.Json.JsonSerializer.Deserialize<T>(jsonText);

    public static byte[] ToByteArray(this string text) => System.Text.Encoding.UTF8.GetBytes(text);

    public static string ToUTF8String(this byte[] bytes) => System.Text.Encoding.UTF8.GetString(bytes);

    public static ReadOnlyMemory<byte> ToReadOnlyMemory(this byte[] bytes) => new ReadOnlyMemory<byte>(bytes);
    
    public static string GetEnumDescription<T>(this T enumValue) where T : Enum
    {
        var enumFieldName = enumValue.ToString();
        var field = typeof(T).GetField(enumFieldName);
        if (field is null)
        {
            return "No description";
        }

        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
        return attribute is null ? "No description" : attribute.Description;
    }
}