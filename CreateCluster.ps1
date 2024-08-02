param (
    [Parameter(mandatory=$true)]
    [string]$name,

    [Parameter(mandatory=$false)]
    [string]$config = "KindCluster.yaml"
)

kind create cluster --name $name --config $config
