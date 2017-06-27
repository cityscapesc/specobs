function BingMap(credentials, mapControl) {
    var map = null;
    var searchManager = null;
    var bingSpatialDataServiceBaseUrl = "https://platform.bing.com/geo/spatial/v1/public/Geodata?SpatialFilter=";
    var safeCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-";
    var mapCredentials = credentials;
    var mapDiv = mapControl;
    var geoLocationProvider = null;

    var self = this;
    self.deferred = $.Deferred();

    this.SearchRequestType = {
        geocodeRequest: 1,
        searchRequest: 2,
        reverseGeocodeRequest: 3
    };

    var CreateSearchManager = function () {
        map.addComponent('searchManager', new Microsoft.Maps.Search.SearchManager(map));
        searchManager = map.getComponent('searchManager');
    }

    this.LoadMap = (function () {
        if (!map) {
            Microsoft.Maps.loadModule('Microsoft.Maps.Themes.BingTheme', {
                callback: function () {
                    map = GetMapObject();
                    self.deferred.resolve();
                }
            });
        }
    }());


    var GetMapObject = function () {
        return new Microsoft.Maps.Map(mapDiv, {
            credentials: mapCredentials,
            mapTypeId: Microsoft.Maps.MapTypeId.road,
            theme: new Microsoft.Maps.Themes.BingTheme(),
            enableSearchLogo: false,
            enableClickableLogo: false,
            showMapTypeSelector: false,
            //labelOverlay: Microsoft.Maps.LabelOverlay.hidden
        });
    }

    self.UnloadMap = function () {
        if (map) {
            map.dispose();
        };
    };

    self.ZoomtoLocation = function (latitude, longitude, zoomlevel) {
        var Loc = new Microsoft.Maps.Location(latitude, longitude);
        var viewOptions = { center: Loc, zoom: zoomlevel };

        if (!map) {
            this.LoadMap();
        }
        else {
            map.setView(viewOptions);
        }
    };

    this.locationFinder = function (request, searchRequestType) {
        registerSerachModule(request, searchRequestType);
    };

    var registerSerachModule = function (requestData, searchRequestType) {
        if (!map) {
            this.LoadMap();
        }

        Microsoft.Maps.loadModule('Microsoft.Maps.Search', {
            callback: function () {
                switch (searchRequestType) {
                    case self.SearchRequestType.searchRequest:
                        searchRequest(requestData)
                        break;
                    case self.SearchRequestType.geocodeRequest:
                        geocodeRequest(requestData)
                        break;
                    case self.SearchRequestType.reverseGeocodeRequest:
                        reverseGeocodeRequest(requestData);
                        break;
                }
            }
        });
    };


    var searchRequest = function (request) {
        loadSearchManagerModule();

        searchManager.search(request);
    };

    var geocodeRequest = function (request) {
        loadSearchManagerModule();

        request.bounds = map.getBounds();

        searchManager.geocode(request);
    };

    var reverseGeocodeRequest = function (request) {
        loadSearchManagerModule();

        searchManager.reverseGeocode(request);
    };

    self.addPushPin = function (location, pushpinOptions) {

        var pushpin = new Microsoft.Maps.Pushpin(location, pushpinOptions);

        map.entities.push(pushpin);

        return pushpin;
    };

    var loadSearchManagerModule = function () {
        if (!searchManager) {
            map.addComponent('searchManager', new Microsoft.Maps.Search.SearchManager(map));
            searchManager = map.getComponent('searchManager');
        }
    };

    self.ClearMapEntities = function () {
        map.entities.clear();
    };

    self.viewchangeend = function (event) {
        var viewchangeend = Microsoft.Maps.Events.addHandler(map, 'viewchangeend', function (e) {
            event(e);
        });
    };

    self.getMapCenter = function () {
        return map.getCenter();
    };

    self.getMapZoom = function () {
        return map.getZoom();
    };

    self.getBoundries = function () {
        return map.getBounds();
    };

    self.BoundingBoxView = function (locationArray) {
        var boundingBox = Microsoft.Maps.LocationRect.fromLocations(locationArray);
        map.setView({ zoom: 16, bounds: boundingBox });
    }

    self.addInfoBox = function (infoBox) {
        map.entities.push(infoBox);
    }

    self.DisplayInfobox = function (e) {
        if (e.targetType == 'pushpin') {            
            //var pushpin = e.target;
            //var pinInfobox = pushpin._infobox;
            //pushpin.setOptions({ state: Microsoft.Maps.EntityState.selected, visible: true, offset: new Microsoft.Maps.Point(0, 5000) });
            //pinInfobox.setLocation(pushpin.getLocation());

            //A buffer limit to use to specify the infobox must be away from the edges of the map.
            //var buffer = 25;

            //var infoboxOffset = pinInfobox.getOffset();
            //var infoboxAnchor = pinInfobox.getAnchor();
            //var infoboxLocation = map.tryLocationToPixel(e.target.getLocation(), Microsoft.Maps.PixelReference.control);

            //var dx = (infoboxLocation.x + infoboxOffset.x) - (infoboxAnchor.x);
            //var dy = infoboxLocation.y - 25 - infoboxAnchor.y;

            //if (dy < buffer) {    //Infobox overlaps with top of map.
            //    //Offset in opposite direction.
            //    dy *= -1;

            //    //add buffer from the top edge of the map.
            //    dy += buffer;
            //} else {
            //    //If dy is greater than zero than it does not overlap.
            //    dy = 0;
            //}

            //if (dx < buffer) {    //Check to see if overlapping with left side of map.
            //    //Offset in opposite direction.
            //    dx *= -1;

            //    //add a buffer from the left edge of the map.
            //    dx += buffer;
            //} else {              //Check to see if overlapping with right side of map.
            //    dx = map.getWidth() - infoboxLocation.x + infoboxAnchor.x - pinInfobox.getWidth();

            //    //If dx is greater than zero then it does not overlap.
            //    if (dx > buffer) {
            //        dx = 0;
            //    } else {
            //        //add a buffer from the right edge of the map.
            //        dx -= buffer;
            //    }
            //}

            ////Adjust the map so infobox is in view
            //if (dx != 0 || dy != 0) {
            //    map.setView({ centerOffset: new Microsoft.Maps.Point(dx, dy), center: map.getCenter() });
            //}
        }
    }
}