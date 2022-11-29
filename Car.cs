using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public float speed = 10f;
    public float torque = 1f;

    public int score = 0;

    private Transform _track;

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal"); //entrada por teclado para el eje horizontal
        float vertical = Input.GetAxis("Vertical");//entrada por teclado para el eje vertical
        float dt = Time.deltaTime;
        MoveCar(horizontal, vertical, dt);
        score += GetTrackIncrement();
    }

//MOVIMIENTO DEL AUTO
    private  void MoveCar(float horizontal, float vertical, float dt)
    {
        // Traducido en la dirección en la que mira el coche
        float moveDist = speed * vertical;
        transform.Translate(dt * moveDist * Vector3.forward);

        // Gira a lo largo del eje hacia arriba
        float rotation = horizontal * torque * 90f;
        transform.Rotate(0f, rotation * dt, 0f);
    }

    private int GetTrackIncrement()
    {
        int reward = 0;
        var carCenter = transform.position + Vector3.up;

        // Encuentra en qué sección estoy
        if (Physics.Raycast(carCenter, Vector3.down, out var hit, 2f))
        {
            var newHit = hit.transform;
            // Comprobar si la sección ha cambiado
            if (_track != null && newHit != _track)
            {
                float angle = Vector3.Angle(_track.forward, newHit.position - _track.position);
                reward = (angle < 90f) ? 1 : -1;
            }

            _track = newHit;
        }

        return reward;
    }
}