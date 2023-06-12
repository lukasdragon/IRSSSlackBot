using System.Text.RegularExpressions;
using SlackNet.Events;

namespace JavaJotter;

public partial class Filter
{
    public bool ExtractRoll(MessageEvent messageEvent, out Roll? roll)
    {
        var message = messageEvent.Text;

        if (RollFormat().IsMatch(message))
        {
            var taggedUser = TaggedUserRegex().Match(message).Groups[1].Value;
            var rolls = ExtractRollValue().Match(message).Groups[1].Value;


            if (int.TryParse(rolls, out var rollValue))
            {
                roll = new Roll(taggedUser, messageEvent.Timestamp, rollValue);
                return true;
            }
        }
      
        roll = null;
        return false;
    }

    [GeneratedRegex("<@[A-Za-z0-9]+>\\s+rolled\\s+\\*\\d+\\*")]
    private static partial Regex RollFormat();

    [GeneratedRegex("<@([A-Za-z0-9]+)>")]
    private static partial Regex TaggedUserRegex();

    [GeneratedRegex("\\*(\\d+)\\*")]
    private static partial Regex ExtractRollValue();
}