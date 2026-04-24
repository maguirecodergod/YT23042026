{ pkgs, ... }: {
  channel = "unstable";
  packages = [
    pkgs.dotnet-sdk_10
    pkgs.dotnet-ef
    pkgs.sudo
    pkgs.icu
    pkgs.openssl
    pkgs.docker
    pkgs.docker-compose
    pkgs.tree
  ];
  env = { };
  services.docker.enable = true;
  idx = {
    extensions = [
      "jsw.csharpextensions"
      "ms-dotnettools.vscode-dotnet-runtime"
      "DotJoshJohnson.xml"
      "eamodio.gitlens"
      "ms-azuretools.vscode-containers"
      "ms-azuretools.vscode-docker"
      "patcx.vscode-nuget-gallery"
      "PKief.material-icon-theme"
      "redhat.vscode-yaml"
      "techer.open-in-browser"
      "zxh404.vscode-proto3"
      "humao.rest-client"
      "ric-v.postgres-explorer"
      "dotnetdev-kr-custom.csharp"
    ];
  };
}