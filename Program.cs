using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Eraser
{
    class Program
    {
        public async static Task<int> Main(String[] args)
        {
            var cmd = new RootCommand
            {
                new Option<String>(
                    "--config-map",
                    getDefaultValue: () => "",
                    description: "Name of the configuration map to load"),
                new Option<String>(
                    "--config-map-namespace",
                    getDefaultValue: () => "default",
                    description: "Namespace of the configuration map to load"),
                new Option<String>(
                    "--config-map-file",
                    getDefaultValue: () => "images.txt",
                    description: "Name of file within the config map to load."),
                new Option<long>(
                    "--delay-ms",
                    getDefaultValue: () => 60 * 1000,
                    description: "The length of time to sleep between scans"
                ),
                new Option<bool>(
                    "--debug",
                    getDefaultValue: () => false,
                    description: "Enable debug logging"
                ),
                new Option<String>(
                    "--container-runtime",
                    getDefaultValue: () => "docker",
                    description: "The container runtime to connect to. Options 'docker' or 'containerd'. Default is 'docker'"
                )
            };
            cmd.Description = "A utility for removing images from local disk";
            cmd.Handler = CommandHandler.Create<String, String, String, long, bool, String>(ImageEraser.Run);
            
            return await cmd.InvokeAsync(args);
        }
    }
}
