# Setup

## Resources
https://www.jeffgeerling.com/blog/2019/mounting-kubernetes-secret-single-file-inside-pod

## Create docker image

Update docker tag in confixDeploymentTest.csproj

Build image
```powershell
dotnet publish --os linux --arch x64 /t:PublishContainer -c Release
```
Push to registry:
```
docker push swisslifef2c/deployment-test:X.X.X
```

Update tag also in k8s/deployment.yml


## Apply k8s resource files

Use [kind](https://kind.sigs.k8s.io/docs/user/quick-start/#installing-with-go-install) as playground for k8s. 


Create namespace and use it as default
```
k create ns confix
k config set-context --current --namespace=confix
```

Deploy secret

```
k apply -f k8s/secret.yml
```

Deploy deployment
```
k apply -f k8s/deployment.yml
```