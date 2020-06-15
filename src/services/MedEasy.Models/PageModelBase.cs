namespace MedEasy.Models
{
    public abstract class PageModelBase
    {
        public virtual long Total { get; set; }

        public virtual long Count { get;  }
    }
}