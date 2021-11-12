#!/bin/bash

# sets some vars
source /user/local/bin/setenv.sh
echo $ENVS_SET

# blob file 
azcopy copy "https://raweusroutefinder.blob.core.windows.net/rawpbf/na.us.wa.osm.pbf" "/tmp/na.osm.pbf"

echo "blob of incoming pbf done"
ls -lh /tmp

# run osmosis 
# read once, filter, then write two sets of files
# one boundary set
# many split out files for runnable ways.
osmosis --read-pbf-fast file=/tmp/na.osm.pbf workers=8 \
    --tee 2 \
    \
        --tf reject-relations access=private \
        --tf reject-relations landuse=military \
        --tf accept-relations boundary=administrative,national_park \
        --used-way \
        --used-node \
        --write-xml /tmp/boundaries.xml \
    \
        --tf accept-ways highway=track,residential,steps,footpath,footway,path,tertiary,cycleway,primary \
        --tf reject-ways golf=cartpath \
        --tf reject-ways access=permit,private \
        --tf reject-ways service=driveway,parking_aisle \
        --tf reject-ways footway=sidewalk \
        --used-node \
        --tee 8 \
        --bounding-box right=-114 bottom=40.5 completeWays=yes --write-xml /tmp/bbox_1_1.xml \
        --bounding-box left=-114 right=-89.5 bottom=40.5 completeWays=yes --write-xml /tmp/bbox_1_2.xml \
        --bounding-box left=-89.5 right=-77.7 bottom=40.5 completeWays=yes --write-xml /tmp/bbox_1_3.xml \
        --bounding-box left=-77.7 bottom=40.5 completeWays=yes --write-xml /tmp/bbox_1_4.xml \
        --bounding-box top=40.5 right=-114 completeWays=yes --write-xml /tmp/bbox_2_1.xml \
        --bounding-box left=-114 top=40.5 right=-89.5 completeWays=yes --write-xml /tmp/bbox_2_2.xml \
        --bounding-box left=-89.5 top=40.5 right=-77.7 completeWays=yes --write-xml /tmp/bbox_2_3.xml \
        --bounding-box left=-77.7 top=40.5 completeWays=yes --write-xml /tmp/bbox_2_4.xml

# Logs
echo "Done with Osmosis processing with these files"

ls -hl /tmp

# blob output files
az storage blob upload-batch -d rawpbf -s /tmp --pattern b*.xml --destination-path rawxml/

echo "Done with blob"