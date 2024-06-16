FROM barichello/godot-ci:mono-4.2.1

RUN wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb

RUN apt-get update
RUN apt-get install -y dotnet-sdk-7.0

RUN cp -a /root/.local/share/godot/templates/. ~/.local/share/godot/export_templates/

RUN [ "useradd", "appuser", "--create-home" ]

WORKDIR /home/appuser/

#Copy source code and prepare files
COPY . ./sc
RUN mkdir ./builds
COPY ./serverParams.json ./builds/server/serverParams.json

#Build project
RUN godot "./sc/project.godot" --headless --export-debug "Linux Docker" /home/appuser/builds/server.sh

#Remove source code and Godot editor
RUN rm -rf ./sc

EXPOSE 7308/udp

ENTRYPOINT su appuser -c "./builds/server.sh --headless --server"