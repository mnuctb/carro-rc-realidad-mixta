using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Moves the car in Unity with the arrow keys or the on-screen buttons
/// (forward/backward and turning can be combined) and highlights the
/// pressed buttons on the Canvas.
///
/// USE: add it to the "Car" object and drag the 4 buttons in the Inspector.
/// </summary>
public class CarroUnity : MonoBehaviour
{
    [Header("Botones del control (arrastra el objeto de cada botón)")]
    public Image btnArriba;
    public Image btnAbajo;
    public Image btnIzquierda;
    public Image btnDerecha;

    [Header("Colores")]
    public Color colorNormal = Color.white;
    public Color colorPresionado = new Color(0.1f, 0.8f, 0.3f, 1f);   // green when pressed

    [Header("Movimiento")]
    public float velocidad = 3f;          // units per second
    public float velocidadGiro = 90f;     // degrees per second
    public bool invertirAdelante = false; // check it if the bus moves backwards

    // -1, 0 or 1
    public int Avance { get; private set; }  // 1 = forward, -1 = backward
    public int Giro { get; private set; }    // 1 = right, -1 = left

    /// <summary>
    /// Command for the Arduino:
    /// S stop | F forward | B backward | L left | R right
    /// Q forward+left | E forward+right | Z backward+left | C backward+right
    /// </summary>
    public char ComandoActual
    {
        get
        {
            if (Avance == 1) return Giro == -1 ? 'Q' : Giro == 1 ? 'E' : 'F';
            if (Avance == -1) return Giro == -1 ? 'Z' : Giro == 1 ? 'C' : 'B';
            return Giro == -1 ? 'L' : Giro == 1 ? 'R' : 'S';
        }
    }

    void Update()
    {
        LeerEntrada();
        Mover();
        ActualizarBotones();
    }

    // Reads the keyboard and the on-screen buttons
    void LeerEntrada()
    {
        int av = 0, gi = 0;

        // Keyboard: each axis is read separately, so they can be combined
        var k = Keyboard.current;
        if (k != null)
        {
            if (k.upArrowKey.isPressed) av += 1;
            if (k.downArrowKey.isPressed) av -= 1;
            if (k.rightArrowKey.isPressed) gi += 1;
            if (k.leftArrowKey.isPressed) gi -= 1;
        }

        // Mouse over the Canvas buttons
        if (Presionado(btnArriba)) av = 1;
        if (Presionado(btnAbajo)) av = -1;
        if (Presionado(btnDerecha)) gi = 1;
        if (Presionado(btnIzquierda)) gi = -1;

        Avance = av;
        Giro = gi;
    }

    // True if the left mouse button is held down over this button
    bool Presionado(Image boton)
    {
        if (boton == null || Mouse.current == null) return false;
        if (!Mouse.current.leftButton.isPressed) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(
            boton.rectTransform, Mouse.current.position.ReadValue(), null);
    }

    // Moves and rotates the virtual car
    void Mover()
    {
        float dir = invertirAdelante ? -1f : 1f;
        if (Avance != 0)
            transform.Translate(Vector3.forward * Avance * dir * velocidad * Time.deltaTime);
        if (Giro != 0)
            transform.Rotate(0f, Giro * velocidadGiro * Time.deltaTime, 0f);
    }

    // Paints the pressed buttons green
    void ActualizarBotones()
    {
        Pintar(btnArriba, Avance == 1);
        Pintar(btnAbajo, Avance == -1);
        Pintar(btnIzquierda, Giro == -1);
        Pintar(btnDerecha, Giro == 1);
    }

    void Pintar(Image b, bool activo)
    {
        if (b != null) b.color = activo ? colorPresionado : colorNormal;
    }
}