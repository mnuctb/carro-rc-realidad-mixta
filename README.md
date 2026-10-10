# Carro RC - Realidad Mixta

Grupo 2 - Universidad de San Buenaventura 2026-2

Carro con Arduino Uno y dos motores DC que funciona en dos modos:

- **Modo automático:** avanza solo y esquiva obstáculos con un sensor de ultrasonido HC-SR04.
- **Modo RC:** se maneja desde una interfaz en Unity, conectada al Arduino por el puerto serial.

Video de funcionamiento: https://www.youtube.com/watch?v=4e7wRNTBTfI

## Carpetas

- **Arduino/ModoAutomatico:** carro anticolisión (usa `MODO 3`; los modos 1 y 2 son pruebas del sensor y de los motores).
- **Arduino/ModoRC:** recibe los comandos de Unity y mueve el carro.
- **Unity:** proyecto con la interfaz de control. Los scripts usados son `CarUnity.cs` y `Serialarduino.cs`.

## Conexión

| Componente | Pin del Arduino |
| --- | --- |
| HC-SR04 TRIG | 2 |
| HC-SR04 ECHO | 3 |
| Motor A (izquierdo) IN1 / IN2 | 6 / 5 |
| Motor B (derecho) IN3 / IN4 | 11 / 10 |

Los motores se conectan al Arduino a través del puente H, y el sensor se alimenta con 5 V y GND del Arduino.

## Cómo usarlo

**Cambio de modo:** se carga en el Arduino el programa del modo que se quiere usar.

**Modo automático**

1. Subir `ModoAutomatico.ino` al Arduino con `MODO 3`.
2. Poner el carro en el piso: avanza y, si encuentra un obstáculo a 20 cm o menos, se detiene, retrocede y gira a la derecha.

**Modo RC**

1. Subir `ModoRC.ino` al Arduino y cerrar el monitor serial.
2. Abrir la carpeta `Unity` con Unity 6 y la escena `Assets/Scenes/SampleScene`.
3. En el objeto **Car**, escribir el puerto COM del Arduino en el campo **Puerto** del componente **SerialArduino**.
4. Dar Play y manejar con las flechas del teclado o con los botones de la pantalla. La interfaz muestra si está conectado, la distancia del sensor y el comando que está ejecutando el carro.

## Comandos (9600 baudios)

| Comando | Acción |
| --- | --- |
| F / B | Adelante / atrás |
| L / R | Girar a la izquierda / derecha |
| Q / E | Adelante + izquierda / adelante + derecha |
| Z / C | Atrás + izquierda / atrás + derecha |
| S | Detener |

Unity envía un comando cada 100 ms. Si el Arduino no recibe comandos durante 300 ms, el carro se detiene.

## Integrantes

Olaya Albino Yuliana, Celeita Rico Norely Yurley, Beltran Castaño Karen Manuela, Campo Rivera Daniela Alexandra
