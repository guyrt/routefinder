import { Map } from "mapbox-gl";

export class LinePlotter {
    private map : Map;

    public constructor(map : Map) {
        this.map = map;
    }

    public plotGeoJson(data, name, lineColor = null, lineWidth = 1) {
        
        let lineColorData : any = lineColor;
        if (lineColor == null) {
            lineColorData = {
                property: 'rfInPolygon',
                type: "categorical",
                default: '#000000',
                stops: [
                    ['in', '#ff0000'],
                    ['notfoot', '#0000ff'],
                    ['out', '#666666']
                ]
            };
        } 
        
        this.map.addLayer({
            id: name,
            type: "line",
            source: {
                type: 'geojson',
                data: data
            },
            paint: {
                "line-color": lineColorData,
                "line-width": lineWidth
            }
        });
    }

}