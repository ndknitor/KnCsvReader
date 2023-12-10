using KnCsvReader;
using Newtonsoft.Json;

Console.WriteLine(JsonConvert.SerializeObject(Csv.ReadFile<User>("User.csv")));
// foreach (var item in users)
// {
//     Console.WriteLine(JsonConvert.SerializeObject(item));
//     Console.WriteLine();
//     // foreach (string key in item.Keys)
//     // {
//     //     Console.WriteLine($"Keys: {key}; Value: {item[key]}");
//     // }
//     // Console.WriteLine();
// }

public class User
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string Fullname { get; set; }
    public int Phone { get; set; }
    public string Address { get; set; }
    public int RoleId { get; set; }
}