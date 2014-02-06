/// <reference path="../../Framework/Signum.Web/Signum/Scripts/globals.ts"/>
define(["require", "exports", "Framework/Signum.Web/Signum/Scripts/Entities", "Framework/Signum.Web/Signum/Scripts/Navigator", "Framework/Signum.Web/Signum/Scripts/Operations", "Framework/Signum.Web/Signum/Scripts/Validator"], function(require, exports, Entities, Navigator, Operations, Validator) {
    function cloneWithData(operationKey, prefix, urlData, urlClone) {
        var modelPrefix = SF.compose(prefix, "New");
        Navigator.viewPopup(Entities.EntityHtml.withoutType(modelPrefix), {
            controllerUrl: urlData
        }).then(function (eHtml) {
            if (eHtml == null)
                return;

            var values = Validator.getFormValuesHtml(eHtml);

            Operations.constructFromDefault({
                prefix: prefix,
                operationKey: operationKey,
                controllerUrl: urlClone,
                isLite: true,
                requestExtraJsonData: $.extend({ modelPrefix: modelPrefix }, values)
            });
        });
    }
    exports.cloneWithData = cloneWithData;

    function createAlbumFromBand(options, urlModel, urlOperation) {
        var modelPrefix = SF.compose(options.prefix, "New");
        Navigator.viewPopup(Entities.EntityHtml.withoutType(modelPrefix), {
            controllerUrl: urlModel
        }).then(function (eHtml) {
            if (eHtml == null)
                return;

            var values = Validator.getFormValuesHtml(eHtml);

            options.controllerUrl = urlOperation;
            options.requestExtraJsonData = $.extend({ modelPrefix: modelPrefix }, values);
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
