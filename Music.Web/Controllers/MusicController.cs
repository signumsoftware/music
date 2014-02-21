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
            return Navigator.NormalPage(this, new NavigateOptions(Database.Retrieve<AlbumDN>(1)) { PartialViewName = "AlbumStrip" });
        }

        public ViewResult BandStrip()
        {
            return Navigator.NormalPage(this, new NavigateOptions(Database.Retrieve<BandDN>(1)) { PartialViewName = "BandStrip" });
        }

        public ViewResult BandDetail()
        {
            return Navigator.NormalPage(this, new NavigateOptions(Database.Retrieve<BandDN>(1)) { PartialViewName = "BandDetail" });
        }

        public ViewResult BandRepeater() 
        {
            return Navigator.NormalPage(this, new NavigateOptions(Database.Retrieve<BandDN>(1)) { PartialViewName = "BandRepeater" });
        }

        [HttpPost]
        public ActionResult CreateAlbumFromBandModel()
        {
            AlbumFromBandModel model = new AlbumFromBandModel();

            TypeContext tc = TypeContextUtilities.UntypedNew(model, this.Prefix());
            return this.PopupOpen(new PopupViewOptions(tc));
        }

        [HttpPost]
        public ActionResult CreateAlbumFromBandExecute()
        {
            string modelPrefix = Request["modelPrefix"];

            var model = this.ExtractEntity<AlbumFromBandModel>(modelPrefix)
                .ApplyChanges(this.ControllerContext, true, modelPrefix).Value;

            AlbumDN newAlbum = this.ExtractLite<BandDN>().ConstructFromLite<AlbumDN>(AlbumOperation.CreateAlbumFromBand,
                model.Name, model.Year, model.Label);

            return OperationClient.DefaultConstructResult(this, newAlbum);
        }

        [HttpPost]
        public ActionResult CreateGreatestHitsAlbum()
        {
            var sourceAlbums = this.ParseLiteKeys<AlbumDN>();
            
            var newAlbum = OperationLogic.ConstructFromMany<AlbumDN, AlbumDN>(sourceAlbums, AlbumOperation.CreateGreatestHitsAlbum);

            return OperationClient.DefaultConstructResult(this, newAlbum);
        }

        [HttpPost]
        public ActionResult Clone()
        {
            var album = this.ExtractLite<AlbumDN>();

            AlbumDN newAlbum = album.ConstructFromLite<AlbumDN>(AlbumOperation.Clone);
            newAlbum.Name = this.ParseValue<string>("newName");

            return OperationClient.DefaultConstructResult(this, newAlbum); 
        }
    }
}
