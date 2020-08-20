using System.Threading.Tasks;
using System.Collections.Generic;

namespace Eraser {
    interface ImageClient {
        public Task<List<string>> ListAsync();
        public Task DeleteAsync(string image);

    }
}