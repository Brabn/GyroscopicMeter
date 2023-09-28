/* 
Monitoring of linear and angular movements 
based on the MPU6050 digital accelerometer and Arduino controller

Version 1.21 from 08/25/15*/


#include <Wire.h>
#include <Kalman.h>

int AverageTime=2;
boolean logging=true;
int timeint=100;
long timing=0;
long lasttiming=0;
String inputString;
Kalman kalmanX;
Kalman kalmanY;
Kalman kalmanZ;
uint8_t IMUAddress = 0x68;
 
/* IMU Data */
int16_t accX;
int16_t accY;
int16_t accZ;
int16_t tempRaw;
int16_t gyroX;
int16_t gyroY;
int16_t gyroZ;
 
double accXangle; // Angle calculate using the accelerometer
double accYangle;
double accZangle;
double temp;
double gyroXangle = 180; // Angle calculate using the gyro
double gyroYangle = 180;
double gyroZangle = 180;
double compAngleX = 180; // Calculate the angle using a Kalman filter
double compAngleY = 180;
double compAngleZ = 180;
double kalAngleX; // Calculate the angle using a Kalman filter
double kalAngleY;
double kalAngleZ; 

double ZeroXangle=180;
double ZeroYangle=180;
double ZeroToXangle;
double ZeroToYangle;

double accY01Zero;
double acc01Zero;
double acc02Zero;
double acc03Zero;

double acc01;
double acc02;
double acc03;
double accY01;
double speedZero;
double distZero;
double LastAccSum;
int counter=0;

double angZeroFiltred;
double accY01Filtred;
double acc01Filtred;
double acc02Filtred;
double acc03Filtred;

uint32_t timer;
byte Tock=0;

boolean Zeroed=false;
class Average
{
  public:
  double LastValues[50];
  double AverageValue;
  double AverageSum;
  int avermode;
  int num;
  Average(int mode)
  {
   AverageValue=0;
   AverageSum=0;
   num=0;
   avermode=mode;
  }
  void Clear()
  {
       for (int i=0; i<=50; i++)
      {
         LastValues[i]=0;
         AverageValue=0;
   AverageSum=0;
   num=0;

      }
  }
  double GetAverage()
  {
     if (avermode==1)
     {
     for (int i=0; i<=50; i++)
      {
         AverageSum+=LastValues[i];
      }
    AverageValue=(double)AverageSum/(double)50;
    AverageSum=0;
     }
    
    return AverageValue;
  }
  void AddValue(double newValue)
  {
    LastValues[num++]=newValue;
    
    if (num>=50) num=0;
  }
};

Average accY01A(1);
Average acc01A(1);
Average acc02A(1);
Average acc03A(1);
void setup() {
 
  Serial.begin(115200);
  Wire.begin();
  Serial.begin(9600);
            
 
  i2cWrite(0x6B,0x00); // Disable sleep mode      
  kalmanX.setAngle(180); // Set starting angle
  kalmanY.setAngle(180);
  kalmanZ.setAngle(180);
  timer = micros();
   //setZeroAngle();
   Zeroed=false;
   lasttiming=0;
}
 
