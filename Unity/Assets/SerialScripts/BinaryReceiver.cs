using UnityEngine;
using System.IO.Ports;
using System.Threading.Tasks;

// PROTOCOLO C: decodifica paquetes binarios [0xAA][len][payload...][checksum]
// Se usa una maquina de estados: header -> longitud -> payload -> checksum.
public class BinaryReceiver : MonoBehaviour, IControlSource
{
    [Tooltip("Cambia esto por tu puerto real (revisa en el Arduino IDE)")]
    public string puerto = "COM6";

    SerialPort serialPort;

    bool[] botones = new bool[4];
    int potenciometro = 0;

    public bool[] Botones => botones;
    public int Potenciometro => potenciometro;

    enum Estado { EsperandoHeader, Longitud, Payload, Checksum }
    Estado estado = Estado.EsperandoHeader;
    byte longitud;
    byte[] payload;
    int payloadIndex;

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
                while (serialPort.BytesToRead > 0)
                {
                    byte b = (byte)serialPort.ReadByte();
                    ProcesarByte(b);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Error serial (Binario): " + ex.Message);
            }
            await Task.Delay(10);
        }
    }

    void ProcesarByte(byte b)
    {
        switch (estado)
        {
            case Estado.EsperandoHeader:
                if (b == 0xAA) estado = Estado.Longitud;
                break;

            case Estado.Longitud:
                longitud = b;
                payload = new byte[longitud];
                payloadIndex = 0;
                estado = Estado.Payload;
                break;

            case Estado.Payload:
                payload[payloadIndex++] = b;
                if (payloadIndex >= longitud) estado = Estado.Checksum;
                break;

            case Estado.Checksum:
                byte calculado = longitud;
                for (int i = 0; i < longitud; i++) calculado ^= payload[i];

                if (calculado == b)
                {
                    byte mask = payload[0];
                    for (int i = 0; i < 4; i++)
                        botones[i] = (mask & (1 << i)) != 0;
                    potenciometro = (payload[1] << 8) | payload[2];
                }
                else
                {
                    Debug.LogWarning("Checksum invalido: paquete descartado");
                }
                estado = Estado.EsperandoHeader; // listo para el siguiente paquete
                break;
        }
    }

    void OnDestroy()
    {
        serialPort?.Close();
    }
}
