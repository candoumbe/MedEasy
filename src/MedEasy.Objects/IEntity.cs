namespace MedEasy.Objects
{

    /// <summary>
    /// Interface that defines the identity
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IEntity<TKey>
    {
        TKey Id { get; set; }
    }
}