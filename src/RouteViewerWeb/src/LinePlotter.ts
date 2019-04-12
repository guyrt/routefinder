import { Map } from "mapbox-gl";

export class LinePlotter {
    private map : Map;

    public constructor(map : Map) {
        this.map = map;
    }

    public plotGeoJsonColorByPolygon(data, name) {
        let lineColorData = {
            property: 'rfInPolygon',
            type: "categorical",
            default: '#000000',
            stops: [
                ['in', '#ff0000'],
                ['notfoot', '#0000ff'],
                ['out', '#666666']
            ]
        };
        this.innerPlotGeoJson(data, name, lineColorData, 1);
    }

    public plotGeoJsonColorByCoverage(data, name) {
        let lineColorData = {
            property: 'EdgeWeight',
            type: "categorical",
            default: '#000000',
            stops: [
                ['1', 'blue'],
                ['2', 'red'],
                ['3', 'orange'],
                ['4', 'black'],
            ]
        };

        let lineWidthData = {
            property: 'EdgeWeight',
            type: "categorical",
            default: 10,
            stops: [
                ['1', 1],
                ['2', 4],
                ['3', 6],
                ['4', 8],
            ]
        };
        this.innerPlotGeoJson(data, name, lineColorData, lineWidthData);
    }

    public plotGeoJson(data, name, lineColor) {
        this.innerPlotGeoJson(data, name, lineColor, 1)
    }

    private innerPlotGeoJson(data, name, lineColorData, lineWidthData) {

        this.map.addLayer({
            id: name,
            type: "line",
            source: {
                type: 'geojson',
                data: data
            },
            paint: {
                "line-color": lineColorData,
                "line-width": lineWidthData
            }
        });
    }

}