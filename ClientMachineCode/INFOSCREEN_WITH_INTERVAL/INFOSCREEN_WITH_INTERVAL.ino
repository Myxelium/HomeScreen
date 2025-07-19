#include <WiFi.h>
#include <HTTPClient.h>
#include <stdint.h>
#include "DEV_Config.h"
#include "EPD.h"
#include "GUI_Paint.h"
#include <ArduinoJson.h>
#include <JPEGDEC.h>

// WiFi credentials
const char* ssid = "x";
const char* password = "x";

// API endpoints
const char* connectionInformation = "http://x:5000/home/configuration";

// These will be updated from the connection information
String imageUrl = ""; // Will be populated from JSON
uint64_t sleepDuration = 30e6; // Default 30 seconds in microseconds

// Display dimensions - use the constants from Waveshare library
#define EPD_WIDTH  EPD_7IN5B_V2_WIDTH
#define EPD_HEIGHT EPD_7IN5B_V2_HEIGHT

// =========== IMAGE TUNING PARAMETERS ===========
// Basic color thresholds
#define BLACK_TEXT_THRESHOLD 190  // rgb(51,51,51) should be detected as black

// Dithering settings - NEW
#define ENABLE_DITHERING true     // Set to false to disable dithering
#define DITHER_STRENGTH 8        // Lower values = stronger dithering (8-32)

// Contrast settings - NEW
#define ENHANCE_CONTRAST true     // Set to false to disable contrast enhancement
#define CONTRAST_LEVEL 30         // Contrast adjustment level (0-100)
// ===============================================

// Framebuffers for black and red layers
UBYTE *BlackImage, *RYImage;

// Error buffers for dithering
int16_t *errorR = NULL;
int16_t *errorG = NULL;
int16_t *errorB = NULL;

// Create an instance of the JPEG decoder
JPEGDEC jpeg;

// Apply contrast adjustment to RGB values
void adjustContrast(uint8_t *r, uint8_t *g, uint8_t *b) {
  if (!ENHANCE_CONTRAST) return;
  
  float contrast = (CONTRAST_LEVEL / 100.0) + 1.0;  // Convert to decimal & shift range: [0..2]
  float intercept = 128 * (1 - contrast);
  
  *r = constrain((*r * contrast) + intercept, 0, 255);
  *g = constrain((*g * contrast) + intercept, 0, 255);
  *b = constrain((*b * contrast) + intercept, 0, 255);
}

