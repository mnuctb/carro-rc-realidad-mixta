// =============================================================
// AUTOMATIC MODE - Anti-collision car with HC-SR04 ultrasonic sensor
// Baud rate: 9600
//
// MODO 1: sensor test (prints the distance)
// MODO 2: motor test (forward, backward, left, right)
// MODO 3: anti-collision robot (mode used in the project)
//
// Logic of MODO 3:
//   distance > 20 cm or no reading -> move forward
//   distance <= 20 cm -> stop, move backward 400 ms, turn right 500 ms
// =============================================================

#define MODO 3            // <-- Change to 1, 2 or 3

// ---------- Pins ----------
const int TRIG = 2;
const int ECHO = 3;

const int IN1 = 6;        // Motor A (top in the schematic)
const int IN2 = 5;
const int IN3 = 11;       // Motor B (bottom in the schematic)
const int IN4 = 10;

// ---------- Parameters ----------
const int DISTANCIA_MINIMA = 20;  // cm: if something is closer than this, dodge it

// ---------- Sensor ----------
// Sends a 10 us pulse and converts the echo time into centimeters
long medirDistancia() {
  digitalWrite(TRIG, LOW);
  delayMicroseconds(2);
  digitalWrite(TRIG, HIGH);
  delayMicroseconds(10);
  digitalWrite(TRIG, LOW);

  long duracion = pulseIn(ECHO, HIGH, 30000);  // timeout 30 ms (~5 m)
  if (duracion == 0) return -1;                // no reading
  return duracion * 0.034 / 2;                 // cm (speed of sound, round trip)
}

// ---------- Motors ----------
void adelante() {
  digitalWrite(IN1, HIGH); digitalWrite(IN2, LOW);
  digitalWrite(IN3, HIGH); digitalWrite(IN4, LOW);
}

void atras() {
  digitalWrite(IN1, LOW); digitalWrite(IN2, HIGH);
  digitalWrite(IN3, LOW); digitalWrite(IN4, HIGH);
}

void izquierda() {
  digitalWrite(IN1, LOW); digitalWrite(IN2, HIGH);   // A backward
  digitalWrite(IN3, HIGH); digitalWrite(IN4, LOW);   // B forward
}

void derecha() {
  digitalWrite(IN1, HIGH); digitalWrite(IN2, LOW);   // A forward
  digitalWrite(IN3, LOW); digitalWrite(IN4, HIGH);   // B backward
}

void parar() {
  digitalWrite(IN1, LOW); digitalWrite(IN2, LOW);
  digitalWrite(IN3, LOW); digitalWrite(IN4, LOW);
}

// ---------- Setup ----------
void setup() {
  Serial.begin(9600);
  pinMode(TRIG, OUTPUT);
  pinMode(ECHO, INPUT);
  pinMode(IN1, OUTPUT); pinMode(IN2, OUTPUT);
  pinMode(IN3, OUTPUT); pinMode(IN4, OUTPUT);
  parar();

  Serial.print("Modo seleccionado: ");
  Serial.println(MODO);
  delay(2000); // time to put the robot down
}

// ---------- Loop ----------
void loop() {

#if MODO == 1
  // ===== SENSOR TEST =====
  long d = medirDistancia();
  if (d < 0) {
    Serial.println("Sin lectura (revisa TRIG/ECHO y la alimentacion)");
  } else {
    Serial.print("Distancia: ");
    Serial.print(d);
    Serial.println(" cm");
  }
  delay(300);

#elif MODO == 2
  // ===== MOTOR TEST =====
  Serial.println("Adelante");  adelante();  delay(2000); parar(); delay(1000);
  Serial.println("Atras");     atras();     delay(2000); parar(); delay(1000);
  Serial.println("Izquierda"); izquierda(); delay(1000); parar(); delay(1000);
  Serial.println("Derecha");   derecha();   delay(1000); parar(); delay(3000);

#elif MODO == 3
  // ===== ANTI-COLLISION ROBOT =====
  long d = medirDistancia();
  Serial.print("Distancia: ");
  Serial.println(d);

  if (d > DISTANCIA_MINIMA || d < 0) {
    adelante();  // path is clear (or no reading) -> keep going
  } else {
    // Obstacle closer than DISTANCIA_MINIMA -> dodge it
    parar();   delay(200);
    atras();   delay(400);
    parar();   delay(200);
    derecha(); delay(500);
    parar();   delay(200);
  }
  delay(50);
#endif
}
