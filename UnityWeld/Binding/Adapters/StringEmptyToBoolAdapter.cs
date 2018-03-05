namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// String to bool adapter that returns false if the string is null or empty, 
    /// otherwise true.
    /// </summary>
    [Adapter(typeof(string), typeof(bool))]
    public class StringEmptyToBoolAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            return !string.IsNullOrEmpty((string)valueIn);
        }
    }
}