void loop() {
  /* Update all the values */
  uint8_t* data = i2cRead(0x3B,14);
  accX = ((data[0] << 8) | data[1]);
  accY = ((data[2] << 8) | data[3]);
  accZ = ((data[4] << 8) | data[5]);
  tempRaw = ((data[6] << 8) | data[7]);
  gyroX = ((data[8] << 8) | data[9]);
  gyroY = ((data[10] << 8) | data[11]);
  gyroZ = ((data[12] << 8) | data[13]);
 
  /* Calculate the angls based on the different sensors and algorithm */
  accYangle = (atan2(accX,accZ)+PI)*RAD_TO_DEG;
  accXangle = (atan2(accY,accZ)+PI)*RAD_TO_DEG;  
  //accZangle = (atan2(accX,accY)+PI)*RAD_TO_DEG;  
  
  double gyroXrate = (double)gyroX/131.0;
  double gyroYrate = -((double)gyroY/131.0);
  double gyroZrate = ((double)gyroZ/131.0);
  
  gyroXangle += kalmanX.getRate()*((double)(micros()-timer)/1000000); // Calculate gyro angle using the unbiased rate
  gyroYangle += kalmanY.getRate()*((double)(micros()-timer)/1000000);
  gyroZangle += kalmanZ.getRate()*((double)(micros()-timer)/1000000);
 
  kalAngleX = kalmanX.getAngle(accXangle, gyroXrate, (double)(micros()-timer)/1000000); // Calculate the angle using a Kalman filter
  kalAngleY = kalmanY.getAngle(accYangle, gyroYrate, (double)(micros()-timer)/1000000);
 accY01=((float)accY/16384);
 acc01=((float)accX/16384);
 acc02=pow(pow(((float)acc01),2)+pow(((float)accY01),2),0.5);
 acc03=((float)accX/16384)*cos(ZeroToYangle/RAD_TO_DEG);
if (Zeroed)
{
 ZeroToXangle=kalAngleX-ZeroXangle;   // угол X оси по отношению к плоскости измерения
 ZeroToYangle=kalAngleY-ZeroYangle;   // угол Н оси по отношению к плоскости измерения
 //accZeroX1=pow(pow(((float)accX/16384)-sin((kalAngleY-180)/RAD_TO_DEG),2)+pow(((float)accY/16384)-sin((kalAngleX-180)/RAD_TO_DEG),2),0.5);   // отнимаем ускорение свободного падения
 //accZeroX1=((float)accX/16384);
 //accZeroX1=((float)accX/16384)-sin((kalAngleY-180)/RAD_TO_DEG);
 /*accY01=((float)accY/16384);
 acc01=((float)accX/16384);
 acc02=pow(pow(((float)accX/16384),2)+pow(((float)accY/16384),2),0.5);
 acc03=((float)accX/16384)*cos(ZeroToYangle/RAD_TO_DEG);*/

 //accZeroX2=accZeroX1*cos(ZeroToYangle/RAD_TO_DEG);  //Проецируем на плоскость измерения

 //accZeroX2=accZeroX1;
 if (Tock==0)
 {
 accY01A.AddValue(accY01-accY01Zero);
 acc01A.AddValue(acc01-acc01Zero);
 acc02A.AddValue(acc02-acc02Zero);
 acc03A.AddValue(acc03-acc03Zero);
 /* acc01A.AddValue(acc01);
 acc02A.AddValue(acc02);
 acc03A.AddValue(acc03);*/
 Tock++;
 counter++;
 }
 else  
 {
 Tock++;
 if (Tock>=AverageTime) Tock=0;
 }
}
 timer = micros();
 if (logging)
    if ((millis()-timing)>timeint)
    {
      
      //if (Zeroed)
      {
      //angZeroFiltred=ang0A.GetAverage();
      accY01Filtred=accY01A.GetAverage();
      acc01Filtred=acc01A.GetAverage();
      acc02Filtred=acc02A.GetAverage();
      acc03Filtred=acc03A.GetAverage();
      acc02Filtred=pow(pow(((float)acc01Filtred),2)+pow(((float)accY01Filtred),2),0.5);
      speedZero+=(float)(acc01Filtred*9.8)*((float)(timing-lasttiming)/1000)*3.6;   // Speed calculation in m/s=>km/h
      distZero+=(float)((float)speedZero/3.6)*((float)(timing-lasttiming)/1000)/1000;   // Distance calculation in m=>km
      }

    Serial.print(millis());
    Serial.print(";");
   // Serial.print("X:");
    Serial.print(kalAngleX-180,0);
    Serial.print(";");
  
    //Serial.print("Y:");
    Serial.print(kalAngleY-180,0);
    Serial.print(";");
        Serial.print(gyroXrate,3);
    Serial.print(";");
        Serial.print(gyroYrate,3);
    Serial.print(";");

    Serial.print((float)accX/16384,3);
    Serial.print(";");
    //Serial.print("accY:");
    Serial.print((float)accY/16384,3);
    Serial.print(";");
    //Serial.print("accZ:");
    Serial.print((float)accZ/16384,3);
    Serial.print(";");

            //Serial.print("accZeroX1:");
    //Serial.print((float)angZeroFiltred,1);    
    //Serial.print(";");
    Serial.print((float)distZero,3);      
    Serial.print(";");
    Serial.print((float)speedZero,3);   
    Serial.print(";");
        //Serial.print("accZeroX1:");
    Serial.print((float)accY01Filtred,4);
    
    Serial.print(";");
            //Serial.print("accZeroX2:");
    Serial.print((float)acc01Filtred,4);
    Serial.print(";");
            //Serial.print("accZeroX2:");
    Serial.print((float)acc02Filtred,4);
    Serial.print(";");
            //Serial.print("accZeroX2:");
    Serial.print((float)acc03Filtred,4);

    Serial.println(";");
    lasttiming=timing;
    timing=millis();
    
    }
    
  
  // The accelerometer's maximum samples rate is 1kHz
}
void i2cWrite(uint8_t registerAddress, uint8_t data){
  Wire.beginTransmission(IMUAddress);
  Wire.write(registerAddress);
  Wire.write(data);
  Wire.endTransmission(); // Send stop
}
uint8_t* i2cRead(uint8_t registerAddress, uint8_t nbytes) {
  uint8_t data[nbytes];
  Wire.beginTransmission(IMUAddress);
  Wire.write(registerAddress);
  Wire.endTransmission(false); // Don't release the bus
  Wire.requestFrom(IMUAddress, nbytes); // Send a repeated start and then release the bus after reading
  for(uint8_t i = 0; i < nbytes; i++)
    data [i]= Wire.read();
  return data;
}

