using UnityEngine;
using System.IO.Ports;
using System.Threading.Tasks;

[System.Serializable]
public class EstadoJSON
{
    public int b1, b2, b3, b4, pot;
}
public class JSONReceiver : MonoBehaviour, IControlSource
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
                while ((idx = buffer.IndexOf('\n')) >= 0)
                {
                    string linea = buffer.Substring(0, idx).Trim();
                    buffer = buffer.Substring(idx + 1);
                    ProcesarLinea(linea);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Error serial (JSON): " + ex.Message);
            }
            await Task.Delay(20);
        }
    }

    void ProcesarLinea(string linea)
    {
        if (linea.Length == 0) return;

        int inicio = linea.IndexOf('{');
        int fin = linea.LastIndexOf('}');
        if (inicio < 0 || fin < 0 || fin < inicio) return; 
        linea = linea.Substring(inicio, fin - inicio + 1);

        try
        {
            EstadoJSON estado = JsonUtility.FromJson<EstadoJSON>(linea);
            botones[0] = estado.b1 == 1;
            botones[1] = estado.b2 == 1;
            botones[2] = estado.b3 == 1;
            botones[3] = estado.b4 == 1;
            potenciometro = estado.pot;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("JSON invalido, se descarta: " + linea + " (" + ex.Message + ")");
        }
    }

    void OnDestroy()
    {
        serialPort?.Close();
    }
}