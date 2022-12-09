namespace PixelColorCounter
{
    public class ComboBoxListItem
    {
        public string Text { get; set; }
        public int Id { get; set; }

        public ComboBoxListItem(string text, int id)
        {
            this.Text = text;
            this.Id = id;
        }
    }
}
