public  class GreenhouseService
{
    public void ExecuteCommand(string device, string command)
    {
        switch (device.ToLower())
        {
            case "window":
                if (command == "open")
                {
                    Console.WriteLine("Fönstret öppnas.");
                }
                else if (command == "close")
                {
                    Console.WriteLine("Fönstret stängs.");
                }
                break;

            case "watering":
                if (command == "start")
                {
                    Console.WriteLine("Bevattningen startas.");
                }
                else if (command == "stop")
                {
                    Console.WriteLine("Bevattningen stoppas.");
                }
                break;

            default:
                Console.WriteLine($"Okänt kommando för enheten {device}");
                break;
        }
    }
}
