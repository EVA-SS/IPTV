namespace script
{
    public static class Json
    {
        public static string? ToJson(this object? obj)
        {
            if (obj == null) return null;
            return obj.ToJsonCore();
        }
        public static string ToJsonCore(this object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }
        public static T? ToJson<T>(this string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
                }
                catch
                {
                }
            }
            return default;
        }
    }
}
