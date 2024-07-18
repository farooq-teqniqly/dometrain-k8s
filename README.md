# Dometrain Kubernetes (K8s) Course

https://courses.dometrain.com/courses/take/from-zero-to-hero-kubernetes-for-developers

## Prerequisites

### Visual Studio Code
1. Install [Visual Studio Code](https://code.visualstudio.com/download)
2. Install the **Kubernetes** extension for Visual Studio Code.

### Docker Desktop
Install [Docker Desktop](https://docs.docker.com/desktop/install/windows-install/) as it is needed for the Kind tool explained in the next section.

### kind
`kind` is a tool for running local Kubernetes clusters using Docker container “nodes”.
kind was primarily designed for testing Kubernetes itself, but may be used for local development or CI.

1. Install [kind](https://kind.sigs.k8s.io/).
2. Add the folder containing `kind.exe` to your system's `PATH` environment variable.

### kubectl
You shouldn't need to install `kubectl` as it is installed by Docker Desktop.

## Create your K8s cluster
Use `kind` to easily create the K8s cluster used in this course.

```powershell
kind create cluster --name dometrain-k8s-cluster
```

## Verify K8s cluster

### Using kind
Verify the cluster is running using kind.

```powershell
kubectl cluster-info --context kind-dometrain-k8s-cluster
```

### Using Visual Studio Code
1. Open the Kubernetes extension.
2. The cluster should appear in the **Clusters** section.
3. Expand the cluster to see the various objects.