using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CarAgent : Agent
{
    public float speed = 10f;
    public float torque = 10f;

    public int score = 0;
    public bool resetOnCollision = true;

    private Transform _track;

    public override void Initialize()
    {
        GetTrackIncrement();
    }
//Movimiento del vehíulo
    private void MoveCar(float horizontal, float vertical, float dt)
    {
        float distance = speed * vertical;
        transform.Translate(distance * dt * Vector3.forward);

        float rotation = horizontal * torque * 90f;
        transform.Rotate(0f, rotation * dt, 0f);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
/*multiplicamos por el paso de tiempo (generalmente 1/60) para obtener como máximo una recompensa 
de 1 por segundo. Esto coincide con la velocidad a la que recorremos una sección, 
como máximo 1 por segundo y asegura que los valores de bonificación y recompensa 
contribuyan en una medida similar.*/
        float horizontal = vectorAction[0];
        float vertical = vectorAction[1];

        var lastPos = transform.position;
        MoveCar(horizontal, vertical, Time.fixedDeltaTime);

        int reward = GetTrackIncrement();
        
/*
obtenemos un vector de movimiento comparando una posición antes y
después del movimiento. Esto nos permite saber cuánto nos hemos
movido “a lo largo” de la pista. Luego, esto se mapea desde el
rango (180 °, 0 °) a (-1,1): cuanto mayor es el ángulo, 
menor es la bonificación, siendo el ángulo> 90 ° una penalización.*/
        var moveVec = transform.position - lastPos;
        float angle = Vector3.Angle(moveVec, _track.forward);
        float bonus = (1f - angle / 90f) * Mathf.Clamp01(vertical) * Time.fixedDeltaTime;
        AddReward(bonus);

        score += reward;
    }

    public override void Heuristic(float[] action)
    {
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
    }
/*comparar la diferencia en la dirección de la baldosa y el automóvil,
  el automóvil intentará minimizar este ángulo para seguir la 
  pista. 
  
  */ 
    public override void CollectObservations(VectorSensor vectorSensor)
    {
        float angle = Vector3.SignedAngle(_track.forward, transform.forward, Vector3.up);
//usamos un ángulo con signo (entre -180 y 180 grados) para indicarle al automóvil si debe girar a la derecha o a la izquierda.
        vectorSensor.AddObservation(angle / 180f);
//Usando Raycasting para observar la distancia de los objetos sólidos alrededor
        vectorSensor.AddObservation(ObserveRay(1.5f, .5f, 25f));//Frontal derecho
        vectorSensor.AddObservation(ObserveRay(1.5f, 0f, 0f));//Frontal
        vectorSensor.AddObservation(ObserveRay(1.5f, -.5f, -25f));//Frontal Izquierdo
        vectorSensor.AddObservation(ObserveRay(-1.5f, 0, 180f));//Atras
    }

/*Toma una posición y un ángulo en relación con los del automóvil y verifica si hay colisión. 
Si hay alguno, almacenamos la información como un valor positivo entre [0,1], si no, devolvemos -1.
*/
    private float ObserveRay(float z, float x, float angle)
    {
        var tf = transform;
    
        // Obtener la posición inicial del rayo
        var raySource = tf.position + Vector3.up / 2f; 
        const float RAY_DIST = 5f;
        var position = raySource + tf.forward * z + tf.right * x;

        // Obtener el ángulo del rayo
        var eulerAngle = Quaternion.Euler(0, angle, 0f);
        var dir = eulerAngle * tf.forward;
    
        // Ver si hay un golpe en la dirección dada
        Physics.Raycast(position, dir, out var hit, RAY_DIST);
        return hit.distance >= 0 ? hit.distance / RAY_DIST : -1f;
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
/*restablecer el automóvil a su posición inicial cuando golpea la pared; 
en combinación con la recompensa negativa, esto ayuda a entrenar el 
comportamiento de evitar la pared 
*/
    public override void OnEpisodeBegin()
    {
        if (resetOnCollision)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }

/*queremos evitar las colisiones con las paredes; esto se penaliza 
al establecer la recompensa -1f y finalizar el episodio de entrenamiento actual.*/
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("wall"))
        {
            SetReward(-1f);
            EndEpisode();
        }
    }
}