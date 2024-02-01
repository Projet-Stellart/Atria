FROM lilian1024/godot-mono

WORKDIR /app

#Copy source code and prepare files
COPY . ./sc
RUN mkdir ./builds

#Build project
RUN godot "./sc/project.godot" --export-debug "Linux Docker" /app/builds/server

#Remove source code and Godot editor
RUN rm -rf ./sc
RUN ungodot

EXPOSE 7308/udp

ENTRYPOINT /app/builds/server --headless --server