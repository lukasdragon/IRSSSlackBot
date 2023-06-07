using JavaJotter.Interfaces;
using SlackNet;
namespace JavaJotter.Services;

public class ScrappingService : IScrapper
{
    private ISlackApiClient _slackClient;

    public ScrappingService(ISlackServiceProvider slackServiceProvider)
    {
        _slackClient = slackServiceProvider.GetApiClient();
    }




    public void Scrape(DateTime date)
    {
        throw new NotImplementedException();
    }
}
