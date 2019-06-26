int analogValue;
byte headercheck1 = 0x9F; // 159
byte headercheck2 = 0x6E; // 110
unsigned long timer = 0;
long loopTimeMicroSec = 1000;


void setup() {
  // Setup serial port
  Serial.begin(115200);
}


void loop() {
  // Read the analog pin
  analogValue = analogRead(A0);

  // Write bytes via serial
  writeBytes(&analogValue);
  timeSync(loopTimeMicroSec);
}


void writeBytes(int* data1){
  // Cast to a byte pointer
  byte* byteData1 = (byte*)(data1);

  // Byte array with header for transmission
  byte buf[4] = {headercheck1, headercheck2, byteData1[0], byteData1[1]};

  // Write the byte
  Serial.write(buf, 4);
}

void timeSync(unsigned long deltaT){
  // Calculate required delay to run at 200 Hz
  unsigned long currTime = micros();
  long timeToDelay = deltaT - (currTime - timer);

  if (timeToDelay > 5000){
    delay(timeToDelay / 1000);
    delayMicroseconds(timeToDelay % 1000);
  } else if (timeToDelay > 0){
    delayMicroseconds(timeToDelay);
  } else {}

  timer = currTime + timeToDelay;
}
