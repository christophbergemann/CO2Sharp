# CO2Sharp
C# solution for reading a [low cost CO2 sensor](https://www.amazon.de/dp/B00TH3OW4Q).
This is based on the Python-based solution [office-weather](https://github.com/wooga/office_weather),
from where I have taken the decryption logic.
This solution however depended on the `fcntl` Python module that is not available on Windows.
This project should be platform-independent but I only tested it on Windows.

## Usage
Running the `Main` method in the `Program.cs` file will regularly print results to the console.
The `Co2SensorController` class contains all the logic. It starts a background thread to read
the data that currently cannot be interrupted. This thread updates the `Temperature` and `Co2`
properties of the controller instance. 



