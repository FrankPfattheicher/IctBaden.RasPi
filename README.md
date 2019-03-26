IctBaden.RasPi
===============

Basic hardware access library for Raspberry Pi .NET programs

Two versions:

| Name | Description |
| ---- | --- |    
| IctBaden.RasPi.Net40	| .NET Full Framework (Mono), *hardare (DMA) PWM on Raspberry Pi 1 only!* |
| IctBaden.RasPi		| .NET Core 2.1, soft PWM support only |

Low Level
---------
* Digital IO (GPIO)  -  [how to use digital IO...](DigitalIo.md)
* I2C

High Level
----------
* LCD connected to I2C (SainSmart IIC LCD1602 Module)
* OLED connected to I2C - [how to use...](OledDisplay.md)
* OneWire temperature sensors (DS18S20)
* PWM (DMA based on Raspberry Pi 1 only, soft PWM on Raspberry Pi 2 up)
