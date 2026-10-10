// Interfaz comun que implementan los 3 receptores (CSV, JSON, Binario).
// Gracias a esto, NaveController.cs no necesita saber que protocolo
// estas usando: simplemente pide "el que este en este objeto".

public interface IControlSource
{
    bool[] Botones { get; }     // [0]=b1, [1]=b2, [2]=b3, [3]=b4
    int Potenciometro { get; }  // 0 a 1023
}
