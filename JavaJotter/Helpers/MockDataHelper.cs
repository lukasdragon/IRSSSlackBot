using System.Collections.Immutable;
using System.Text;

namespace JavaJotter.Helpers;

internal static class MockDataHelper
{
    private static readonly Random Random = new();

    static MockDataHelper()
    {
        var ids = new string[Usernames.Length];
        for (var i = 0; i < Usernames.Length; i++)
        {
            ids[i] = EncodeToAlphanumeric(Usernames[i]);
        }

        UserIds = ids.ToImmutableArray();
    }

    private static readonly ImmutableArray<string> Usernames = ImmutableArray.Create<string>(
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
        "LukeMitchell"
    );

    private static readonly ImmutableArray<string> UserIds;


    public static ImmutableArray<string> GetUsernames()
    {
        return Usernames;
    }

    public static ImmutableArray<string> GetUserIds()
    {
        return UserIds;
    }


    public static string GetRandomUserName()
    {
        return Usernames[Random.Next(Usernames.Length)];
    }

    public static string GetRandomUserId()
    {
        return UserIds[Random.Next(UserIds.Length)];
    }

    private static string EncodeToAlphanumeric(string input)
    {
        // Convert the input string to a byte array
        var byteData = Encoding.UTF8.GetBytes(input);

        // Encode the byte array to a Base64 string
        var base64String = Convert.ToBase64String(byteData);

        // Remove non-alphanumeric characters
        return base64String.Replace('+', 'A').Replace('/', 'B').Replace('=', 'C');
    }

    private static string DecodeFromAlphanumeric(string input)
    {
        // Replace characters back to the original Base64 encoding characters
        input = input.Replace('A', '+').Replace('B', '/').Replace('C', '=');

        // Convert the Base64 string back to a byte array
        var byteData = Convert.FromBase64String(input);

        // Convert the byte array back to the original string
        return Encoding.UTF8.GetString(byteData);
    }
}