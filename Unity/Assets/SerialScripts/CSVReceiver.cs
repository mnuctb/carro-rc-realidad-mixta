using UnityEngine;
using System.IO.Ports;
using System.Threading.Tasks;

// PROTOCOLO A: decodifica lineas de texto "b1,b2,b3,b4,pot\n"
public class CSVReceiver : MonoBehaviour, IControlSource
{
    [Tooltip("Cambia esto por tu puerto real (revisa en el Arduino IDE)")]
    public string puerto = "COM6";

    SerialPort serialPort;
    string buffer = "";

    bool[] botones = new bool[4];
    int potenciometro = 0;

    public bool[] Botones => botones;
    public int Potenciometro => potenciometro;

    void Start()
    {
        serialPort = new SerialPort(puerto, 9600);
        serialPort.DtrEnable = true;
        serialPort.Open();
        _ = ReceiveLoop();
    }

    async Task ReceiveLoop()
    {
        while (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                buffer += serialPort.ReadExisting();
                int idx;
                // puede llegar mas de una linea junta: se procesan todas
                while ((idx = buffer.IndexOf('\n')) >= 0)
                {
                    string linea = buffer.Substring(0, idx).Trim();
                    buffer = buffer.Substring(idx + 1);
                    ProcesarLinea(linea);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Error serial (CSV): " + ex.Message);
            }
            await Task.Delay(20);
        }
    }

    void ProcesarLinea(string linea)
    {
        string[] partes = linea.Split(',');
        if (partes.Length != 5) return; // paquete incompleto/corrupto: se descarta

        for (int i = 0; i < 4; i++)
            botones[i] = partes[i] == "1";

        int.TryParse(partes[4], out potenciometro);
    }

    void OnDestroy()
    {
        serialPort?.Close();
    }
}
