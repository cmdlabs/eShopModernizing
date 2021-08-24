# Setup

## Get EC2 Windows Instance

* Provision EC2 Windows box using Windows_Server-2019-English-Full-ContainersLatest-2021.08.11
  * Default vpc, custom SG
  * New keypair
  * 60GB disk

## Install chocolatey

```
Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
```

## Install git

    choco install git -y

## install dotnet 5.0 sdk

    choco install dotnet-5.0-sdk -y

### Install visual studio community

Depends on MSBuild, and VS2019 is recommend, so:

    choco install visualstudio2019community -y

## Clone eShopModernizing

    git clone https://github.com/dotnet-architecture/eShopModernizing.git