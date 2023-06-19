using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace JavaJotter.Services;

internal class RandomDataHelper
{
    private static readonly Random Random = new();

  
    
    public static string GetRandomUserName()
    {
        var names = new[]
        {
            "LuckyLuke",
            "UnluckyLuke",
            "LukeSkywalker",
            "LukeCage",
            "LukeWarm",
            "LukeDuke",
            "LukePerry",
            "LukeWilson",
            "LukeEvans",
            "LukeHemsworth",
            "LukeBracey",
            "LukeMitchell",
            "LukeArnold",
            "LukeGrimes",
            "LukeBenward",
            "LukeMacfarlane",
            "LukePasqualino",
            "LukeTreadaway",
            "LukeNewberry",
            "LukeMably",
            "LukeYoungblood",
            "LukeGoss",
            "LukeKleintank",
            "LukeBilyk",
            "LukeMitchell",
            "LukeYoungblood",
            "LukeGoss",
            "LukeKleintank",
            "LukeBilyk",
            "LukeMitchell",
        };
        return names[Random.Next(names.Length)];
    }

    public static string GetRandomuserId()
    {
        return EncodeToAlphanumeric(GetRandomUserName());
    }

    public static string EncodeToAlphanumeric(string input)
    {
        // Convert the input string to a byte array
        var byteData = Encoding.UTF8.GetBytes(input);

        // Encode the byte array to a Base64 string
        var base64String = Convert.ToBase64String(byteData);

        // Remove non-alphanumeric characters
        return base64String.Replace('+', 'A').Replace('/', 'B').Replace('=', 'C');
    }

    public static string DecodeFromAlphanumeric(string input)
    {
        // Replace characters back to the original Base64 encoding characters
        input = input.Replace('A', '+').Replace('B', '/').Replace('C', '=');

        // Convert the Base64 string back to a byte array
        var byteData = Convert.FromBase64String(input);

        // Convert the byte array back to the original string
        return Encoding.UTF8.GetString(byteData);
    }

   
    
}