using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crossatro.Grid
{
    public class WorldGeneration : MonoBehaviour
    {
        [SerializeField] 
        private EnvironementGenerator m_environementGenerator;

        [SerializeField] 
        private CrossWordGridGenerator m_crossWordGridGenerator;

        public Board board;


        private void Start()
        {
            //StartCoroutine(BaseWorldGenerationRoutine());
        }

        private IEnumerator BaseWorldGenerationRoutine()
        {
            yield return new WaitUntil(() => m_environementGenerator.IsStarted);
            
            m_environementGenerator.GenerateBase();
            yield return new WaitUntil(() => m_crossWordGridGenerator);
            board.ResetDoTweenDelay();
            yield return new WaitForSeconds(2f);
            m_crossWordGridGenerator.GenerateBase();
        }
    }
}