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

Install [kind](https://kind.sigs.k8s.io/).

### kubectl

You shouldn't need to install `kubectl` as it is installed by Docker Desktop.

### k9s

Optionally install [k9s](https://k9scli.io/), a sophisticated k8s CLI.

## Create your K8s cluster

Run `createCluster.ps1` in a Powershell console to create your cluster.

```powershell
.\createCluser.ps1 -name {your k8s cluster name}
```

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

# .NET Core Console App Deployment to Kubernetes Checklist

## Pre-Deployment Steps

1. **Prepare the .NET Core Console App:**

   - Ensure the app is configured correctly to run as a console application.
   - Verify that the app has proper logging and error handling mechanisms in place.
   - Configure the app to exit properly after execution.

2. **Containerize the Application:**

   - Create a Dockerfile in the root of the .NET Core Console app.
   - Specify the base image (e.g., `mcr.microsoft.com/dotnet/aspnet:7.0` or the appropriate version).
   - Build the Docker image:
     ```bash
     docker build -t your-app-name:tag .
     ```
   - Test the Docker image locally to ensure it runs correctly:
     ```bash
     docker run --rm your-app-name:tag
     ```

3. **Push Docker Image to Registry:**

   - Tag the Docker image for your container registry:
     ```bash
     docker tag your-app-name:tag your-registry/your-app-name:tag
     ```
   - Push the image to your container registry:
     ```bash
     docker push your-registry/your-app-name:tag
     ```

4. **Create Kubernetes YAML Manifests:**

   - **Job Manifest** (`job.yaml`):
     ```yaml
     apiVersion: batch/v1
     kind: Job
     metadata:
       name: your-app-job
     spec:
       template:
         spec:
           containers:
             - name: your-app-container
               image: your-registry/your-app-name:tag
               command: ["dotnet", "your-app.dll"]
           restartPolicy: OnFailure
     ```
   - **CronJob Manifest** (`cronjob.yaml`):
     ```yaml
     apiVersion: batch/v1
     kind: CronJob
     metadata:
       name: your-app-cronjob
     spec:
       schedule: "0 0 * * *" # Runs every day at midnight
       jobTemplate:
         spec:
           template:
             spec:
               containers:
                 - name: your-app-container
                   image: your-registry/your-app-name:tag
                   command: ["dotnet", "your-app.dll"]
               restartPolicy: OnFailure
     ```

5. **Configuration and Secrets:**

   - If your app requires configuration files, secrets, or environment variables, create Kubernetes ConfigMaps and Secrets.
   - **ConfigMap (optional):**
     ```yaml
     apiVersion: v1
     kind: ConfigMap
     metadata:
       name: your-app-config
     data:
       appsettings.json: |-
         {
           "Key": "Value"
         }
     ```
   - **Secret (optional):**
     ```yaml
     apiVersion: v1
     kind: Secret
     metadata:
       name: your-app-secret
     type: Opaque
     data:
       connectionString: base64encodedstring
     ```

6. **Update CronJob Manifest with Config and Secrets:**
   - Add environment variables or mount volumes in the CronJob manifest as necessary:
     ```yaml
     containers:
       - name: your-app-container
         image: your-registry/your-app-name:tag
         command: ["dotnet", "your-app.dll"]
         env:
           - name: CONNECTION_STRING
             valueFrom:
               secretKeyRef:
                 name: your-app-secret
                 key: connectionString
         volumeMounts:
           - name: config-volume
             mountPath: /app/config
     volumes:
       - name: config-volume
         configMap:
           name: your-app-config
     ```

## Deployment Steps

1. **Deploy ConfigMaps and Secrets (if applicable):**

   - Apply ConfigMaps and Secrets:
     ```bash
     kubectl apply -f configmap.yaml
     kubectl apply -f secret.yaml
     ```

2. **Deploy the CronJob:**

   - Apply the CronJob manifest to the cluster:
     ```bash
     kubectl apply -f cronjob.yaml
     ```

3. **Verify Deployment:**

   - Check that the CronJob is created:
     ```bash
     kubectl get cronjob
     ```
   - Ensure the Job runs at the scheduled time by checking the status:
     ```bash
     kubectl get jobs --watch
     ```

4. **Monitor Logs:**
   - View logs from the running Job to ensure it executed correctly:
     ```bash
     kubectl logs job/your-app-job-name
     ```

## Post-Deployment

1. **Set Up Monitoring and Alerts:**

   - Configure monitoring for the Job, such as using Prometheus or other monitoring tools.
   - Set up alerts to notify you if the Job fails or doesn’t run as expected.

2. **Backup and Disaster Recovery:**

   - Ensure backups are in place for any data that the console app might interact with or generate.
   - Implement a disaster recovery plan in case the job fails multiple times.

3. **Document the Process:**
   - Document the deployment process and configurations for future reference.
   - Include troubleshooting steps for common issues that may arise with the CronJob.
