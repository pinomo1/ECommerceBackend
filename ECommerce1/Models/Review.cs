namespace ECommerce1.Models
{
    public class Review : AItemUser
    {
        public string ReviewText { get; set; }
        public int Quality { get; set; }
        public IList<ReviewPhoto> Photos { get; set; }

        public Review()
        {
            Photos = new List<ReviewPhoto>();
        }
    }
}
