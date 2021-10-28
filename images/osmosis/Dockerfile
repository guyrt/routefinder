FROM openjdk:latest

LABEL author="guyrt"

ARG OSMOSIS_URL="https://github.com/openstreetmap/osmosis/releases/download/0.48.3/osmosis-0.48.3.tgz"
ENV OSMOSIS_URL $OSMOSIS_URL

RUN set -x \
  && useradd -ms /bin/bash osmosis \
  && mkdir -p /opt/osmosis \
  && curl -L $OSMOSIS_URL --output /opt/osmosis/osmosis.tgz \
  && ls /opt/osmosis \
  && tar -xzf /opt/osmosis/osmosis.tgz -C /opt/osmosis \
  && ls /opt/osmosis \
  && ln -s /opt/osmosis/bin/osmosis /usr/local/bin/osmosis

USER osmosis
CMD ["osmosis"]