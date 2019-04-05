Data notes
----------

TODO: automate download (easy to get single file)

Key problem: how to find trails you care about?
Thoughts:
* Start with a region: I want Cougar Mountain State Park.
* Find all trails/roads in the park.
* Keep anything that interesects park. 


TODOs:
- cut graph - find optimal route for all p2p and mark edges used as "in"
- compute the expansion (postman)
- find eulerian circuits

- Missing a few nodes. Redownload.

- https://en.wikipedia.org/wiki/Route_inspection_problem
- http://www.highcube.org/a-single-route-for-every-atlanta-road


Graph notes
-----------

Data Cleanup Notes
------------------

Need to allow crossing parking lots.
Edits:
* Connected Big Tree Ridge Trail to the road
* Get https://www.openstreetmap.org/api/0.6/map?bbox=-122.1817%2C47.4843%2C-121.9927%2C47.558 when you can.

why is 53188016 missing?
- zeroed everything out
- add back in ones on the MST that connects it back in.

is 42108700 in
- special cased it :/
