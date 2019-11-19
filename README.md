# TestProxyKit
Reproduce the localhost bug
The requirement for testing projects is to use proxykit to proxy access to the web front end and support hot loading

# start web client
 1. cd web
  2. yarn install umi -g
  3. yarn install
  4. yarn start 
  5. browse the  http://localhost:8000  the web client start ok;

# start mvc
  1. open DemoUmiExtensions.cs  file
  2. UseUmiProxy function  change target param to  http://localhost:8000
  3. reproduce the bug
