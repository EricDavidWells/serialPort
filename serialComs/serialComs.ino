int analogValue;
byte headercheck1 = 0x9F; // 159
byte headercheck2 = 0x6E; // 110
byte footercheck1 = 0x7D;
byte footercheck2 = 0x8E;
unsigned long timer = 0;
long loopTimeMicroSec = 5000;


void setup() {
  // Setup serial port
  Serial.begin(115200);
}


void loop() {
  // Read the analog pin
  analogValue = analogRead(A0);
  int data[] = {analogValue, analogValue+2, analogValue+4, analogValue+6};

  // Write bytes via serial
  writeBytes(data, 8);
  timeSync(loopTimeMicroSec);
}


void writeBytes(int* data_, int numdata){
  // Cast to a byte pointer
  byte* byteData1 = (byte*)(data_);

  int msgsize = numdata + 4;

  // add header checks to buffer
  byte buf[numdata+msgsize] = {headercheck1, headercheck2};

  // fill rest of buffer with two bytes per integer in data array
  for (int i=0; i<numdata; i+=2){
    buf[i+2] = *(byteData1+i);
    buf[i+3] = *(byteData1+i+1);
  }

  buf[msgsize-2] = footercheck1;
  buf[msgsize-1] = footercheck2;

  // Write the bytes
  Serial.write(buf, msgsize);
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