// JPEG draw callback function for JPEGDEC
int jpegDrawCallback(JPEGDRAW *pDraw) {
  // Get MCU block information
  uint16_t *pPixels = pDraw->pPixels;
  int x = pDraw->x;
  int y = pDraw->y;
  int width = pDraw->iWidth;
  int height = pDraw->iHeight;
  
  // Initialize error buffers for dithering if needed
  if (ENABLE_DITHERING && errorR == NULL) {
    errorR = (int16_t*)malloc(EPD_WIDTH * sizeof(int16_t));
    errorG = (int16_t*)malloc(EPD_WIDTH * sizeof(int16_t));
    errorB = (int16_t*)malloc(EPD_WIDTH * sizeof(int16_t));
    
    if (errorR && errorG && errorB) {
      memset(errorR, 0, EPD_WIDTH * sizeof(int16_t));
      memset(errorG, 0, EPD_WIDTH * sizeof(int16_t));
      memset(errorB, 0, EPD_WIDTH * sizeof(int16_t));
    } else {
      Serial.println("Failed to allocate dithering buffers");
      if (errorR) free(errorR);
      if (errorG) free(errorG);
      if (errorB) free(errorB);
      errorR = errorG = errorB = NULL;
    }
  }
  
  // Process each row in this MCU block
  for (int iy = 0; iy < height; iy++) {
    // Reset error buffers for each row
    if (ENABLE_DITHERING && errorR != NULL) {
      memset(errorR, 0, EPD_WIDTH * sizeof(int16_t));
      memset(errorG, 0, EPD_WIDTH * sizeof(int16_t));
      memset(errorB, 0, EPD_WIDTH * sizeof(int16_t));
    }
    
    // Process each pixel in the row
    for (int ix = 0; ix < width; ix++) {
      int pos_x = x + ix;
      int pos_y = y + iy;
      
      // Skip if outside display bounds
      if (pos_x >= EPD_WIDTH || pos_y >= EPD_HEIGHT) continue;
      
      // Get the 16-bit pixel value (RGB565)
      uint16_t pixel = pPixels[iy * width + ix];
      
      // Extract RGB components (565 format) and convert to 0-255 range
      uint8_t r = ((pixel >> 11) & 0x1F) << 3;
      uint8_t g = ((pixel >> 5) & 0x3F) << 2;
      uint8_t b = (pixel & 0x1F) << 3;
      
      // Apply contrast adjustment if enabled
      if (ENHANCE_CONTRAST) {
        adjustContrast(&r, &g, &b);
      }
      
      // Apply dithering errors if enabled
      if (ENABLE_DITHERING && errorR != NULL) {
        r = constrain(r + (errorR[pos_x] / DITHER_STRENGTH), 0, 255);
        g = constrain(g + (errorG[pos_x] / DITHER_STRENGTH), 0, 255);
        b = constrain(b + (errorB[pos_x] / DITHER_STRENGTH), 0, 255);
      }
      
      // Calculate grayscale value
      float gray = (r * 0.299 + g * 0.587 + b * 0.114);
      
      // ===== IMPROVED COLOR CLASSIFICATION LOGIC =====
      // Variable for final color (0=black, 1=white, 2=red)
      int finalColor;
      
      // Check for "redness" - how much stronger red is than other components
      float redness = r / (float)(g + b + 1);  // Add 1 to avoid division by zero
      
      // Check if this is likely a red pixel based on redness
      if (r > 100 && redness > 1.5) {
        finalColor = 2;  // Red
      }
      // If not red, determine if it's black or white based on grayscale
      else if (gray < BLACK_TEXT_THRESHOLD) {
        finalColor = 0;  // Black
      }
      else {
        finalColor = 1;  // White
      }
      
      // Determine target colors for error calculation
      uint8_t targetR, targetG, targetB;
      
      switch (finalColor) {
        case 0:  // Black
          targetR = targetG = targetB = 0;
          break;
        case 2:  // Red
          targetR = 255;
          targetG = targetB = 0;
          break;
        default: // White
          targetR = targetG = targetB = 255;
          break;
      }
      
      // Calculate and distribute dithering errors
      if (ENABLE_DITHERING && errorR != NULL) {
        int16_t err_r = r - targetR;
        int16_t err_g = g - targetG;
        int16_t err_b = b - targetB;
        
        // Floyd-Steinberg dithering pattern
        if (pos_x + 1 < EPD_WIDTH) {
          // Right pixel (7/16)
          errorR[pos_x + 1] += (err_r * 7) >> 4;
          errorG[pos_x + 1] += (err_g * 7) >> 4;
          errorB[pos_x + 1] += (err_b * 7) >> 4;
        }
        
        if (pos_x > 0 && pos_x + 1 < EPD_WIDTH) {
          errorR[pos_x - 1] += (err_r * 3) >> 4;  // left-down (3/16)
          errorG[pos_x - 1] += (err_g * 3) >> 4;
          errorB[pos_x - 1] += (err_b * 3) >> 4;
          
          errorR[pos_x] += (err_r * 5) >> 4;      // down (5/16)
          errorG[pos_x] += (err_g * 5) >> 4;
          errorB[pos_x] += (err_b * 5) >> 4;
          
          errorR[pos_x + 1] += (err_r * 1) >> 4;  // right-down (1/16)
          errorG[pos_x + 1] += (err_g * 1) >> 4;
          errorB[pos_x + 1] += (err_b * 1) >> 4;
        }
      }
      
      // Draw the pixel based on the final color
      switch (finalColor) {
        case 0:  // Black
          Paint_SelectImage(BlackImage);
          Paint_SetPixel(pos_x, pos_y, BLACK);
          Paint_SelectImage(RYImage);
          Paint_SetPixel(pos_x, pos_y, WHITE);
          break;
          
        case 2:  // Red
          Paint_SelectImage(BlackImage);
          Paint_SetPixel(pos_x, pos_y, WHITE);
          Paint_SelectImage(RYImage);
          Paint_SetPixel(pos_x, pos_y, BLACK); // BLACK in RY buffer = RED
          break;
          
        default: // White
          Paint_SelectImage(BlackImage);
          Paint_SetPixel(pos_x, pos_y, WHITE);
          Paint_SelectImage(RYImage);
          Paint_SetPixel(pos_x, pos_y, WHITE);
          break;
      }
    }
  }
  
  return 1; // Continue decoding
}

