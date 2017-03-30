
namespace MedEasy.RestObjects
{
    public class GenericGetResponse<T> : IGetResponse<T>
    {
        public GenericGetResponse(T item, string href)
        {
            Item = item;
            Href = href;
        }

        public T Item { get;  }
        public string Href { get; }
    }
}