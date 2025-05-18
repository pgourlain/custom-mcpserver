

FAQ :
help urls 
- https://devblogs.microsoft.com/dotnet/build-a-model-context-protocol-mcp-server-in-csharp/#publish-your-mcp-server
- https://github.com/jamesmontemagno/monkeymcp
- 

to debug
npx @modelcontextprotocol/inspector dotnet run -v q --no-build
in vscode : you can use sse mcpserver

in claude :
{
    "mcpServers": {
        "my-mcp-local": {                       
            "command": "docker",
            "args" : [
                "run",
                "-i",
                "--rm",
                "trucmcp"
            ]
        }
    }
}
to publish into local docker
ensure 'Use containerd for pulling and storing images' is check (under Features in development)
from mcp stdio directory
```shell
dotnet publish /t:PublishContainer
```


//seems not working
{
    "mcpServers": {
        "my-mcp-local": {                       
            "command": "/usr/local/share/dotnet/dotnet",
            "args" : [
                "run",
                "--no-build",
                "--project",
                "/Users/genius/Documents/sources/github/custom-mcpserver/stdio/custom-stdio-mcpserver.csproj"
            ]
        }
    }
}