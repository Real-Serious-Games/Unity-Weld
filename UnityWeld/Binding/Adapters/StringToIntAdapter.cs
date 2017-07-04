namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Adapter for converting from a string to an int.
    /// </summary>
    [Adapter(typeof(string), typeof(int))]
    public class StringToIntAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            return int.Parse((string)valueIn);
        }
    }
}