void setup() {
  Serial.begin(115200);
  Serial.println("E-Ink Display Initialization");
  
  // Calculate buffer size as in Waveshare example
  UWORD Imagesize = ((EPD_WIDTH % 8 == 0) ? (EPD_WIDTH / 8) : (EPD_WIDTH / 8 + 1)) * EPD_HEIGHT;
  
  // Allocate framebuffers
  BlackImage = (UBYTE *)malloc(Imagesize);
  RYImage = (UBYTE *)malloc(Imagesize);
  
  if ((BlackImage == NULL) || (RYImage == NULL)) {
    Serial.println("Failed to allocate memory for framebuffers!");
    while(1); // Halt if memory allocation fails
  }
  
  // Initialize the Paint library with the buffers
  Paint_NewImage(BlackImage, EPD_WIDTH, EPD_HEIGHT, 0, WHITE);
  Paint_NewImage(RYImage, EPD_WIDTH, EPD_HEIGHT, 0, WHITE);
  
  // Clear both buffers to WHITE using Paint library
  Paint_SelectImage(BlackImage);
  Paint_Clear(WHITE);
  Paint_SelectImage(RYImage);
  Paint_Clear(WHITE);
  
  Serial.println("Buffers allocated and cleared");

  // Connect to WiFi
  WiFi.begin(ssid, password);
  Serial.print("Connecting to WiFi");
  int wifiAttempts = 0;
  while (WiFi.status() != WL_CONNECTED && wifiAttempts < 20) {
    delay(500);
    Serial.print(".");
    wifiAttempts++;
  }
  
  if (WiFi.status() == WL_CONNECTED) {
    Serial.println();
    Serial.println("WiFi connected");
    Serial.print("IP address: ");
    Serial.println(WiFi.localIP());
  } else {
    Serial.println();
    Serial.println("WiFi connection failed!");
    return;
  }

  // Test connectivity to server and get configuration
  Serial.println("Fetching connection information...");
  if (fetchConnectionInformation()) {
    // Fetch and display image
    fetchAndDisplayImage();
  } else {
    Serial.println("Server connectivity test failed - skipping image fetch");
  }

  // Free dithering buffers if allocated - NEW
  if (errorR) free(errorR);
  if (errorG) free(errorG);
  if (errorB) free(errorB);
  errorR = errorG = errorB = NULL;

  // Put display to sleep
  EPD_7IN5B_V2_Sleep();

  // Free framebuffers
  free(BlackImage);
  free(RYImage);
  BlackImage = NULL;
  RYImage = NULL;

  // Enter deep sleep
  Serial.print("Going to sleep for ");
  Serial.print(sleepDuration / 60000000);
  Serial.println(" minutes");
  esp_sleep_enable_timer_wakeup(sleepDuration);
  esp_deep_sleep_start();
}

bool fetchConnectionInformation() {
  HTTPClient http;
  http.begin(connectionInformation);
  http.setTimeout(10000);
  
  int httpCode = http.GET();
  
  Serial.print("HTTP response code: ");
  Serial.println(httpCode);
  
  // Handle the response payload
  String payload = "";
  
  if (httpCode == HTTP_CODE_OK) {
    payload = http.getString();
    
    // Debug output to show the exact response
    Serial.println("-----RAW HTTP RESPONSE BEGIN-----");
    Serial.println(payload);
    Serial.println("-----RAW HTTP RESPONSE END-----");
    
    // Check if payload is empty
    if (payload.length() == 0) {
      Serial.println("Warning: Server returned empty response");
      http.end();
      return false;
    }
    
    // Try to find JSON content in the response
    int jsonStart = payload.indexOf('{');
    int jsonEnd = payload.lastIndexOf('}');
    
    if (jsonStart >= 0 && jsonEnd >= 0 && jsonEnd > jsonStart) {
      String jsonPayload = payload.substring(jsonStart, jsonEnd + 1);
      Serial.println("-----EXTRACTED JSON BEGIN-----");
      Serial.println(jsonPayload);
      Serial.println("-----EXTRACTED JSON END-----");
      
      // Deserialize the JSON document
      StaticJsonDocument<512> doc;
      DeserializationError error = deserializeJson(doc, jsonPayload);
      if (error) {
        Serial.print("JSON parsing failed: ");
        Serial.println(error.c_str());
        http.end();
        return false;
      }
      
      // Extract values from the JSON
      if (doc.containsKey("informationBoardImageUrl")) {
        imageUrl = doc["informationBoardImageUrl"].as<String>();
        Serial.print("Image URL set to: ");
        Serial.println(imageUrl);
      } else {
        Serial.println("Warning: informationBoardImageUrl not found in JSON");
        http.end();
        return false;
      }
      
      if (doc.containsKey("updateIntervalMinutes")) {
        int minutes = doc["updateIntervalMinutes"].as<int>();
        sleepDuration = (uint64_t)minutes * 60 * 1000000; // Convert minutes to microseconds
        Serial.print("Update interval set to: ");
        Serial.print(minutes);
        Serial.println(" minutes");
      } else {
        Serial.println("Warning: updateIntervalMinutes not found in JSON");
        // Keep default sleep duration
      }
      
      http.end();
      return true;
    } else {
      Serial.println("No valid JSON object found in the response");
      http.end();
      return false;
    }
  } else {
    Serial.print("HTTP request failed with code: ");
    Serial.println(httpCode);
    http.end();
    return false;
  }
}

