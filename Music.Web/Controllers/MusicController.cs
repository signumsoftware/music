using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using Signum.Test;
using Signum.Web.Operations;
using Signum.Engine;
using Signum.Entities;
using Signum.Engine.Operations;
using Signum.Engine.Basics;
using Signum.Utilities;
using Signum.Web;
using Signum.Test.Environment;
using Music.Test;

namespace Music.Web
{
    public class MusicController : Controller
    {
        public ViewResult AlbumStrip()
        {
            return this.NormalPage(Database.Retrieve<AlbumEntity>(1), new NavigateOptions { PartialViewName = "AlbumStrip" });
        }

        public ViewResult BandStrip()
        {
            return this.NormalPage(Database.Retrieve<BandEntity>(1), new NavigateOptions { PartialViewName = "BandStrip" });
        }

        public ViewResult BandDetail()
        {
            return this.NormalPage(Database.Retrieve<BandEntity>(1), new NavigateOptions { PartialViewName = "BandDetail" });
        }

        public ViewResult BandRepeater() 
        {
            return this.NormalPage(Database.Retrieve<BandEntity>(1), new NavigateOptions { PartialViewName = "BandRepeater" });
        }

        public ViewResult BandTabRepeater()
        {
            return this.NormalPage(Database.Retrieve<BandEntity>(1), new NavigateOptions { PartialViewName = "BandTabRepeater" });
        }

        [HttpPost]
        public ActionResult CreateAlbumFromBandModel()
        {
            AlbumFromBandModel model = new AlbumFromBandModel();

            return this.PopupView(model, new PopupViewOptions(this.Prefix()));
        }

        [HttpPost]
        public ActionResult CreateAlbumFromBandExecute()
        {
            string modelPrefix = Request["modelPrefix"];

            var model = this.ExtractEntity<AlbumFromBandModel>(modelPrefix)
                .ApplyChanges(this, modelPrefix).Value;

            AlbumEntity newAlbum = this.ExtractLite<BandEntity>().ConstructFromLite(AlbumOperation.CreateAlbumFromBand,
                model.Name, model.Year, model.Label);

            return OperationClient.DefaultConstructResult(this, newAlbum);
        }

        [HttpPost]
        public ActionResult CreateGreatestHitsAlbum()
        {
            var sourceAlbums = this.ParseLiteKeys<AlbumEntity>();
            
            var newAlbum = OperationLogic.ConstructFromMany(sourceAlbums, AlbumOperation.CreateGreatestHitsAlbum);

            return OperationClient.DefaultConstructResult(this, newAlbum);
        }

        [HttpPost]
        public ActionResult Clone()
        {
            var album = this.ExtractLite<AlbumEntity>();

            AlbumEntity newAlbum = album.ConstructFromLite(AlbumOperation.Clone);
            newAlbum.Name = this.ParseValue<string>("newName");

            return OperationClient.DefaultConstructResult(this, newAlbum); 
        }
    }
}
