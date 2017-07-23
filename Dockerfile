FROM resin/raspberrypi3-node
MAINTAINER Craig Mulligan <craig@resin.io>
ENV INITSYSTEM on

ENV DEBIAN_FRONTEND noninteractive

# native deps for electron
RUN apt-get update && apt-get upgrade
RUN apt-get install -yq --no-install-recommends \
    chromium-browser \
    x11-xserver-utils \
    unclutter
RUN apt-get clean && rm -rf /var/lib/apt/lists/*
RUN npm i -g npm t
#RUN nano ~/.config/lxsession/LXDE-pi/autostart

RUN mkdir -p /usr/src/app && ln -s /usr/src/app /app
COPY package.json /usr/src/app/package.json
WORKDIR /usr/src/app
#RUN JOBS=MAX npm install --unsafe-perm

COPY . /usr/src/app

CMD ["xinit", "/usr/src/app/launch_app.sh", "--kiosk", "--", "-nocursor"]