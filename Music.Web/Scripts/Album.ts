/// <reference path="../../Framework/Signum.Web/Signum/Scripts/globals.ts"/>

import Entities = require("Framework/Signum.Web/Signum/Scripts/Entities")
import Navigator = require("Framework/Signum.Web/Signum/Scripts/Navigator")
import Finder = require("Framework/Signum.Web/Signum/Scripts/Finder")
import Operations = require("Framework/Signum.Web/Signum/Scripts/Operations")
import Validator = require("Framework/Signum.Web/Signum/Scripts/Validator")

export function cloneWithData(event: MouseEvent, operationKey: string, prefix: string, vlb: Navigator.ValueLineBoxOptions, urlClone: string) {
    Navigator.valueLineBox(vlb).then(newName => {
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

export function createAlbumFromBand(options: Operations.EntityOperationOptions, event: MouseEvent, urlModel: string, urlOperation: string) {

    var modelPrefix = options.prefix.child("New");
    getModelData(modelPrefix, urlModel).then(modelData=> {
        if (modelData == null)
            return;

        options.controllerUrl = urlOperation;
        options.requestExtraJsonData = $.extend({ modelPrefix: modelPrefix }, modelData);
        Operations.constructFromDefault(options, event);
    });
}

export function createAlbumFromBandContextual(options: Operations.OperationOptions, event: MouseEvent, urlModel: string, urlOperation: string) {

    var modelPrefix = options.prefix.child("New");
    getModelData(modelPrefix, urlModel).then(modelData=> {
        if (modelData == null)
            return;


        options.controllerUrl = urlOperation;
        options.requestExtraJsonData = $.extend({ modelPrefix: modelPrefix }, modelData);
        Operations.constructFromDefaultContextual(options, event);
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

export function createGreatestHitsAlbum(options: Operations.OperationOptions, event: MouseEvent,  url: string) {
    options.controllerUrl = url;
    Operations.constructFromManyDefault(options, event);
}