using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;

public class InstagramScraper
{
    private readonly HttpClient _httpClient;
    private readonly string _username;
    private readonly string _password;

    public InstagramScraper(string username, string password)
    {
        var handler = new HttpClientHandler
        {
            CookieContainer = new CookieContainer(),
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        _httpClient = new HttpClient(handler);
        _username = username;
        _password = password;

        // Configurar headers padrão
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        _httpClient.DefaultRequestHeaders.Add("Referer", "https://www.instagram.com/accounts/login/");
    }

    public async Task<List<string>> GetFollowers()
    {
        await Login();
        var userId = await GetUserId();
        return await GetFollowersList(userId);
    }

    public async Task Login()
    {
        var loginUrl = "https://www.instagram.com/accounts/login/ajax/";

        // Obter a página inicial para capturar os cookies e o token CSRF
        var initialResponse = await _httpClient.GetAsync("https://www.instagram.com/");
        var initialContent = await initialResponse.Content.ReadAsStringAsync();

        // Extrair o token CSRF
        var csrfMatch = Regex.Match(initialContent, "\"csrf_token\":\"(.*?)\"");
        if (!csrfMatch.Success)
        {
            throw new Exception("Token CSRF não encontrado.");
        }
        var csrfToken = csrfMatch.Groups[1].Value;

        // Atualizar os headers necessários
        _httpClient.DefaultRequestHeaders.Remove("X-CSRFToken");
        _httpClient.DefaultRequestHeaders.Add("X-CSRFToken", csrfToken);
        _httpClient.DefaultRequestHeaders.Remove("X-Requested-With");
        _httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        _httpClient.DefaultRequestHeaders.Remove("X-IG-App-ID");
        _httpClient.DefaultRequestHeaders.Add("X-IG-App-ID", "936619743392459");

        // Preparar o valor da senha criptografada
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var encPassword = $"#PWD_INSTAGRAM_BROWSER:0:{timestamp}:{_password}";

        // Preparar o conteúdo da requisição de login
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("username", _username),
            new KeyValuePair<string, string>("enc_password", encPassword),
            new KeyValuePair<string, string>("queryParams", "{}"),
            new KeyValuePair<string, string>("optIntoOneTap", "false"),
        });

        // Enviar a requisição de login
        var response = await _httpClient.PostAsync(loginUrl, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Logs para depuração
        Console.WriteLine($"Status Code: {response.StatusCode}");
        Console.WriteLine($"Response Content: {responseContent}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Login falhou: {response.StatusCode} - {responseContent}");
        }

        // Analisar a resposta JSON
        var jsonResponse = JObject.Parse(responseContent);

        if (jsonResponse["authenticity_token"] != null && jsonResponse["authenticated"]?.Value<bool>() == false)
        {
            throw new Exception("Login falhou: Usuário ou senha inválidos.");
        }

        if (jsonResponse["two_factor_required"] != null && jsonResponse["two_factor_required"]?.Value<bool>() == true)
        {
            // 2FA é necessário
            Console.Write("Insira o código de autenticação de dois fatores: ");
            string? twoFactorCode = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(twoFactorCode))
            {
                throw new Exception("Código de autenticação de dois fatores não fornecido.");
            }

            await SubmitTwoFactorCode(twoFactorCode, jsonResponse["two_factor_info"]);
        }

        if (jsonResponse["authenticated"] != null && jsonResponse["authenticated"]?.Value<bool>() == true)
        {
            Console.WriteLine("Login realizado com sucesso!");
            return;
        }

        throw new Exception($"Login falhou: Resposta inesperada - {responseContent}");
    }

    private async Task SubmitTwoFactorCode(string twoFactorCode, JToken? twoFactorInfo)
    {
        var twoFactorUrl = "https://www.instagram.com/accounts/login/ajax/two_factor/";

        string? verificationMethod = twoFactorInfo?["verification_method"]?.ToString();
        string? obfuscatedPhoneNumber = twoFactorInfo?["obfuscated_phone_number"]?.ToString();

        if (string.IsNullOrEmpty(verificationMethod))
        {
            throw new Exception("Método de verificação de 2FA não disponível.");
        }

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("username", _username),
            new KeyValuePair<string, string>("enc_password", $"#PWD_INSTAGRAM_BROWSER:0:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}:{_password}"),
            new KeyValuePair<string, string>("queryParams", "{}"),
            new KeyValuePair<string, string>("optIntoOneTap", "false"),
            new KeyValuePair<string, string>("verificationCode", twoFactorCode),
        });

        var response = await _httpClient.PostAsync(twoFactorUrl, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"2FA Status Code: {response.StatusCode}");
        Console.WriteLine($"2FA Response Content: {responseContent}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"2FA falhou: {response.StatusCode} - {responseContent}");
        }

        var jsonResponse = JObject.Parse(responseContent);

        if (jsonResponse["authenticated"] != null && jsonResponse["authenticated"]?.Value<bool>() == true)
        {
            Console.WriteLine("Autenticação de dois fatores concluída com sucesso!");
            return;
        }

        if (jsonResponse["two_factor_required"] != null && jsonResponse["two_factor_required"]?.Value<bool>() == true)
        {
            throw new Exception("Autenticação de dois fatores falhou: Código inválido.");
        }

        throw new Exception($"2FA falhou: Resposta inesperada - {responseContent}");
    }

    private async Task<string> GetUserId()
    {
        var response = await _httpClient.GetAsync($"https://www.instagram.com/{_username}/");
        var content = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(content, @"""profilePage_(\d+)""");
        if (!match.Success)
        {
            throw new Exception("ID do usuário não encontrado.");
        }
        return match.Groups[1].Value!;
    }

    private async Task<List<string>> GetFollowersList(string userId)
    {
        var followers = new List<string>();
        var hasNextPage = true;
        var endCursor = "";

        while (hasNextPage)
        {
            var variables = JsonConvert.SerializeObject(new
            {
                id = userId,
                include_reel = true,
                fetch_mutual = false,
                first = 50,
                after = endCursor
            });

            var url = $"https://www.instagram.com/graphql/query/?query_hash=c76146de99bb02f6415203be841dd25a&variables={Uri.EscapeDataString(variables)}";
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(content);

            if (data?.data?.user?.edge_followed_by == null)
            {
                throw new Exception("Falha ao buscar seguidores. A estrutura da resposta mudou ou a requisição foi bloqueada.");
            }

            foreach (var edge in data.data.user.edge_followed_by.edges)
            {
                if (edge?.node?.username != null)
                {
                    followers.Add((string)edge.node.username);
                }
            }

            hasNextPage = data.data.user.edge_followed_by.page_info.has_next_page;
            endCursor = data.data.user.edge_followed_by.page_info.end_cursor;

            await Task.Delay(2000);
        }

        return followers;
    }

    public void SaveFollowers(List<string> followers)
    {
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        File.WriteAllText($"followers_{date}.json", JsonConvert.SerializeObject(followers, Formatting.Indented));
    }

    public List<string> LoadFollowers(string filename)
    {
        if (File.Exists(filename))
        {
            var json = File.ReadAllText(filename);
            return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
        }
        return new List<string>();
    }

    public List<string> CompareFollowers(List<string> oldFollowers, List<string> newFollowers)
    {
        return oldFollowers.Except(newFollowers).ToList();
    }
}