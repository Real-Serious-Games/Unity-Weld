using UnityEngine;

namespace UnityWeld.Binding.Adapters
{
    [CreateAssetMenu(menuName = "Adapter options/Float to string adapter")]
    public class FloatToStringAdapterOptions : AdapterOptions
    {
        public string Format = "0.00";
    }
}