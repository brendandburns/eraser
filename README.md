A simple daemon for removing images from machines automatically.

Expects a Kubernetes ConfigMap with a list of images, one image per line.

Running via Docker:

Connecting to dockerd:
```sh
docker run -v $HOME/.kube:/root/.kube \
           -v /var/run/docker.sock:/var/run/docker.sock \
           brendanburns/eraser:v1 --config-map images --container-runtime docker --debug
```

Connecting to containerd:
```sh
docker run -v $HOME/.kube:/root/.kube \
           -v /run/containerd/containerd.sock:/run/containerd/containerd.sock \
           brendanburns/eraser:v1 --config-map images --container-runtime containerd --debug
```



