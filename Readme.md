In progress notes
------------------


System setup: the ADF flow for prepping data
* Azure Data Factory to download raw pbf file
* Azure batch to run osmosis in a single job/task that will pre-process data. Saves multiple data files in azure blob. 
* Each file is compared against the region-wide "boundaries" file. Run Azure Batches to process. This is the expensive/long running task but memory requirements are fairly low so cheap machines can be used.



try:
[copy the data locally from Azure...]
docker run -d -p 80:80 osmosis:latest osmosis --blah blah blah
[move data back to azure]
[write queue events for each file]