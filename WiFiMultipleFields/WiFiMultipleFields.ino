#include "WiFiEsp.h"
#include "secrets.h"
#include "ThingSpeak.h" 

char ssid[] = SECRET_SSID; 
char pass[] = SECRET_PASS;
int keyIndex = 0;            
WiFiEspClient  client;

#ifndef HAVE_HWSERIAL1
#include "SoftwareSerial.h"
SoftwareSerial Serial1(4, 3); 
#define ESP_BAUDRATE  19200
#else
#define ESP_BAUDRATE  115200
#endif 

#define moisture A0
#define lux A1
#define direct_1 7
#define direct_2 6
#define echo_pin 10
#define trick_pin 11


unsigned long myChannelNumber = SECRET_CH_ID;
const char * myWriteAPIKey = SECRET_WRITE_APIKEY;

// Initialize values
int soilMoisture = 0;
int lightLevel = 0;
int waterLevel = 0;
int wateringInterval = 0; // 1 for watering, 0 for not watering

int soilMoistureLowerLimit = 20; // lower limit for soil moisture to start watering
int waterLevelLowerLimit = 2; // lower limit for water level to stop watering
String myStatus = "";

void setup() {
  Serial.begin(115200); 
  while(!Serial);

  // moisture and light input pins
  pinMode(moisture, INPUT);
  pinMode(lux, INPUT);
  // motor direction pins
  pinMode(direct_1,OUTPUT);
  pinMode(direct_2,OUTPUT);
  // pins for ultrasonic sound sensor
  pinMode(trick_pin, OUTPUT);
  pinMode(echo_pin, INPUT); 


  setEspBaudRate(ESP_BAUDRATE);
  
  while (!Serial);

  Serial.print("Searching for ESP8266..."); 
  WiFi.init(&Serial1);

  if (WiFi.status() == WL_NO_SHIELD) {
    Serial.println("WiFi shield not present");
    while (true);
  }
  Serial.println("found it!");
   
  ThingSpeak.begin(client);
}

void loop() {
  if(WiFi.status() != WL_CONNECTED){
    Serial.print("Attempting to connect to SSID: ");
    Serial.println(SECRET_SSID);
    while(WiFi.status() != WL_CONNECTED){
      WiFi.begin(ssid, pass);
      Serial.print(".");
      delay(5000);     
    } 
    Serial.println("\nConnected.");
  }

  // get moisture and light values from sensors
  soilMoisture = map(analogRead(moisture),0,1023,100,0);
  lightLevel = map(analogRead(lux),0,1023,100,0);
  
  // get water level
  digitalWrite(trick_pin, HIGH);
  delayMicroseconds(10);
  digitalWrite(trick_pin, LOW);

  long duration = pulseIn(echo_pin, HIGH);
  waterLevel = 12 - duration * 0.017;


  // check soil moisture and water level to start watering or not
  if (soilMoisture < soilMoistureLowerLimit && waterLevel >= waterLevelLowerLimit) {
    wateringInterval = 1; // start watering
    myStatus = "Watering started";
    // code to start watering
    digitalWrite(direct_1,HIGH);
    digitalWrite(direct_2,LOW);
    
    delay(15000); // watering time
    myStatus = "Watering completed";
    // code to stop watering
    digitalWrite(direct_1,LOW);
    
  }
  // print statements to see the current values on serial monitor
  Serial.print("Soil moisture level is ");
  Serial.println(soilMoisture);
  Serial.print("Light level is ");
  Serial.println(lightLevel);
  Serial.print("Water level is ");
  Serial.println(waterLevel);
  Serial.print("Water interval is ");
  Serial.println(wateringInterval);

  // set the fields with the values
  ThingSpeak.setField(1, soilMoisture);
  ThingSpeak.setField(2, lightLevel);
  ThingSpeak.setField(3, waterLevel);
  ThingSpeak.setField(4, wateringInterval);
  wateringInterval = 0;
  // set the status
  ThingSpeak.setStatus(myStatus);
  
  // write to the ThingSpeak channel
  int x = ThingSpeak.writeFields(myChannelNumber, myWriteAPIKey);
  if(x == 200){
    Serial.println("Channel update successful.");
  }
  else{
    Serial.println("Problem updating channel. HTTP error code " + String(x));
  }
  
  delay(15000); // Wait  seconds to update the channel again
}

void setEspBaudRate(unsigned long baudrate){
  long rates[6] = {115200,74880,57600,38400,19200,9600};

  Serial.print("Setting ESP8266 baudrate to ");
  Serial.print(baudrate);
  Serial.println("...");

  for(int i = 0; i < 6; i++){
    Serial1.begin(rates[i]);
    delay(100);
    Serial1.print("AT+UART_DEF=");
    Serial1.print(baudrate);
    Serial1.print(",8,1,0,0\r\n");
    delay(100);  
  }
    
  Serial1.begin(baudrate);
}
