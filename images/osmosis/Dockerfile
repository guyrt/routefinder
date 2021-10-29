FROM ubuntu:latest
#FROM openjdk:18

LABEL author="guyrt"

ARG OSMOSIS_URL="https://github.com/openstreetmap/osmosis/releases/download/0.48.3/osmosis-0.48.3.tgz"
ENV OSMOSIS_URL $OSMOSIS_URL

# Install java
RUN apt-get update

RUN apt-get -y install default-jre && which java

# Install .NET SDK
RUN apt-get -y install wget

RUN wget "https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb" -O "packages-microsoft-prod.deb"
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb

RUN apt-get install -y apt-transport-https && \
  apt-get update && \
  apt-get install -y aspnetcore-runtime-5.0


RUN set -x \
  && useradd -ms /bin/bash osmosis \
  && mkdir -p /opt/osmosis \
  && wget $OSMOSIS_URL -O "/opt/osmosis/osmosis.tgz" \
  && tar -xzf /opt/osmosis/osmosis.tgz -C /opt/osmosis \
  && ln -s /opt/osmosis/bin/osmosis /usr/local/bin/osmosis

RUN mkdir -p /opt/azcopy \
  && wget "https://aka.ms/downloadazcopy-v10-linux" -O "/tmp/azcopy.tgz" \
  && tar -xzf /tmp/azcopy.tgz -C /tmp \
  && mv /tmp/azcopy_linux*/* /opt/azcopy \
  && ln -s /opt/azcopy/azcopy /usr/local/bin/azcopy \
  && chmod 777 /opt/azcopy/azcopy

RUN rm -rf /tmp/azcopy*

# Cleanup from apt update
RUN rm -rf /var/lib/apt/lists/*

# Get your driver file
COPY "osmosis_driver.sh" "/usr/local/bin/osmosis_driver.sh"

USER osmosis
CMD ["osmosis_driver.sh"]