using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using NLog;
using k8s;

namespace Eraser
{
    class Program
    {
        private static readonly Logger log = NLog.LogManager.GetCurrentClassLogger();

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
            cmd.Handler = CommandHandler.Create<String, String, String, long, bool, String>(Run);
            
            return await cmd.InvokeAsync(args);
        }

        static bool shouldRemove(String image, String[] imageList)
        {
            foreach (var img in imageList) {
                if (img.Equals(image)) {
                    return true;
                }
            }
            return false;
        }

        static async Task scanAndDeleteImages(ImageClient client, IKubernetes k8s, String name, String ns, String key)
        {
            // TODO: Watch here?
            var configMap = await k8s.ReadNamespacedConfigMapAsync(name, ns);
            if (configMap.Metadata.Name == null) {
                log.Error("ConfigMap is missing: " + name + "/" + ns);
                return;
            }
            if (configMap.Data == null || !configMap.Data.ContainsKey(key)) {
                log.Error("ConfigMap file is missing: " + key);
                return;
            }
            var imageList = configMap.Data[key].Trim('\n').Split("\n");
            log.Debug("Loaded config map:");
            log.Debug("-=-=-=-=-=-=-");
            foreach (var img in imageList) {
                log.Debug(img);
            }
            log.Debug("-=-=-=-=-=-=-");

            var images = await client.ListAsync();
            foreach (var image in images)
            {
                if (shouldRemove(image, imageList))
                {
                    await client.DeleteAsync(image);
                    log.Info("Removed image " + image);
                }
                else
                {
                    log.Debug("Skipped image " + image);
                }
            }
        }

        static async Task Run(String configMap, String configMapNamespace, String configMapFile, long delayMs, bool debug, String containerRuntime)
        {
            var logConfig = new NLog.Config.LoggingConfiguration();
            var minLevel = debug ? LogLevel.Debug : LogLevel.Info;
            logConfig.AddRule(minLevel, LogLevel.Fatal, new NLog.Targets.ConsoleTarget("eraser"));
            NLog.LogManager.Configuration = logConfig;

            ImageClient client = null;
            if (containerRuntime.Equals("docker"))
            {
                var uri = new Uri("unix:///var/run/docker.sock");
                var docker = new DockerClientConfiguration(uri).CreateClient();
                client = new DockerImageClient(docker);
            }
            else if (containerRuntime.Equals("containerd"))
            {
                // TODO: Use the gRPC API instead of 'ctr' ?
                client = new CtrImageClient();
            }
            else
            {
                log.Error("Unknown runtime: " + containerRuntime);
            }

            var config = KubernetesClientConfiguration.BuildDefaultConfig();
            var k8s  = new Kubernetes(config);

            while (true) {
                log.Info("Scanning for images");
                await scanAndDeleteImages(client, k8s, configMap, configMapNamespace, configMapFile);
                await Task.Delay(60000);
            }
        }
    }
}
