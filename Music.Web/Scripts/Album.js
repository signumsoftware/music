/// <reference path="../../Framework/Signum.Web/Signum/Scripts/globals.ts"/>
define(["require", "exports", "Framework/Signum.Web/Signum/Scripts/Entities", "Framework/Signum.Web/Signum/Scripts/Navigator", "Framework/Signum.Web/Signum/Scripts/Operations", "Framework/Signum.Web/Signum/Scripts/Validator"], function(require, exports, Entities, Navigator, Operations, Validator) {
    function cloneWithData(event, operationKey, prefix, vlb, urlClone) {
        Navigator.valueLineBox(vlb).then(function (newName) {
            if (!newName)
                return;

            Operations.constructFromDefault({
                prefix: prefix,
                operationKey: operationKey,
                controllerUrl: urlClone,
                isLite: true,
                requestExtraJsonData: { newName: newName }
            }, event);
        });
    }
    exports.cloneWithData = cloneWithData;

    function createAlbumFromBand(options, event, urlModel, urlOperation) {
        var modelPrefix = options.prefix.child("New");
        exports.getModelData(modelPrefix, urlModel).then(function (modelData) {
            if (modelData == null)
                return;

            options.controllerUrl = urlOperation;
            options.requestExtraJsonData = $.extend({ modelPrefix: modelPrefix }, modelData);
            Operations.constructFromDefault(options, event);
        });
    }
    exports.createAlbumFromBand = createAlbumFromBand;

    function createAlbumFromBandContextual(options, event, urlModel, urlOperation) {
        var modelPrefix = options.prefix.child("New");
        exports.getModelData(modelPrefix, urlModel).then(function (modelData) {
            if (modelData == null)
                return;

            options.controllerUrl = urlOperation;
            options.requestExtraJsonData = $.extend({ modelPrefix: modelPrefix }, modelData);
            Operations.constructFromDefaultContextual(options, event);
        });
    }
    exports.createAlbumFromBandContextual = createAlbumFromBandContextual;

    function getModelData(modelPrefix, urlModel) {
        return Navigator.viewPopup(Entities.EntityHtml.withoutType(modelPrefix), {
            controllerUrl: urlModel
        }).then(function (eHtml) {
            if (!eHtml)
                return null;
            return Validator.getFormValuesHtml(eHtml);
        });
    }
    exports.getModelData = getModelData;

    function createGreatestHitsAlbum(options, event, url) {
        options.controllerUrl = url;
        Operations.constructFromManyDefault(options, event);
    }
    exports.createGreatestHitsAlbum = createGreatestHitsAlbum;
});
//# sourceMappingURL=Album.js.map
