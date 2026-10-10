using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

/// <summary>
/// Sends the car command (arrow keys / on-screen buttons) to the Arduino
/// through the serial port, and reads the distance and real state of the car.
///
/// USE: add it to the "Car" object (next to CarroUnity) and write the COM port
///      in the Inspector (no need to change the code).
/// REQUIREMENT: Edit > Project Settings > Player > Other Settings >
///              Api Compatibility Level = ".NET Framework"
/// </summary>
public class SerialArduino : MonoBehaviour
{
    [Header("Conexión")]
    public string puerto = "COM3";   // COM port of the Arduino
    public int baudios = 9600;       // must match Serial.begin() on the Arduino

    [Header("Referencias")]
    public CarroUnity carro;               // car script (if empty, it is found automatically)
    public TMPro.TMP_Text textoInfo;       // optional: shows connection state and distance

    SerialPort sp;
    Thread hilo;
    volatile bool corriendo;
    readonly ConcurrentQueue<string> cola = new ConcurrentQueue<string>();  // lines received

    float tEnvio, tModo;
    string distancia = "--";   // last distance reported by the Arduino
    char estadoReal = 'S';     // command the Arduino is really executing

    void Start()
    {
        if (carro == null) carro = GetComponent<CarroUnity>();

        // List the available ports in the console to help choose the right one
        Debug.Log("Puertos disponibles: " + string.Join(", ", SerialPort.GetPortNames()));
        try
        {
            sp = new SerialPort(puerto, baudios);
            sp.ReadTimeout = 200;
            sp.WriteTimeout = 200;
            sp.DtrEnable = true;
            sp.Open();
            corriendo = true;
            // Reading is done in a separate thread so Unity does not freeze
            hilo = new Thread(LeerLoop) { IsBackground = true };
            hilo.Start();
            Debug.Log("Conectado a " + puerto);
        }
        catch (Exception e)
        {
            Debug.LogWarning("No se pudo abrir " + puerto + ": " + e.Message +
                " (cierra el Monitor Serial del Arduino IDE y revisa el número de COM)");
        }
    }

    // Background thread: reads complete lines and stores them in the queue
    void LeerLoop()
    {
        while (corriendo)
        {
            try { cola.Enqueue(sp.ReadLine().Trim()); }
            catch (TimeoutException) { }
            catch (Exception) { break; }
        }
    }

    // Sends a one-character command to the Arduino
    void Enviar(char c)
    {
        if (sp == null || !sp.IsOpen) return;
        try { sp.Write(c.ToString()); } catch (Exception) { }
    }

    void Update()
    {
        // Process the lines sent by the Arduino
        while (cola.TryDequeue(out string l))
        {
            if (l.StartsWith("Distancia:")) distancia = l.Substring(10).Trim();
            else if (l.StartsWith("E:") && l.Length >= 3) estadoReal = l[2];
        }

        // 'M' is sent every 0.5 s to mark RC mode (the current sketch ignores it)
        if (Time.time - tModo > 0.5f) { tModo = Time.time; Enviar('M'); }

        // Send the current command 10 times per second
        if (Time.time - tEnvio > 0.1f)
        {
            tEnvio = Time.time;
            if (carro != null) Enviar(carro.ComandoActual);
        }

        // Show connection state, distance and real state on screen
        if (textoInfo != null)
        {
            bool ok = sp != null && sp.IsOpen;
            textoInfo.text = (ok ? "Conectado " + puerto : "SIN CONEXIÓN") +
                             "\nDistancia: " + distancia + " cm" +
                             "\nCarro real: " + estadoReal;
        }
    }

    void OnApplicationQuit() { Cerrar(); }
    void OnDestroy() { Cerrar(); }

    // Stops the car and closes the port when the game ends
    void Cerrar()
    {
        corriendo = false;
        Enviar('S');
        if (hilo != null) hilo.Join(300);
        if (sp != null && sp.IsOpen) sp.Close();
    }
}