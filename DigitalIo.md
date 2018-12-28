
## Using DigitalIo


``` csharp
    // initialize basic library
    var io = new DigitalIo();
    if (!io.Initialize())
    {
        Console.WriteLine("Failed to initialize IO");
        return;
    }

    // define convenience inputs and outputs
    var myInput = io.CreateInput(Gpio.Gpio17);
    var myOutput = io.CreateOutput(Gpio.Gpio7);

    // use that
    myOutput.Set(myInput);
``` 
