# ServerMocker

Spin up a server with endpoints based on a json DSL

ServerMocker is a utility for subbing out REST endpoint integrations with mock endpoints, but using a real server.
This can be useful in cases where the calling application code can't be changed, 
but it's configuration for target server can be updated to point to this fake service instead.
