﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Signum.Web;
using Signum.Utilities;
using System.Reflection;
using Signum.Test;
using Signum.Web.Operations;
using System.Web.Mvc;
using Signum.Entities.Processes;
using Music.Test;
using Signum.Test.Environment;

namespace Music.Web
{
    public static class MusicClient
    {
        public static string ViewPrefix = "~/Views/Music/{0}.cshtml";

        public static void Start()
        {
            if (Navigator.Manager.NotDefined(MethodInfo.GetCurrentMethod()))
            {
                Navigator.AddSettings(new List<EntitySettings>
                {
                    new EntitySettings<AlbumDN>() { PartialViewName = e => ViewPrefix.Formato("Album") },
                    new EntitySettings<ArtistDN>() { PartialViewName = e => ViewPrefix.Formato("Artist") },
                    new EntitySettings<BandDN>() { PartialViewName = e => ViewPrefix.Formato("Band") },
                    
                    new EntitySettings<GrammyAwardDN>() { PartialViewName = e => ViewPrefix.Formato("GrammyAward") },
                    new EntitySettings<AmericanMusicAwardDN>() { PartialViewName = e => ViewPrefix.Formato("AmericanMusicAward") },
                    new EntitySettings<PersonalAwardDN>() { PartialViewName = e => ViewPrefix.Formato("PersonalAward") },
                    
                    new EntitySettings<LabelDN>() { PartialViewName = e => ViewPrefix.Formato("Label") },
                    new EmbeddedEntitySettings<SongDN>() { PartialViewName = e => ViewPrefix.Formato("Song")},

                    new EntitySettings<NoteWithDateDN>() { PartialViewName = e => ViewPrefix.Formato("NoteWithDate") },

                    new EmbeddedEntitySettings<AlbumFromBandModel>(){PartialViewName = e => ViewPrefix.Formato("AlbumFromBandModel")},
                });

                QuickLinkWidgetHelper.RegisterEntityLinks<LabelDN>((entity, partialViewName, prefix) =>
                {
                    if (entity.IsNew)
                        return null;

                    return new QuickLink[]
                    {
                        new QuickLinkFind(typeof(AlbumDN), "Label", entity, true)
                    };
                });

                ButtonBarEntityHelper.RegisterEntityButtons<AlbumDN>((ctx, entity) =>
                {
                    if (entity.IsNew)
                        return null;

                    return new ToolBarButton[]
                    {
                        new ToolBarButton
                        {
                            DivCssClass = ToolBarButton.DefaultEntityDivCssClass,
                            Id = TypeContextUtilities.Compose(ctx.Prefix, "CloneWithData"),
                            Text = "Clone with data",
                            OnClick = new JsOperationConstructorFrom(new JsOperationOptions
                            { 
                                ControllerUrl = RouteHelper.New().Action<MusicController>(mc => mc.CloneWithData(Js.NewPrefix(ctx.Prefix))),
                                Prefix = ctx.Prefix
                            }).ajax(Js.NewPrefix(ctx.Prefix), JsOpSuccess.OpenPopupNoDefaultOk).ToJS()
                        }
                    };
                });

                OperationsClient.AddSettings( new List<OperationSettings>
                {
                    new EntityOperationSettings(AlbumOperation.CreateAlbumFromBand)
                    { 
                        OnClick = ctx => new JsOperationConstructorFrom(ctx.Options<MusicController>(mc => mc.CreateAlbumFromBand(Js.NewPrefix(ctx.Prefix))))
                            .ajax(Js.NewPrefix(ctx.Prefix), JsOpSuccess.OpenPopupNoDefaultOk),

                        Contextual = 
                        {
                            OnClick = ctx => new JsOperationConstructorFrom(ctx.Options<MusicController>(mc => mc.CreateAlbumFromBand(Js.NewPrefix(ctx.Prefix))))
                                .ajax(Js.NewPrefix(ctx.Prefix), JsOpSuccess.OpenPopupNoDefaultOk)
                        },
                    },
                    new ContextualOperationSettings(AlbumOperation.CreateGreatestHitsAlbum)
                    {
                        OnClick = ctx => new JsOperationConstructorFromMany(ctx.Options<MusicController>(mc => mc.CreateGreatestHitsAlbum()))
                            .submitSelected()
                    },
                });
            }
        }
    }
}
