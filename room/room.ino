
#include <LiquidCrystal.h>
#include <ArduinoJson.h>
#include <async.h>

static bool isConnected;
static int comTimeout;

static int character;
static bool finishRead;
static String fullstr;

const int rs = 12, en = 11, d4 = 5, d5 = 4, d6 = 3, d7 = 2;
LiquidCrystal lcd(rs, en, d4, d5, d6, d7);
Async asyncEngine = Async();  // Instances the engine
short id = -1;

int totalColumns = 16;
int totalRows = 2;

void scrollMessage(int row, String message, int delayTime, int totalColumns) {
  for (int i = 0; i < totalColumns; i++) {
    message = " " + message;
  }
  message = message + " ";
  for (int position = 0; position < message.length(); position++) {


    lcd.setCursor(0, row);
    lcd.print(message.substring(position, position + totalColumns));
    delay(delayTime);
  }
}
void thread2() {
  if (!isConnected) {
    scrollMessage(0, "No Connection", 250, totalColumns);
  }
}
void setup() {
  pinMode(6, OUTPUT);  // sets the pin as output
  lcd.begin(16, 2);
  lcd.clear();
  Serial.begin(9600);
  isConnected = false;

  id = asyncEngine.setInterval(thread2, 100);

  //  asyncEngine.setTimeout(thread2, 100);
}
void connected() {
  isConnected = true;
  lcd.clear();
  lcd.setCursor(3, 0);
  lcd.print("Connected!!");
  delay(800);
  lcd.clear();
  lcd.setCursor(3, 1);
  lcd.print("Connected!!");
  delay(1000);
}
void loop() {
  lcd.setCursor(0, 0);
  //   if (!isConnected) {
  //   scrollMessage(0, "No Connection", 250, totalColumns);
  // }
  if (!isConnected) {
    asyncEngine.run();
  }
  comTimeout++;
  if (finishRead) {
    comTimeout = 0;
    JsonDocument doc;
    deserializeJson(doc, fullstr.c_str());
    const int system = doc["system"];
    const char* str = doc["str"];
    switch (system) {
      case 0:
        {
          if (!isConnected) {
            connected();
          }
          break;
        }
      case 1:
        {
          if (!isConnected) {
            connected();
          }
          const int val = doc["value"];
          const int val2 = doc["value2"];
          const bool clearlcd = doc["clearlcd"];
          if (clearlcd) {
            lcd.clear();
          }
          lcd.setCursor(val, val2);
          lcd.print(str);
          break;
        }
      case 2:
        {
          const int val = doc["value"];
          analogWrite(6, val);
          break;
        }
    }

    finishRead = false;
    fullstr = "";
  }
  if (Serial.available()) {
    char data = (char)Serial.read();

    if (data == '\n') {
      character = 0;
      finishRead = true;
      return;
    }


    fullstr += data;
    // Serial.write(fullstr.c_str());
    character++;
  }
  if (comTimeout > 25000) {
    lcd.print("no");
    comTimeout = 0;
    isConnected = false;
    lcd.clear();
  }
}
