namespace JavaJotter.Interfaces;

public interface IScrapper
{
    public void Scrape(DateTime date);


    public void Scrape()
    {
        Scrape(DateTime.MinValue);
    }
}
