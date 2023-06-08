namespace JavaJotter.Interfaces;

public interface IScrapper
{
    public Task Scrape(DateTime date);


    public void Scrape()
    {
        Scrape(DateTime.MinValue);
    }
}
