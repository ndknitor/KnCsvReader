using System.Reflection;
using System.Text;

namespace KnCsvReader;

public class Csv
{
    public static IEnumerable<IDictionary<string, object>> ReadFile(string filePath, string delimiter = ",")
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            string[] headers = reader.ReadLine().Split(delimiter);
            string str = null;
            while (!string.IsNullOrEmpty(str = reader.ReadLine()))
            {
                IDictionary<string, object> result = new Dictionary<string, object>();
                string[] data = str.Split(delimiter);
                for (int i = 0; i < headers.Length; i++)
                {
                    string key = headers[i], value = data[i];
                    if (long.TryParse(value, out long lv))
                    {
                        if (int.TryParse(value, out int iv))
                        {
                            result[key] = iv;
                        }
                        else
                        {
                            result[key] = lv;
                        }
                    }
                    else if (DateTime.TryParse(value, out DateTime dv))
                    {
                        result[key] = dv;
                    }
                    else if (bool.TryParse(value, out bool bv))
                    {
                        result[key] = bv;
                    }
                    else if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    {
                        value = value.Substring(2);
                        result[key] = GetBytesFromHexString(value);
                    }
                    else
                    {
                        result[key] = value;
                    }
                }
                yield return result;
            }
        }
    }
    public static IEnumerable<T> ReadFile<T>(string filePath, string delimiter = ",")
        where T : new()
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            string[] headers = reader.ReadLine().Split(delimiter);
            string str = null;
            while (!string.IsNullOrEmpty(str = reader.ReadLine()))
            {
                T result = new T();
                string[] data = str.Split(delimiter);
                for (int i = 0; i < headers.Length; i++)
                {
                    string key = headers[i], value = data[i];
                    PropertyInfo property = typeof(T).GetProperty(key);
                    if (property != null)
                    {
                        if (property.PropertyType == typeof(int))
                        {
                            if (int.TryParse(value, out int iv))
                            {
                                property.SetValue(result, iv);
                            }
                        }
                        else if (property.PropertyType == typeof(long))
                        {
                            if (long.TryParse(value, out long lv))
                            {
                                property.SetValue(result, lv);
                            }
                        }
                        else if (property.PropertyType == typeof(DateTime))
                        {
                            if (DateTime.TryParse(value, out DateTime dv))
                            {
                                property.SetValue(result, dv);
                            }
                        }
                        else if (property.PropertyType == typeof(byte[]))
                        {
                            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                            {
                                value = value.Substring(2);
                                property.SetValue(result, GetBytesFromHexString(value));
                            }
                        }
                        else if (property.PropertyType == typeof(bool))
                        {
                            if (bool.TryParse(value, out bool bv))
                            {
                                property.SetValue(result, bv);
                            }
                        }
                        else
                        {
                            property.SetValue(result, value);
                        }
                    }
                }
                yield return result;
            }
        }
    }
    public static int WriteFile(string filePath, IEnumerable<IDictionary<string, object>> values, string delimiter = ",")
    {
        int result = 0;
        if (!values.Any())
        {
            return 0;
        }
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            string headersString = string.Join(delimiter, values.ElementAt(0).Keys);
            writer.WriteLine(headersString);
            foreach (var item in values)
            {
                StringBuilder line = new StringBuilder();
                foreach (var value in item.Values)
                {
                    Type type = value.GetType();
                    if (type == typeof(byte[]))
                    {
                        line.Append($"{GetHexBytesString(value as byte[])}{delimiter}");
                    }
                    else
                    {
                        line.Append($"{value}{delimiter}");
                    }
                }
                line.Remove(line.Length - 1, 1);
                writer.WriteLine(line);
                result++;
            }
        }
        return result;
    }
    public static async Task<int> WriteFileAsync(string filePath, IEnumerable<IDictionary<string, object>> values, string delimiter = ",")
    {
        int result = 0;
        if (!values.Any())
        {
            return 0;
        }
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            string headersString = string.Join(delimiter, values.ElementAt(0).Keys);
            await writer.WriteLineAsync(headersString);
            foreach (var item in values)
            {
                StringBuilder line = new StringBuilder();
                foreach (var value in item.Values)
                {
                    Type type = value.GetType();
                    if (type == typeof(byte[]))
                    {
                        line.Append($"{GetHexBytesString(value as byte[])}{delimiter}");
                    }
                    else
                    {
                        line.Append($"{value}{delimiter}");
                    }
                }
                line.Remove(line.Length - 1, 1);
                await writer.WriteLineAsync(line);
                result++;
            }
        }
        return result;
    }
    public static int WriteFile<T>(string filePath, IEnumerable<T> values, string delimiter = ",")
    {
        int result = 0;
        if (!values.Any())
        {
            return 0;
        }
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            var properties = typeof(T).GetProperties();
            string headersString = string.Join(delimiter, properties.Select(p => p.Name));
            writer.WriteLine(headersString);

            foreach (var item in values)
            {
                StringBuilder line = new StringBuilder();
                foreach (var property in properties)
                {
                    var value = property.GetValue(item);
                    Type type = value?.GetType() ?? typeof(object);

                    if (type == typeof(byte[]))
                    {
                        line.Append($"{GetHexBytesString(value as byte[])}{delimiter}");
                    }
                    else
                    {
                        line.Append($"{value.ToString()}{delimiter}");
                    }
                }
                line.Remove(line.Length - 1, 1);
                writer.WriteLine(line);
                result++;
            }
        }
        return result;
    }
    public static async Task<int> WriteFileAsync<T>(string filePath, IEnumerable<T> values, string delimiter = ",")
    {
        int result = 0;
        if (!values.Any())
        {
            return 0;
        }
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            var properties = typeof(T).GetProperties();
            string headersString = string.Join(delimiter, properties.Select(p => p.Name));
            await writer.WriteLineAsync(headersString);
            foreach (var item in values)
            {
                StringBuilder line = new StringBuilder();
                foreach (var property in properties)
                {
                    var value = property.GetValue(item);
                    Type type = value?.GetType() ?? typeof(object);

                    if (type == typeof(byte[]))
                    {
                        line.Append($"{GetHexBytesString(value as byte[])}{delimiter}");
                    }
                    else
                    {
                        line.Append($"{value}{delimiter}");
                    }
                }
                line.Remove(line.Length - 1, 1);
                await writer.WriteLineAsync(line);
                result++;
            }
        }
        return result;
    }
    private static string GetHexBytesString(byte[] bytes)
    {
        StringBuilder result = new StringBuilder("0x");
        foreach (byte b in bytes)
        {
            result.Append(b.ToString("X2"));
        }
        return result.ToString();
    }
    private static byte[] GetBytesFromHexString(string s) => Enumerable.Range(0, s.Length / 2).Select(i => Convert.ToByte(s.Substring(i * 2, 2), 16)).ToArray();
}
