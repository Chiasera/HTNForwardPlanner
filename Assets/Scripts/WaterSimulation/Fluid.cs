using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Fluid
{
    [AddComponentMenu("FusionWater/Fluid")]
    public class Fluid : MonoBehaviour
    {       
        private float waveAmplitude;
        private float waveLength;
        private float waveSteepness;
        private float waveSpeed;
        private Vector2 waveDirection;
        private new Renderer renderer;
        private float waterHeight;

        private GerstnerWave[] waves;

        public float density = 1;

        public float drag = 1;

        public float angularDrag = 1f;

        public Collider coll { get; private set; }

        private void Start()
        {
            renderer = GetComponent<Renderer>();           
        }

        private void Update()
        {
            waveAmplitude = renderer.material.GetFloat("_WaveAmplitude");
            waveLength = renderer.material.GetFloat("_WaveLength");
            waveSteepness = renderer.material.GetFloat("_WaveSteepness");
            waveSpeed = renderer.material.GetFloat("_WaveSpeed");
            waveDirection = renderer.material.GetVector("_WaveDirection");
            coll = GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out BaseFluidInteractor fluidInteractor))
            {
                fluidInteractor.EnterFluid(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out BaseFluidInteractor fluidInteractor))
            {
                fluidInteractor.ExitFluid(this);
            }
        }

        public Vector3 CircularWave(Vector2 waveCenter, Vector2 vertexPosition, float amplitude_, float waveLength_, float waveSpeed_) 
        {
            float frequency = Mathf.Sqrt(9.8f * Mathf.PI * 2 / waveLength_);
            Vector2 distance = vertexPosition - waveCenter;
            Vector2 Di = distance / distance.magnitude;
            float phaseConstant = waveSpeed_ * 2 / waveLength_;
            float waveHeight = amplitude_ * Mathf.Sin(Vector2.Dot(Di, vertexPosition) * frequency + phaseConstant * Time.time);
            return new Vector3(vertexPosition.x, waveHeight, vertexPosition.y);
        }

        //Gerstner waves is a simple a quite cheap way to generate realistic unidirectional water waves
        public float GetWaterHeight(Vector2 position) 
        {
            /* Needs better approximation for more waves */
            return new GerstnerWave(waveDirection, waveLength, waveAmplitude, waveSteepness, waveSpeed).GetWaveHeight(position)
                + transform.position.y;
        }
    }
}