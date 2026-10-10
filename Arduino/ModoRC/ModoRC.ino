// =============================================================
// RC MODE - Car controlled from Unity through the serial port
// Baud rate: 9600
//
// Commands received from Unity (one character, sent every 100 ms):
//   S stop            | F forward          | B backward
//   L turn left       | R turn right
//   Q forward + left  | E forward + right
//   Z backward + left | C backward + right
// Any other character (for example 'M') is ignored.
//
// Messages sent to Unity every 200 ms:
//   "Distancia: <cm>"  -> distance measured by the ultrasonic sensor
//   "E:<command>"      -> command the motors are actually executing
// =============================================================

// ---------- Pins ----------
const int TRIG = 2;   // HC-SR04 trigger
const int ECHO = 3;   // HC-SR04 echo
const int IN1 = 6;    // Motor A (left)
const int IN2 = 5;
const int IN3 = 11;   // Motor B (right)
const int IN4 = 10;

// ---------- Parameters ----------
const unsigned long TIMEOUT_ORDEN = 300;   // ms without commands -> the car stops
const bool FRENO_OBSTACULO = false;        // true = won't move forward if something is close
const int DISTANCIA_MINIMA = 15;           // cm

char orden = 'S';                 // last command received
unsigned long tUltimaOrden = 0;   // time of the last command
unsigned long tInforme = 0;       // time of the last message sent to Unity
long distancia = -1;              // last distance (cm), -1 = no reading

// Sends a 10 us pulse and converts the echo time into centimeters
long medirDistancia() {
  digitalWrite(TRIG, LOW);
  delayMicroseconds(2);
  digitalWrite(TRIG, HIGH);
  delayMicroseconds(10);
  digitalWrite(TRIG, LOW);
  long duracion = pulseIn(ECHO, HIGH, 30000);  // timeout 30 ms (~5 m)
  if (duracion == 0) return -1;                // no echo received
  return duracion * 0.034 / 2;                 // speed of sound 0.034 cm/us, round trip
}

// d = 1 forward, -1 backward, 0 stopped
void motorA(int d) {
  digitalWrite(IN1, d > 0 ? HIGH : LOW);
  digitalWrite(IN2, d < 0 ? HIGH : LOW);
}

void motorB(int d) {
  digitalWrite(IN3, d > 0 ? HIGH : LOW);
  digitalWrite(IN4, d < 0 ? HIGH : LOW);
}

// Translates a command into the direction of each motor
void aplicar(char c) {
  switch (c) {
    case 'F': motorA(1);  motorB(1);  break;
    case 'B': motorA(-1); motorB(-1); break;
    case 'L': motorA(-1); motorB(1);  break;
    case 'R': motorA(1);  motorB(-1); break;
    case 'Q': motorA(0);  motorB(1);  break;
    case 'E': motorA(1);  motorB(0);  break;
    case 'Z': motorA(-1); motorB(0);  break;
    case 'C': motorA(0);  motorB(-1); break;
    default:  motorA(0);  motorB(0);  break;   // 'S' or unknown -> stop
  }
}

void setup() {
  Serial.begin(9600);
  pinMode(TRIG, OUTPUT);
  pinMode(ECHO, INPUT);
  pinMode(IN1, OUTPUT); pinMode(IN2, OUTPUT);
  pinMode(IN3, OUTPUT); pinMode(IN4, OUTPUT);
  aplicar('S');
}

void loop() {
  // 1) Read the commands sent by Unity
  while (Serial.available() > 0) {
    char c = Serial.read();
    if (c == 'S' || c == 'F' || c == 'B' || c == 'L' || c == 'R' ||
        c == 'Q' || c == 'E' || c == 'Z' || c == 'C') {
      orden = c;
      tUltimaOrden = millis();
    }
  }

  // 2) Safety: if Unity stops sending commands, the car stops
  if (millis() - tUltimaOrden > TIMEOUT_ORDEN) orden = 'S';

  // 3) Optional obstacle brake (disabled by default)
  char efectiva = orden;
  if (FRENO_OBSTACULO && distancia >= 0 && distancia < DISTANCIA_MINIMA &&
      (orden == 'F' || orden == 'Q' || orden == 'E')) {
    efectiva = 'S';
  }
  aplicar(efectiva);

  // 4) Every 200 ms: measure the distance and report to Unity
  if (millis() - tInforme >= 200) {
    tInforme = millis();
    distancia = medirDistancia();
    Serial.print("Distancia: ");
    if (distancia < 0) Serial.println("--");
    else Serial.println(distancia);
    Serial.print("E:");
    Serial.println(efectiva);
  }
}
