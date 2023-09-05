namespace ECommerce1.Models
{
    public class QuestionProduct : AItemUser
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public bool IsAnswered { get; set; }
    }
}
