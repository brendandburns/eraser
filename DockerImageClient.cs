
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Eraser {
    class DockerImageClient : ImageClient {
        private DockerClient docker;

        public DockerImageClient(DockerClient docker) {
            this.docker = docker;
        }

        public async Task<List<string>> ListAsync() {
            var images = await docker.Images.ListImagesAsync(new ImagesListParameters());
            List<string> result = new List<string>();

            foreach (var image in images)
            {
                foreach (var tag in image.RepoTags)
                {
                    result.Add(tag);
                }
            }
            return result;
        }

        public async Task DeleteAsync(string image) {
            await docker.Images.DeleteImageAsync(image, new ImageDeleteParameters());
        }
    }
}