void(* resetFunc) (void) = 0;

void serialEvent() 
{
   while (Serial.available()) 
   {
    char inChar = (char)Serial.read();
    if (inChar == ';') 
    {
      RunCommand(inputString);
      inputString="";
    }
    else
    {
      inputString += inChar;
    }
  }
}
void  setZeroAngle()
{
  speedZero=0;
  distZero=0;
   ZeroXangle=kalAngleX;
   ZeroYangle=kalAngleY;
   accY01Zero=accY01;
   acc01Zero=acc01;
   acc02Zero=acc02;
   acc03Zero=acc03;
   Zeroed=true;
}
void RunCommand(String command)
{
  if ((command.substring(0,3)).equals("ti="))
  {
    timeint=command.substring(3).toInt();
    //Serial.print("Set timeint=");
   // Serial.println(timeint);
  } 
  else  
  if ((command.substring(0,3)).equals("at="))
  {
    AverageTime=command.substring(3).toInt();
    AverageTime=AverageTime*0.270/50;
    //Serial.print("Set timeint=");
   // Serial.println(timeint);
  } 
  if (command.equals("run"))
  {
    //int val=command.substring(3).toInt();
    //Serial.println("command: set t=true;");
    logging=true;
  }else
  if (command.equals("stop"))
  {
    //int val=command.substring(3).toInt();
    //Serial.println("command: set t=false;");
    logging=false;
  }
  if (command.equals("zero"))
  {
    //int val=command.substring(3).toInt();
    //Serial.println("command: set t=false;");
    //SetBeginingState();
    setZeroAngle();
  }
  if (command.equals("reset"))
  {
    
   
  Serial.begin(115200);
  Wire.begin();
  Serial.begin(9600);
            
 
  i2cWrite(0x6B,0x00); // Disable sleep mode      
  kalmanX.setAngle(180); // Set starting angle
  kalmanY.setAngle(180);
  kalmanZ.setAngle(180);
  Zeroed=false;
  acc01Zero=0;
  acc02Zero=0;
  acc03Zero=0;
  accY01Zero=0;
  ZeroToYangle=0;
  timer = micros();
   //setZeroAngle();
   Zeroed=false;
    resetFunc();
  }
}
