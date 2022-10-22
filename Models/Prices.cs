namespace Mongo.WebUI.Models
{
    internal enum Prices : int
    {
        First = 100,
        Second = 50,
        Third = 20
    }

    public class PriceMapper
    {
        public static int MapPrice(int seatClass)
        {
            return seatClass switch
            {
                1 => (int)Prices.First,
                2 => (int)Prices.Second,
                3 => (int)Prices.Third,
                _ => throw new Exception("Error: such seat class does not exist."),
            };
        }
    }

}
