#include <WiFi.h>
#include "EPD.h"
#include "DEV_Config.h"
#include "GUI_Paint.h"

// — Wi-Fi & image endpoint ———————————————————————————————
const char* ssid     = "x";
const char* password = "x";
const char* HOST     = "192.168.x.x";
const uint16_t PORT  = 5000;
const char* PATH     = "/Home/default.bmp";

// — Framebuffers for black & red ——————————————————————————————
UBYTE *BlackImage = nullptr;
UBYTE *RYImage    = nullptr;

void setup() {
  Serial.begin(115200);
  DEV_Module_Init();

  // 1) Allocate framebuffers
  const int W = EPD_7IN5B_V2_WIDTH;
  const int H = EPD_7IN5B_V2_HEIGHT;
  size_t bufSize = ((W + 7) / 8) * H;
  BlackImage = (UBYTE*)malloc(bufSize);
  RYImage    = (UBYTE*)malloc(bufSize);
  if (!BlackImage || !RYImage) {
    Serial.println("ERROR: not enough RAM"); while(1) delay(1000);
  }

  // 2) Wi-Fi connect
  WiFi.begin(ssid, password);
  Serial.print("Wi-Fi connecting");
  while (WiFi.status() != WL_CONNECTED) {
    delay(500); Serial.print(".");
  }
  Serial.println(" ✅");
  Serial.print("ESP32 IP = "); Serial.println(WiFi.localIP());

  // 3) Prepare white canvases
  Paint_NewImage(BlackImage, W, H, 0, WHITE);
  Paint_NewImage(RYImage,    W, H, 0, WHITE);
  Paint_SelectImage(BlackImage); Paint_Clear(WHITE);
  Paint_SelectImage(RYImage);    Paint_Clear(WHITE);

  // 4) Manual HTTP GET via WiFiClient
  {
    WiFiClient client;
Serial.printf("Connecting to %s:%u …", HOST, PORT);
if (!client.connect(HOST, PORT)) {
  Serial.println(" FAILED"); 
} else {
  Serial.println(" OK");

  // Send the GET request
  client.printf("GET %s HTTP/1.1\r\n", PATH);
  client.printf("Host: %s\r\n", HOST);
  client.print ("Connection: close\r\n\r\n");

  // Wait up to 15 s for the first byte
  uint32_t start = millis();
  while (!client.available() && millis() - start < 15000) {
    delay(10);
  }

  if (!client.available()) {
    Serial.println("No response—timeout");
  } else {
    Serial.println("Received response, parsing…");

    // ✅ 4.1 Read and parse status line
    String statusLine = client.readStringUntil('\n');
    Serial.print("HTTP Status Line: ");
    Serial.println(statusLine);

    int statusCode = statusLine.substring(9, 12).toInt(); // "HTTP/1.1 200 OK"
    if (statusCode != 200) {
      Serial.printf("❌ HTTP Error: %d\n", statusCode);
      client.stop();
      return;
    }

    // ✅ 4.2 Skip the remaining HTTP headers
    while (client.available()) {
      String line = client.readStringUntil('\n');
      if (line == "\r" || line == "") break; // End of headers
    }

    // ✅ Continue with BMP header reading...
    // 4.3 Read BMP header
    uint8_t header[54];
    client.readBytes(header, 54);
    uint32_t dataOffset = 
        uint32_t(header[10]) 
      | (uint32_t(header[11]) << 8) 
      | (uint32_t(header[12]) << 16) 
      | (uint32_t(header[13]) << 24);

        // 4.3 Skip any extra header padding
        if (dataOffset > 54) {
          uint32_t toSkip = dataOffset - 54;
          uint8_t dum[32];
          while (toSkip) {
            size_t chunk = toSkip > sizeof(dum) ? sizeof(dum) : toSkip;
            client.readBytes(dum, chunk);
            toSkip -= chunk;
          }
        }

        // 4.4 Decode bottom-up, line by line
        int rowSize = ((W * 3 + 3) / 4) * 4;
        uint8_t *rowBuf = (uint8_t*)malloc(rowSize);
        if (!rowBuf) {
          Serial.println("ERROR: rowBuf malloc failed");
        }
        else {
          for (int y = H - 1; y >= 0; y--) {
            client.readBytes(rowBuf, rowSize);

            uint8_t mask   = 0x80;
            uint32_t idx   = (y * W) / 8;
            for (int x = 0; x < W; x++) {
              uint8_t b = rowBuf[x*3 + 0];
              uint8_t g = rowBuf[x*3 + 1];
              uint8_t r = rowBuf[x*3 + 2];

              bool isRed   = (r > 150 && g < 80 && b < 80);
              bool isBlack = (!isRed && ((r+g+b)/3 < 180));
              if (isRed)   RYImage[idx]   &= ~mask;
              if (isBlack) BlackImage[idx] &= ~mask;

              mask >>= 1;
              if (!mask) { mask = 0x80; idx++; }
            }
          }
          free(rowBuf);
          Serial.println("BMP decoded successfully!");
        }
      }
      client.stop();
    }
  }

  // 5) Display on the e-ink
  EPD_7IN5B_V2_Init();
  EPD_7IN5B_V2_Display(BlackImage, RYImage);
  DEV_Delay_ms(2000);
  EPD_7IN5B_V2_Sleep();

  // 6) Clean up
  free(BlackImage);
  free(RYImage);
}

void loop() {
  // nothing
}
