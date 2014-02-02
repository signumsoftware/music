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

            Operations.constructFromDefault({
                prefix: prefix,
                operationKey: operationKey,
                controllerUrl : urlClone,
                isLite: true,
                requestExtraJsonData: $.extend({ modelPrefix: modelPrefix }, eHtml.html.serializeObject())
            });
        });
}

export function createAlbumFromBand(options: Operations.EntityOperationOptions, urlModel: string, urlOperation: string) {

    var modelPrefix = SF.compose(options.prefix, "New");
    Navigator.viewPopup(Entities.EntityHtml.withoutType(options.prefix), {
        controllerUrl: urlModel
    }).then(eHtml=> {
        if (eHtml == null)
            return;

        options.controllerUrl = urlOperation;
        options.requestExtraJsonData = $.extend({ modelPrefix: modelPrefix }, eHtml.html.serializeObject());
        Operations.constructFromDefault(options);
    }); 
}

export function createAlbumFromBandContextual(options: Operations.OperationOptions, urlModel: string, urlOperation: string) {

    var modelPrefix = SF.compose(options.prefix, "New");
    Navigator.viewPopup(Entities.EntityHtml.withoutType(options.prefix), {
        controllerUrl: urlModel
    }).then(eHtml=> {
            if (eHtml == null)
                return;

            options.controllerUrl = urlOperation;
            options.requestExtraJsonData = $.extend({ modelPrefix: modelPrefix }, eHtml.html.serializeObject());
            Operations.constructFromDefaultContextual(options);
        });
}

export function createGreatestHitsAlbum(options: Operations.OperationOptions, url: string) {
    options.controllerUrl = url;
    Operations.constructFromManyDefault(options); 
}