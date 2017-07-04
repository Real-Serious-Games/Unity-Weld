namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Adapter for converting from an int to a string.
    /// </summary>
    [Adapter(typeof(int), typeof(string))]
    public class IntToStringAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            return ((int)valueIn).ToString();
        }
    }
}
