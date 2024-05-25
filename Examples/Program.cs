using VRClient.Abstractions;

namespace VRClient
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            IVRClient client = new Impls.VRClient();
            await client.AuthAsync("user", "user");
            var userInfo = await client.GetMeAsync();
            Console.WriteLine(userInfo);
            Console.ReadLine();
        }
    }
}
