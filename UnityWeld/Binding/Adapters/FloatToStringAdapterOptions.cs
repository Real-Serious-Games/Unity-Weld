using UnityEngine;

namespace UnityWeld.Binding.Adapters
{
    [CreateAssetMenu(menuName = "Unity Weld/Adapter options/Float to string adapter")]
    public class FloatToStringAdapterOptions : AdapterOptions
    {
        public string Format = "0.00";
    }
}