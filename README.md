# Dometrain Kubernetes (K8s) Course

https://courses.dometrain.com/courses/take/from-zero-to-hero-kubernetes-for-developers

## Prerequisites

### Visual Studio Code
1. Install [Visual Studio Code](https://code.visualstudio.com/download)
2. Install the **Kubernetes** extension for Visual Studio Code.

### Docker Desktop
Install [Docker Desktop](https://docs.docker.com/desktop/install/windows-install/) and ensure it is running as it is needed for the Kind tool explained in the next section.

### kind
`kind` is a tool for running local Kubernetes clusters using Docker container “nodes”.
kind was primarily designed for testing Kubernetes itself, but may be used for local development or CI.

Install [kind](https://kind.sigs.k8s.io/).

### kubectl
You shouldn't need to install `kubectl` as it is installed by Docker Desktop.

### k9s
Optionally install [k9s](https://k9scli.io/), a sophisticated k8s CLI.  

## Create your K8s cluster
Run `createCluster.ps1` in a Powershell console to create your cluster.

```powershell
.\createCluser.ps1 -name {your k8s cluster name}
````

## Verify K8s cluster

### Using kind
Verify the cluster is running using kind.

```powershell
kubectl cluster-info --context kind-{your k8s cluster name}
```
### Using Visual Studio Code
1. Open the Kubernetes extension.
2. The cluster should appear in the **Clusters** section.
3. Expand the cluster to see the various objects.

## Install NGINX Ingress Controller
```powershell
.\InstallIngressController.ps1
```

## Cleanup
Delete the cluster by running the following in a Poweshell console:

```powershell
.\deleteCluster.ps1 -name {your k8s cluster name}
```
