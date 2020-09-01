using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Docker.DotNet;
using NLog;
using k8s;

namespace Eraser {
    class ImageEraser {
        
        private static readonly Logger log = NLog.LogManager.GetCurrentClassLogger();

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
            var imageSet = new HashSet<String>();
            log.Debug("Loaded config map:");
            log.Debug("-=-=-=-=-=-=-");
            foreach (var img in imageList) {
                log.Debug(img);
                imageSet.Add(img);
            }
            log.Debug("-=-=-=-=-=-=-");

            var images = await client.ListAsync();
            foreach (var image in images)
            {
                if (imageSet.Contains(image))
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

        public static async Task Run(String configMap, String configMapNamespace, String configMapFile, long delayMs, bool debug, String containerRuntime)
        {
            var logConfig = new NLog.Config.LoggingConfiguration();
            var minLevel = debug ? LogLevel.Debug : LogLevel.Info;
            logConfig.AddRule(minLevel, LogLevel.Fatal, new NLog.Targets.ConsoleTarget("eraser"));
            NLog.LogManager.Configuration = logConfig;

            ImageClient client = null;
            if (containerRuntime.Equals("docker"))
            {
                // TODO: Fix this for Windows?
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