
## Using DigitalIo


``` csharp
    // define GPOI numbers to be used as digital inputs
    var inputs = new[] { /* GPIO */ 17, 27, 22, 18 };
    // define GPOI numbers to be used as digital outputs
    var outputs = new[] { /* GPIO */ 7, 8, 9, 10, 11, 23, 24, 25 };
    // initialize basic library
    var io = new DigitalIo(inputs, outputs);
    if (!io.Initialize())
    {
        Console.WriteLine("Failed to initialize IO");
        return;
    }

    // define convenience inputs and outputs
    var myInput = new Input(io, 0);
    var myOutput = new Output(io, 0);

    // user that
    myOutput.Set(myInput);
``` 
