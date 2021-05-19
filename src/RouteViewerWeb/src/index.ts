import * as mapboxgl from "mapbox-gl";
import { LinePlotter } from "./LinePlotter";
import {dataSets} from "./DataSets";

(mapboxgl as typeof mapboxgl).accessToken = 'pk.eyJ1IjoiZ3V5cnQiLCJhIjoiY2pwZW1yYzJnMDF1cTN3cGJpY2N1YWU4dSJ9.9AKbNVoVbmagYSkY7ZoKVQ';

export class RouteFinder {

    private map : mapboxgl.Map;
    private linePlotter : LinePlotter;

    initMap(domTarget : string) : mapboxgl.Map {
        let map = new mapboxgl.Map({
            container: domTarget,
            style: 'mapbox://styles/mapbox/streets-v9', // outdoors
            center: [-122.10851, 47.68325],
            zoom: 13
        });
        this.map = map;
        this.linePlotter = new LinePlotter(this.map);
        return map;
    }

    private dataLoader(path : string, callback) {
        let request = new XMLHttpRequest();
        request.open('GET', path, true);
        let self = this;

        request.onreadystatechange = function() {
            if (this.readyState === 4) {
                if (this.status >= 200 && this.status < 400) {
                    let resp = this.responseText;
                    let data = JSON.parse(resp);
                    callback(self.linePlotter, data);
                } else {
                }
            }
        };

        request.send();
        request = null;
    }

    loadData(dataSetList : string[]) {
        for (let dataset of dataSetList) {
            let datasetDetails = dataSets[dataset];
            if (datasetDetails === undefined) {
                alert("Bad dataset: " + dataset);
            }
            this.dataLoader(datasetDetails.path, datasetDetails.callback);
        }
    }

}


function getDataSets() : string[] {
    let url = new URL(window.location.toString());
    let datasets = url.searchParams.get('datasets');
    return datasets.split(',');
}

const r = new RouteFinder();
let map = r.initMap("map");
map.on('load', () => r.loadData(getDataSets()));
