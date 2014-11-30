using System;
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
using Signum.Entities.Basics;

namespace Music.Web
{
    public static class MusicClient
    {
        public static string ViewPrefix = "~/Views/Music/{0}.cshtml";
        public static JsModule Module = new JsModule("Album");

        public static void Start()
        {
            if (Navigator.Manager.NotDefined(MethodInfo.GetCurrentMethod()))
            {
                Navigator.AddSettings(new List<EntitySettings>
                {
                    new EntitySettings<AlbumEntity>() { PartialViewName = e => ViewPrefix.FormatWith("Album") },
                    new EntitySettings<ArtistEntity>() { PartialViewName = e => ViewPrefix.FormatWith("Artist") },
                    new EntitySettings<BandEntity>() { PartialViewName = e => ViewPrefix.FormatWith("Band") },
                    
                    new EntitySettings<GrammyAwardEntity>() { PartialViewName = e => ViewPrefix.FormatWith("GrammyAward") },
                    new EntitySettings<AmericanMusicAwardEntity>() { PartialViewName = e => ViewPrefix.FormatWith("AmericanMusicAward") },
                    new EntitySettings<PersonalAwardEntity>() { PartialViewName = e => ViewPrefix.FormatWith("PersonalAward") },
                    
                    new EntitySettings<LabelEntity>() { PartialViewName = e => ViewPrefix.FormatWith("Label") },
                    new EmbeddedEntitySettings<SongEntity>() { PartialViewName = e => ViewPrefix.FormatWith("Song")},

                    new EntitySettings<NoteWithDateEntity>() { PartialViewName = e => ViewPrefix.FormatWith("NoteWithDate") },

                    new EmbeddedEntitySettings<AlbumFromBandModel>(){PartialViewName = e => ViewPrefix.FormatWith("AlbumFromBandModel")},
                });

                LinksClient.RegisterEntityLinks<LabelEntity>((entity, ctx) => new[]
                {
                    new QuickLinkExplore(typeof(AlbumEntity), "Label", entity)
                });

                ButtonBarEntityHelper.RegisterEntityButtons<AlbumEntity>((ctx, entity) =>
                {
                    if (entity.IsNew)
                        return null;

                    return new ToolBarButton[]
                    {
                        new ToolBarButton(ctx.Prefix, "CloneWithData")
                        {
                            Text = "Clone with data",
                            OnClick = Module["cloneWithData"](JsFunction.Event, AlbumOperation.Clone.Symbol.Key, ctx.Prefix, 
                                new ValueLineBoxOptions(ValueLineType.TextBox, ctx.Prefix, "New") { title = "New name",  message = "Write new album's name", labelText = "Name"},
                                ctx.Url.Action((MusicController mc)=>mc.Clone()))
                        }
                    };
                });

                OperationClient.AddSettings(new List<OperationSettings>
                {
                    new EntityOperationSettings<BandEntity>(AlbumOperation.CreateAlbumFromBand)
                    { 
                        Click = ctx => Module["createAlbumFromBand"](ctx.Options(), JsFunction.Event,
                            ctx.Url.Action((MusicController mc)=>mc.CreateAlbumFromBandModel()), 
                            ctx.Url.Action((MusicController mc)=>mc.CreateAlbumFromBandExecute())),

                        Contextual = 
                        {
                            Click = ctx => Module["createAlbumFromBandContextual"](ctx.Options(), JsFunction.Event,
                                ctx.Url.Action((MusicController mc)=>mc.CreateAlbumFromBandModel()), 
                                ctx.Url.Action((MusicController mc)=>mc.CreateAlbumFromBandExecute())),
                        },
                    },
                    new ContextualOperationSettings<AlbumEntity>(AlbumOperation.CreateGreatestHitsAlbum)
                    {
                        Click = ctx =>Module["createGreatestHitsAlbum"](ctx.Options(),  JsFunction.Event,
                            ctx.Url.Action((MusicController mc)=>mc.CreateGreatestHitsAlbum()))
                    },
                });
            }
        }
    }
}
