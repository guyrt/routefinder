import { LinePlotter } from "./LinePlotter";

export let dataSets = {
    labeledWays: {
        path: 'data/labeledWays.json',
        callback: (linePlotter : LinePlotter, data: any) => linePlotter.plotGeoJson(data, 'labeledways')
    },
    cleanPolygon: {
        path: 'data/cleanpolygon.json',
        callback: (linePlotter : LinePlotter, data: any) => linePlotter.plotGeoJson(data, 'cleanPolygon', '#000000')
    },
    reducedGraph: {
        path: 'data/reducedGraph.json',
        callback: (linePlotter : LinePlotter, data: any) => linePlotter.plotGeoJson(data, 'reducedGraph', 'purple', 2)
    },
    lazyRoute: {
        path: 'data/lazyRouteCoverage.json',
        callback: (linePlotter : LinePlotter, data : any) => linePlotter.plotGeoJson(data, 'lazyRouteCoverage', 'brown', 1.5)
    },
    triangles: {
        path: 'data/triangles.json',
        callback: (linePlotter : LinePlotter, data : any) => linePlotter.plotGeoJson(data, 'triangles')
    }
}