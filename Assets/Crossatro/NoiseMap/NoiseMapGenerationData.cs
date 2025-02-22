using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Crossatro.Data
{
    [CreateAssetMenu(fileName = "NoiseMapData", menuName = "Crossatro/NoiseMapData", order = 0)]
    public class NoiseMapGenerationData : ScriptableObject
    {
        [field: SerializeField]
        public List<NoiseValueData> NoiseValueDatas { get; private set; } = new List<NoiseValueData>();

        [field: SerializeField] 
        public float ZoomValue { get; private set; } = 1.0f;
        
        private void OnValidate()
        {
            NoiseValueDatas = NoiseValueDatas.OrderBy(data => data.NoiseValue).ToList();
        }
        
        [Serializable]
        public struct NoiseValueData
        {
            [Range(0,1)]
            public float NoiseValue;
            public Material AssociatedMaterial;
        }
    }
}