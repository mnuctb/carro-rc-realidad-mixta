using UnityEngine;

// Controlador del carro (bus), version con movimiento suave.
// Funciona con CUALQUIERA de los 3 receptores (CSV, JSON o Binario),
// porque solo pide "el que implemente IControlSource" en este mismo objeto.
//
// Mapeo:
// - Boton 1 (b1): adelante
// - Boton 2 (b2): atras
// - Boton 3 (b3): izquierda
// - Boton 4 (b4): derecha
// - Potenciometro: regula la velocidad maxima (opcional)
//
// Mejoras respecto a la primera version:
// - El carro acelera y frena poco a poco (no arranca ni para de golpe)
// - Solo gira cuando avanza, como un carro de verdad
// - Al ir en reversa, el volante se invierte (como en un carro real)
// - El giro entra y sale suavemente
public class Car : MonoBehaviour
{
    IControlSource control;

    [Header("Movimiento")]
    public float velocidadMaxima = 8f;   // unidades por segundo
    public float aceleracion = 6f;       // que tan rapido arranca
    public float frenado = 10f;          // que tan rapido se detiene al soltar

    [Header("Giro")]
    public float giroMaximo = 70f;       // grados por segundo a velocidad maxima
    public float suavizadoGiro = 5f;     // que tan rapido entra el giro
    public bool girarConCarroQuieto = false; // true = tambien gira detenido

    [Header("Potenciometro")]
    public bool potenciometroRegulaVelocidad = true;
    [Range(0f, 1f)]
    public float velocidadMinima = 0.2f; // con el pot al minimo, va al 20 %

    float velocidadActual = 0f;          // velocidad real en este momento (con signo)
    float giroActual = 0f;               // -1 izquierda, 0 recto, +1 derecha

    void Start()
    {
        // Busca, en este mismo GameObject, cualquier script que implemente
        // IControlSource (CSVReceiver, JSONReceiver o BinaryReceiver).
        control = GetComponent<IControlSource>();
        if (control == null)
        {
            Debug.LogError("Car no encontro ningun receptor " +
                           "(agrega CSVReceiver, JSONReceiver o BinaryReceiver a este objeto)");
        }
    }

    void Update()
    {
        if (control == null) return;

        bool[] b = control.Botones;
        float dt = Time.deltaTime;

        // --- Entrada de los botones ---
        float avance = 0f;               // +1 adelante, -1 atras
        if (b[0]) avance += 1f;
        if (b[1]) avance -= 1f;

        float lado = 0f;                 // +1 derecha, -1 izquierda
        if (b[3]) lado += 1f;
        if (b[2]) lado -= 1f;

        // --- Velocidad: el potenciometro fija el maximo (0.2 a 1.0) ---
        float factorPot = 1f;
        if (potenciometroRegulaVelocidad)
        {
            factorPot = Mathf.Lerp(velocidadMinima, 1f, control.Potenciometro / 1023f);
        }

        // --- Aceleracion y frenado suaves ---
        float objetivo = avance * velocidadMaxima * factorPot;
        float ritmo = (avance != 0f) ? aceleracion : frenado;
        velocidadActual = Mathf.MoveTowards(velocidadActual, objetivo, ritmo * dt);

        // --- Giro suave ---
        giroActual = Mathf.MoveTowards(giroActual, lado, suavizadoGiro * dt);

        // El giro depende de la velocidad: quieto no gira, a toda velocidad gira al maximo
        float factorGiro = Mathf.Clamp01(Mathf.Abs(velocidadActual) / velocidadMaxima);
        if (girarConCarroQuieto) factorGiro = Mathf.Max(factorGiro, 0.4f);

        // En reversa el volante se invierte, como en un carro real
        float sentido = (velocidadActual < -0.01f) ? -1f : 1f;

        transform.Rotate(0f, giroActual * giroMaximo * factorGiro * sentido * dt, 0f);

        // --- Mover hacia adelante (solo en el plano, la altura no cambia) ---
        transform.position += transform.forward * velocidadActual * dt;
    }
}