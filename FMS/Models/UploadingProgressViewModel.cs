namespace FMS.Models
{
    public class UploadingProgressViewModel
    {
        public int Id { get; set; }
        public float Percent { get; set; }
        public UploadingProgressViewModel(int id, float percent)
        {
            Id = id;
            Percent = percent;
        }
    }
}
