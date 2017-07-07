namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Adapter that inverts the value of the bound boolean property.
    /// </summary>
    [Adapter(typeof(bool), typeof(bool))]
    public class BoolInversionAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            return !(bool)valueIn;
        }
    }
}
