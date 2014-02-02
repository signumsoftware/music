/// <reference path="../../Framework/Signum.Web/Signum/Scripts/globals.ts"/>
define(["require", "exports", "Framework/Signum.Web/Signum/Scripts/Entities", "Framework/Signum.Web/Signum/Scripts/Navigator", "Framework/Signum.Web/Signum/Scripts/Operations"], function(require, exports, Entities, Navigator, Operations) {
    function cloneWithData(operationKey, prefix, urlData, urlClone) {
        var modelPrefix = SF.compose(prefix, "New");
        Navigator.viewPopup(Entities.EntityHtml.withoutType(modelPrefix), {
            controllerUrl: urlData
        }).then(function (eHtml) {
            if (eHtml == null)
                return;

            Operations.constructFromDefault({
                prefix: prefix,
                operationKey: operationKey,
                controllerUrl: urlClone,
                isLite: true,
                requestExtraJsonData: $.extend({ modelPrefix: modelPrefix }, eHtml.html.serializeObject())
            });
        });
    }
    exports.cloneWithData = cloneWithData;

    function createAlbumFromBand(options, urlModel, urlOperation) {
        var modelPrefix = SF.compose(options.prefix, "New");
        Navigator.viewPopup(Entities.EntityHtml.withoutType(options.prefix), {
            controllerUrl: urlModel
        }).then(function (eHtml) {
            if (eHtml == null)
                return;

            options.controllerUrl = urlOperation;
            options.requestExtraJsonData = $.extend({ modelPrefix: modelPrefix }, eHtml.html.serializeObject());
            Operations.constructFromDefault(options);
        });
    }
    exports.createAlbumFromBand = createAlbumFromBand;

    function createAlbumFromBandContextual(options, urlModel, urlOperation) {
        var modelPrefix = SF.compose(options.prefix, "New");
        Navigator.viewPopup(Entities.EntityHtml.withoutType(options.prefix), {
            controllerUrl: urlModel
        }).then(function (eHtml) {
            if (eHtml == null)
                return;

            options.controllerUrl = urlOperation;
            options.requestExtraJsonData = $.extend({ modelPrefix: modelPrefix }, eHtml.html.serializeObject());
            Operations.constructFromDefaultContextual(options);
        });
    }
    exports.createAlbumFromBandContextual = createAlbumFromBandContextual;

    function createGreatestHitsAlbum(options, url) {
        options.controllerUrl = url;
        Operations.constructFromManyDefault(options);
    }
    exports.createGreatestHitsAlbum = createGreatestHitsAlbum;
});
//# sourceMappingURL=Album.js.map
