using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eraser {
    class CtrImageClient : ImageClient {
        public async Task<List<string>> ListAsync()
        {
            var output = await runCommand("image ls -q");
            output = output.Trim('\n');
            var result = new List<string>();
            if (output.Length > 0)
            {
                var images = output.Split('\n');
                foreach (string image in images) {
                    result.Add(image);
                }
            }
            return result;
        }

        public async Task DeleteAsync(string image)
        {
            await runCommand("image rm " + image);
        }


        private async Task<string> runCommand(string args)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ctr",
                    Arguments = "image ls -q",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true,
            };
            
            var completion = new TaskCompletionSource<string>();
            process.Exited += (sender, args) =>
            {
                completion.SetResult(process.StandardOutput.ReadToEnd());
                process.Dispose();
            };
            process.Start();

            return await completion.Task;
        }
    }
}