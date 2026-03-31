FROM nginx:alpine

WORKDIR /app

COPY . /app

RUN npm install

EXPOSE 80

CMD ["dotnet", "server.js"]


// docker build .
// docker run ***id*** (then you get a name)

// docker ps -a (for non running containers use -a)
// docker stop ***containername***

// docker run -p (publish) 3000:8080 (localport:dockercotainerport) ***id*** 

// build -t (custom-name)


