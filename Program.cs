using Microsoft.Data.Sqlite;
using System;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using TopGGPrune.GeneratedModels;

public class Program
{

    public static string BaseUrl { get; set; } = "https://top.gg/api/client/entities/search";

    public static void Main(string[] args)
    {
        InitDB();
        string query = "";
        if(args.Length > 0)
        {
            query = args[0];
            Console.WriteLine("Querying for '" + query + "'");
        }
        for(int i = 0; i < 100; i++)
        {
            Console.WriteLine("Getting Page " + i);
            var page = GetPage(i, query).Result;
            if(page == null)
            {
                Console.Error.WriteLine("Couldn't load page " + page);
                continue;
            }
            if(page.count == 0)
            {
                return;
            }
            foreach(var result in page.results)
            {
                CheckValidity(result).Wait();
            }
        }
        InitDB();
    }

    public static void InitDB()
    {
        using(var connection = new SqliteConnection("Data Source=Prune.db"))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE IF NOT EXISTS Servers(id string primary key, name string, inviteValid bool)";
            command.ExecuteNonQuery();
        }
    }

    public static async Task<SearchResults> GetPage(int page, string query)
    {
        using(var client = new HttpClient())
        {
            var res = await client.GetAsync(new Uri(BaseUrl + $"?platform=discord&tag=&skip={page * 10}&amount=10&query={query}&reviewScore=&entityType=server"));
            if(res.StatusCode != HttpStatusCode.OK)
            {
                Console.Error.WriteLine(res.StatusCode.ToString());
                return null;
            }
            var json = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json))
            {
                Console.Error.WriteLine("Empty response!");
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<SearchResults>(json);
            } catch(Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return null;
            }
        }
    }

    public static async Task CheckValidity(Result result)
    {
        var clientHandler = new HttpClientHandler();
        clientHandler.AllowAutoRedirect = false;
        using (var client = new HttpClient(clientHandler))
        {
            var res = await client.GetAsync($"https://top.gg/servers/{result.id}/join");
            if (res.StatusCode != HttpStatusCode.TemporaryRedirect)
            {
                Console.Error.WriteLine(result.id + " Failed -> HTTP " + res.StatusCode.ToString());
                return;
            }

            var location = res.Headers.Where(header => header.Key.ToLower() == "location").Select(kvp => string.Join(", ", kvp.Value)).FirstOrDefault();
            if(string.IsNullOrEmpty(location))
            {
                Console.Error.WriteLine(result.id + " Failed -> Could not find canonical URL");
                File.AppendAllText("error_log.txt", "===== " + result.id + "\n" +string.Join("\n", res.Headers.Select(kvp => kvp.Key + ": " + kvp.Value)) + "\n");
                return;
            }
            var url = location.Replace("https://discord.gg/", "https://discord.com/api/invite/");
            var validate = await client.GetAsync(url);

            if(validate.StatusCode == HttpStatusCode.TooManyRequests)
            {
                Console.Error.WriteLine(result.id + " Failed -> Rate limited");
                return;
            }

            bool valid = validate.StatusCode != HttpStatusCode.NotFound;

            using (var connection = new SqliteConnection("Data Source=Prune.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT OR IGNORE INTO Servers VALUES($id, $name, $valid)";
                command.Parameters.AddWithValue("$id", result.id);
                command.Parameters.AddWithValue("$name", result.name);
                command.Parameters.AddWithValue("$valid", valid);

                var code = command.ExecuteNonQuery();
                Console.WriteLine(result.id + $" OK -> {(valid ? "valid" : "invalid")}");
            }
        }
    }
}
