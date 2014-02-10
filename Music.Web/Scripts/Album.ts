/// <reference path="../../Framework/Signum.Web/Signum/Scripts/globals.ts"/>

import Entities = require("Framework/Signum.Web/Signum/Scripts/Entities")
import Navigator = require("Framework/Signum.Web/Signum/Scripts/Navigator")
import Finder = require("Framework/Signum.Web/Signum/Scripts/Finder")
import Operations = require("Framework/Signum.Web/Signum/Scripts/Operations")
import Validator = require("Framework/Signum.Web/Signum/Scripts/Validator")

export function cloneWithData(operationKey: string, prefix : string, urlData: string, urlClone :string) {
    var modelPrefix = SF.compose(prefix, "New");
    Navigator.viewPopup(Entities.EntityHtml.withoutType(modelPrefix), {
        controllerUrl: urlData
    }).then(eHtml => {
            if (eHtml == null)
                return;

            var values = Validator.getFormValuesHtml(eHtml);

            Operations.constructFromDefault({
                prefix: prefix,
                operationKey: operationKey,
                controllerUrl : urlClone,
                isLite: true,
                requestExtraJsonData: $.extend({ modelPrefix: modelPrefix }, values)
            });
        });
}

export function createAlbumFromBand(options: Operations.EntityOperationOptions, urlModel: string, urlOperation: string) {

    var modelPrefix = SF.compose(options.prefix, "New");
    getModelData(modelPrefix, urlModel).then(modelData=> {
        if (modelData == null)
            return;

        options.controllerUrl = urlOperation;
        options.requestExtraJsonData = $.extend({ modelPrefix: modelPrefix }, modelData);
        Operations.constructFromDefault(options);
    });
}

export function createAlbumFromBandContextual(options: Operations.OperationOptions, urlModel: string, urlOperation: string) {

    var modelPrefix = SF.compose(options.prefix, "New");
    getModelData(modelPrefix, urlModel).then(modelData=> {
        if (modelData == null)
            return;


        options.controllerUrl = urlOperation;
        options.requestExtraJsonData = $.extend({ modelPrefix: modelPrefix }, modelData);
        Operations.constructFromDefaultContextual(options);
    });
}

export function getModelData(modelPrefix: string, urlModel: string): Promise<FormObject> {
    return Navigator.viewPopup(Entities.EntityHtml.withoutType(modelPrefix), {
        controllerUrl: urlModel
    }).then(eHtml=> {
            if (!eHtml)
                return null;
            return Validator.getFormValuesHtml(eHtml);
        });
}

export function createGreatestHitsAlbum(options: Operations.OperationOptions, url: string) {
    options.controllerUrl = url;
    Operations.constructFromManyDefault(options); 
}