using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Instagram Follower Tracker");

        // Coletar o nome de usuário
        Console.Write("Enter your Instagram username: ");
        string? username = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(username))
        {
            Console.WriteLine("Username não pode ser vazio.");
            return;
        }

        // Coletar a senha
        Console.Write("Enter your Instagram password: ");
        string? password = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("Password não pode ser vazio.");
            return;
        }

        try
        {
            var scraper = new InstagramScraper(username, password);
            var followers = await scraper.GetFollowers();
            scraper.SaveFollowers(followers);
            Console.WriteLine("Seguidores salvos com sucesso!");

            // Opcional: Carregar seguidores anteriores e comparar
            /*
            var oldFollowers = scraper.LoadFollowers("followers_2023-10-10.json"); // Exemplo de data
            var newFollowers = followers;
            var lostFollowers = scraper.CompareFollowers(oldFollowers, newFollowers);
            Console.WriteLine("Seguidores perdidos:");
            foreach (var follower in lostFollowers)
            {
                Console.WriteLine(follower);
            }
            */
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
