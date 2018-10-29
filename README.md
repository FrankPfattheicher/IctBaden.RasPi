IctBaden.RasPi
===============

Basic hardware access library for Raspberry Pi .NET programs

Two versions:

| Name | Description |
| ---- | --- |    
| IctBaden.RasPi.Net40	| .NET Full Framework (Mono), *Raspberry Pi 1 only!* |
| IctBaden.RasPi			  | .NET Core 2.1, no PWM support |

Low Level
---------
* Digital IO (GPIO)  -  [how to use digital IO...](DigitalIo.md)
* I2C

High Level
----------
* LCD connected to I2C (SainSmart IIC LCD1602 Module)
* OneWire temperature sensors (DS18S20)
* PWM (Raspberry Pi 1 only)
