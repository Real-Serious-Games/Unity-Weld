namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Adapter that parses a string as a float.
    /// </summary>
    [Adapter(typeof(string), typeof(float))]
    public class StringToFloatAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            return float.Parse((string)valueIn);
        }
    }
}
