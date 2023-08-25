# Gyroscopic vehicle dynamics meter

System for measuring the dynamic characteristics of a car on a three-axis accelerometer/gyroscope.

Passive system that does not require connection to the car's on-board computer, it is enough to install it rigidly inside the cabin.

After turning on, the system begins to receive data on accelerations and turns in real time, on the basis of which it generates relative data on movement, turn, speed and acceleration. 
Data is transferred to a PC via USB connection (COM-port emulation) and processed by a control application. 
Further, the data is displayed in a graphical form with the ability to save the log, open previously taken measurements and calculate the dynamic characteristics

After turning on the system, the car performs maneuvers (acceleration, braking, etc.) during which data is recorded.  
The period of time that the tester is interested in can be selected (either for the current measurement or after opening previously saved logs of earlier measurements).
After that, based on the data on the movements, data on the dynamics of the car are calculated (distance traveled, speed, acceleration, engine speed, power, etc.)

## Functions:

* Real-time 3-axis (-X, -Y, -Z) detection: 
  - turning
  - acceleration
* Calculation of dynamic characteristics for a certain period of time (acceleration/deceleration of car)
  - traveled distance
  - real speed
  - acceleration
  - engine speed (based on predetermined gear ratio)
  - power (based on predetermined weight of car)
* Graphical display of all measured and calculated values
* Saving logs and opening previously saved data 
* Comparison between several test (same car or different cars) 

## Main system parameters:
* Range of angular rate sensors (X-, Y-, and Z-Axis)		±250, ±500, ±1000, ±2000°/sec
* Range of triple-axis accelerometer (X-, Y-, and Z-Axis)	±2g, ±4g, ±8g, ±16g
* Shock tolerant 						10 000 g 
* Connection from sensor to controller				I2C bus
* Connection from to controller to PC				USB (COM-port emulation)

## Control Application:

## Main functions of control application

## Components

## Wiring diagram 

## Possible further improvements
