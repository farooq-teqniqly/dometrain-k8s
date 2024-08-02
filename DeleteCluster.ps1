param (
    [Parameter(mandatory=$true)]
    [string]$name
)

$confirmation = Read-Host "Are you sure you want to delete the cluster $name? (y/n)"

if ($confirmation.ToLowerInvariant() -ne "y") {
    exit
}

kind delete cluster --name $name