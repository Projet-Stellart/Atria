FROM lilian1024/godot-mono

RUN [ "useradd", "appuser", "--create-home" ]

WORKDIR /home/appuser/

#Copy source code and prepare files
COPY . ./sc
RUN mkdir ./builds

#Build project
RUN godot "./sc/project.godot" --export-debug "Linux Docker" /home/appuser/builds/server

#Remove source code and Godot editor
RUN rm -rf ./sc
RUN ungodot

EXPOSE 7308/udp

ENTRYPOINT su appuser -c "./builds/server.sh --headless --server --port 7308"