void fetchAndDisplayImage() {
  // Check WiFi connection before making HTTP request
  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("WiFi not connected, cannot fetch image");
    return;
  }
  
  if (imageUrl.length() == 0) {
    Serial.println("Image URL not set, cannot fetch image");
    return;
  }
  
  HTTPClient http;
  http.begin(imageUrl);
  http.setTimeout(30000); // Set 30 second timeout
  http.addHeader("User-Agent", "ESP32");
  
  Serial.print("Starting HTTP GET for image: ");
  Serial.println(imageUrl);
  int httpCode = http.GET();
  Serial.print("HTTP response code: ");
  Serial.println(httpCode);
  
  if (httpCode == HTTP_CODE_OK) {
    int len = http.getSize();
    Serial.print("Content length: ");
    Serial.println(len);
    
    if (len > 0) {
      Serial.print("Free heap before allocation: ");
      Serial.println(ESP.getFreeHeap());
      
      // Check if we have enough memory
      if (ESP.getFreeHeap() < len + 10000) { // Keep 10KB buffer
        Serial.println("Not enough memory to load image");
        http.end();
        return;
      }
      
      uint8_t *buffer = (uint8_t*)malloc(len);
      if (buffer) {
        Serial.print("Allocated ");
        Serial.print(len);
        Serial.println(" bytes for image buffer.");
        
        // Clear both buffers before processing new image - USING PAINT LIBRARY
        Paint_SelectImage(BlackImage);
        Paint_Clear(WHITE);
        Paint_SelectImage(RYImage);
        Paint_Clear(WHITE);
        
        WiFiClient *stream = http.getStreamPtr();
        int totalBytesRead = 0;
        unsigned long timeout = millis() + 30000; // 30 second timeout for reading
        while (totalBytesRead < len && millis() < timeout) {
          int bytesRead = stream->readBytes(buffer + totalBytesRead, len - totalBytesRead);
          if (bytesRead == 0) {
            delay(5); // Short delay if no data available
            if (stream->available() == 0) {
              if (totalBytesRead < len) {
                Serial.println("Stream ended prematurely.");
              }
              break;
            }
          } else {
            totalBytesRead += bytesRead;
          }
        }
        Serial.print("Total bytes read: ");
        Serial.println(totalBytesRead);
        
        if (totalBytesRead == len) {
          // Process and display the image using JPEGDEC
          Serial.println("Decoding JPEG image...");
          
          // Open JPEG image from memory
          if (jpeg.openRAM(buffer, len, jpegDrawCallback)) {
            // Get information about the image
            int jpegWidth = jpeg.getWidth();
            int jpegHeight = jpeg.getHeight();
            Serial.print("JPEG image dimensions: ");
            Serial.print(jpegWidth);
            Serial.print(" x ");
            Serial.println(jpegHeight);
            
            // Decode the image
            if (jpeg.decode(0, 0, 0)) {
              Serial.println("JPEG image decoded successfully");
            } else {
              Serial.println("Error decoding JPEG image");
            }
            
            // Close the file
            jpeg.close();

            // Initialize e-ink display exactly as in Waveshare example
            DEV_Module_Init();
            EPD_7IN5B_V2_Init();
            EPD_7IN5B_V2_Clear();
            DEV_Delay_ms(500);
            
            // Display the processed image - using Waveshare's function
            Serial.println("Sending image to display...");
            EPD_7IN5B_V2_Display(BlackImage, RYImage);
            Serial.println("Image displayed successfully.");
          } else {
            Serial.println("Failed to open JPEG image");
          }
        } else {
          Serial.println("Failed to read entire image.");
        }
        free(buffer);
      } else {
        Serial.println("Failed to allocate buffer!");
      }
    } else {
      Serial.println("Content length unknown or invalid.");
    }
  } else if (httpCode == HTTPC_ERROR_CONNECTION_REFUSED) {
    Serial.println("Connection refused - server may be down");
  } else if (httpCode == HTTPC_ERROR_CONNECTION_LOST) {
    Serial.println("Connection lost during request");
  } else if (httpCode == HTTPC_ERROR_NO_HTTP_SERVER) {
    Serial.println("No HTTP server found");
  } else if (httpCode == HTTPC_ERROR_NOT_CONNECTED) {
    Serial.println("Not connected to server");
  } else {
    Serial.printf("HTTP GET failed, error: %d\n", httpCode);
  }
  
  http.end();
}

void loop() {
  // Empty - using deep sleep instead
}
