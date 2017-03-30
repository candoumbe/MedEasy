namespace MedEasy.DTO.Autocomplete
{
    public abstract class AutocompleteInfo<TValue>
    {
        public TValue Value { get; set; }

        public string Text { get; set; }

    }
}
