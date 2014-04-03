var map, quakes, isoseimals, distance, myGeometry;

require(["esri/map", "esri/symbols/SimpleMarkerSymbol", "esri/symbols/SimpleLineSymbol", "esri/renderers/ClassBreaksRenderer",
         "esri/dijit/Scalebar", "dojo/_base/json", "dojo/_base/array", "dojo/domReady!", "esri/geometry"],
function (Map, MarkerSymbol, LineSymbol, ClassBreaksRenderer, Scalebar, dojo, array) {

    map = new Map("map", {
        basemap: "gray",
        center: [174, -41],
        zoom: 6,
        showAttribution: true
    });

    isoseimals = new esri.layers.GraphicsLayer();
    map.addLayer(isoseimals);

    quakes = new esri.layers.GraphicsLayer();
    map.addLayer(quakes);

    var me = new esri.layers.GraphicsLayer();
    map.addLayer(me);

    distance = new esri.layers.GraphicsLayer();
    map.addLayer(distance);
    
    var symbol = new MarkerSymbol(MarkerSymbol.STYLE_CIRCLE, 30,
        new LineSymbol(LineSymbol.STYLE_SOLID,
        new dojo.Color([255, 0, 0, 1]), 7),
        new dojo.Color([255, 0, 0, 0.7]));
    var renderer = new ClassBreaksRenderer(symbol, "magnitude");
    renderer.addBreak(0, 1.99, new MarkerSymbol(MarkerSymbol.STYLE_CIRCLE, 10,
    new LineSymbol(LineSymbol.STYLE_SOLID,
    new dojo.Color([192, 255, 192, 1]), 2),
    new dojo.Color([192, 255, 192, 0.7])));
    renderer.addBreak(2, 2.99, new MarkerSymbol(MarkerSymbol.STYLE_CIRCLE, 14,
    new LineSymbol(LineSymbol.STYLE_SOLID,
    new dojo.Color([76, 255, 76, 1]), 2),
    new dojo.Color([76, 255, 76, 0.7])));
    renderer.addBreak(3, 3.99, new MarkerSymbol(MarkerSymbol.STYLE_CIRCLE, 16,
    new LineSymbol(LineSymbol.STYLE_SOLID,
    new dojo.Color([255, 255, 70, 1]), 3),
    new dojo.Color([255, 255, 70, 0.7])));
    renderer.addBreak(4, 5.29, new MarkerSymbol(MarkerSymbol.STYLE_CIRCLE, 20,
    new LineSymbol(LineSymbol.STYLE_SOLID,
    new dojo.Color([255, 140, 20, 1]), 4),
    new dojo.Color([255, 140, 20, 0.7])));
    renderer.addBreak(5.3, 6.99, new MarkerSymbol(MarkerSymbol.STYLE_CIRCLE, 24,
    new LineSymbol(LineSymbol.STYLE_SOLID,
    new dojo.Color([255, 55, 15, 1]), 5),
    new dojo.Color([255, 55, 15, 0.7])));
    renderer.addBreak(7, 8.99, new MarkerSymbol(MarkerSymbol.STYLE_CIRCLE, 28,
    new LineSymbol(LineSymbol.STYLE_SOLID,
    new dojo.Color([255, 0, 0, 1]), 6),
    new dojo.Color([255, 0, 0, 0.7])));
    quakes.setRenderer(renderer);
    
    map.on("load", function (evt) {
        map.resize();
        var scalebar = new Scalebar({ map: map, scalebarUnit: "metric" });

        for (var i = 0; i < data.length; i++) {
            quakes.add(new esri.Graphic(data[i]));
        }

        //Check if browser supports W3C Geolocation API
        if (navigator.geolocation)
            navigator.geolocation.getCurrentPosition(getCurrentPositionSuccess);        
    });

    function getCurrentPositionSuccess(position) {

        var meSymbol = new MarkerSymbol();
        meSymbol.setPath("M16,3.5c-4.142,0-7.5,3.358-7.5,7.5c0,4.143,7.5,18.121,7.5,18.121S23.5,15.143,23.5,11C23.5,6.858,20.143,3.5,16,3.5z M16,14.584c-1.979,0-3.584-1.604-3.584-3.584S14.021,7.416,16,7.416S19.584,9.021,19.584,11S17.979,14.584,16,14.584z");
        meSymbol.setColor(new dojo.Color([0, 0, 0, 1]));
        meSymbol.setOutline(null);
        meSymbol.setOffset(0, 12);
        myGeometry = new esri.geometry.Point(position.coords.longitude, position.coords.latitude);

        me.add(new esri.Graphic(myGeometry, meSymbol));
    }
});

function show(id) {

    require(["dojo/_base/array", "dojo/_base/xhr", "esri/symbols/SimpleFillSymbol", "esri/symbols/SimpleLineSymbol", "esri/symbols/TextSymbol", "esri/symbols/Font", "esri/config"],
        function (array, xhr, FillSymbol, LineSymbol, TextSymbol, Font) {

            var lineSymbol = new LineSymbol();
            var font = new Font();
            font.setSize("10pt");
            font.setWeight(Font.WEIGHT_BOLD);

            isoseimals.clear();
            distance.clear();

            var filteredArr = array.filter(quakes.graphics, function (existingGraphic) {
                return existingGraphic.attributes["publicid"] == id;
            });

            if (myGeometry != null) {

                var polyline = new esri.geometry.Polyline(new esri.SpatialReference({ wkid: 4326 }));
                polyline.addPath([myGeometry, filteredArr[0].geometry]);

                var graphic = new esri.Graphic(esri.geometry.geodesicDensify(polyline, 10000), lineSymbol);
                distance.add(graphic);

                var lengths = esri.geometry.geodesicLengths([polyline], esri.Units.KILOMETERS);
                var text = new TextSymbol(parseFloat(lengths).toFixed(2) + "km from your current location");
                text.setOffset(130, -6);
                text.setFont(font);
                distance.add(new esri.Graphic(filteredArr[0].geometry, text));
            }
            
            map.centerAt(filteredArr[0].geometry);

            xhr.get({
                url: "/eq?id=" + id + "&X=" + filteredArr[0].geometry.x + "&Y=" + filteredArr[0].geometry.y + "&Depth=" + filteredArr[0].attributes["depth"] + "&Magnitude=" + filteredArr[0].attributes["magnitude"],
                handleAs: "json",
                headers: { "Content-Type": "application/json" },
                load: function (rings) {

                    var symbol = new FillSymbol();
                    symbol.setOutline(new esri.symbol.SimpleLineSymbol(esri.symbol.SimpleLineSymbol.STYLE_SOLID, new dojo.Color([100, 100, 255, 0.6]), 2));
                    symbol.setColor(new dojo.Color([0, 0, 0, 0]));
                    array.forEach(rings, function (item, i) {

                        var g = new esri.Graphic(item);
                        var area = esri.geometry.geodesicAreas([g.geometry], esri.Units.KILOMETERS);
                        if (area > 0) {
                            g.setSymbol(symbol);
                            isoseimals.add(g);
                        }
                    });
                }
            });
        });
}