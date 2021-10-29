#!/bin/bash

# we'll use azcopy for all of this, because we can azcopy with a service principal.
# to use, create SP and give Azure Batch that role.

# copy file 
azcopy copy "https://raweusroutefinder.blob.core.windows.net/rawpbf/na.us.wa.osm.pbf" "/tmp/na.osm.pbf"

# run osmosis 
# read once, filter, then write two sets of files
# one boundary set
# many split out files for runnable ways.
osmosis --read-pbf-fast file=/tmp/na.osm.pbf workers=8 \
    --tf accept-ways highway=track,residential,footpath,footway,path,service,tertiary,cycleway,primary \
    --tf reject-ways golf=cartpath \
    --tf reject-ways access=permit,private \
    --tf reject-ways service=driveway,parking_aisle \
    --tf accept-relations boundary=administrative \
    --tf reject-relations access=private \
    --used-node \
    --write-xml /tmp/highway.xml

# Logs
echo "Done with Osmosis processing with these files"

ls -hl /tmp

# copy output files

