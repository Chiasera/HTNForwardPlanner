using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GerstnerWave
{
    Vector2 waveDirection;
    float waveLength;
    float waveAmplitude;
    float waveSteepness;
    float waveSpeed;

    public GerstnerWave(Vector2 waveDirection, float waveLength, float waveAmplitude, float waveSteepness, float waveSpeed)
    {
        this.waveDirection = waveDirection;
        this.waveLength = waveLength;
        this.waveAmplitude = waveAmplitude;
        this.waveSteepness = waveSteepness;
        this.waveSpeed = waveSpeed;
    }
    // Start is called before the first frame update
    private Vector3 GetWave(Vector2 position) // position = coordinates of the floating object on the XZ plane --> INPUT to getstner wave function
    {
        float gravityForce = 9.8f;
        float sqrRoot = Mathf.Sqrt(((Mathf.PI * 2) / waveLength) * gravityForce);
        float dirX = waveDirection.x * -1;
        float dirY = waveDirection.y * -1;
        Vector2 normalDir = (new Vector2(dirX, dirY)).normalized;
        Vector2 sqrRoot_times_normalDir = sqrRoot * normalDir;
        float input_dot_sqrRoot_times_normalDir = Vector2.Dot(position, sqrRoot_times_normalDir);
        float amp_times_steepness = waveAmplitude * waveSteepness; // atp
        float time = Time.time;
        float time_times_waveSpeed = time * waveSpeed; // ttw
        float normDirX_times_atp = normalDir.x * amp_times_steepness;
        float normDirY_times_atp = normalDir.y * amp_times_steepness;
        float addDot = input_dot_sqrRoot_times_normalDir + time_times_waveSpeed;
        float X_component = Mathf.Cos(addDot) * normDirX_times_atp;
        float Y_component = waveAmplitude * Mathf.Sin(addDot);
        float Z_component = Mathf.Cos(addDot) * normDirY_times_atp;
        return new Vector3(X_component, Y_component, Z_component);
    }

    public float GetWaveHeight(Vector2 position)
    {
        return GetWave(position).y;
    }
}
