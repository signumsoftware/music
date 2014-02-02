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
        public ActionResult CreateAlbumFromBand(string prefix)
        {
            BandDN band = Navigator.ExtractEntity<BandDN>(this);

            AlbumFromBandModel model = new AlbumFromBandModel();

            TypeContext tc = TypeContextUtilities.UntypedNew(model, prefix);
            return this.PopupOpen(new PopupViewOptions(tc));
        }

        [HttpPost]
        public ActionResult CreateAlbumFromBandExecute(string prefix, string modelPrefix, string newPrefix)
        {
            var model = this.ExtractEntity<AlbumFromBandModel>(modelPrefix)
                .ApplyChanges(this.ControllerContext, prefix, true).Value;

            AlbumDN newAlbum = this.ExtractLite<AlbumDN>(prefix).ConstructFromLite<AlbumDN>(AlbumOperation.CreateAlbumFromBand, 
                new object[] { model.Name, model.Year, model.Label });

            return OperationClient.DefaultConstructResult(this, newAlbum, newPrefix);
        }

        [HttpPost]
        public ActionResult CreateGreatestHitsAlbum(string newPrefix)
        {
            var sourceAlbums = Navigator.ParseLiteKeys<AlbumDN>(Request["keys"]);
            
            var newAlbum = OperationLogic.ConstructFromMany<AlbumDN, AlbumDN>(sourceAlbums, AlbumOperation.CreateGreatestHitsAlbum);

            return OperationClient.DefaultConstructResult(this, newAlbum, newPrefix);
        }

        [HttpPost]
        public ActionResult CloneValueLine(string prefix)
        {
            ViewData[ViewDataKeys.Title] = "Write new album's name";

            var model = new ValueLineBoxModel(this.ExtractEntity<AlbumDN>(), ValueLineBoxType.String, "Name", "Write new album's name");
            return this.PopupOpen(new PopupViewOptions(new TypeContext<ValueLineBoxModel>(model, prefix)));
        }
       

        [HttpPost]
        public ActionResult Clone(string prefix, string modelPrefix, string newPrefix)
        {
            var modelo = this.ExtractEntity<ValueLineBoxModel>(modelPrefix)
                               .ApplyChanges(this.ControllerContext, modelPrefix, false).Value;

            var album = this.ExtractLite<AlbumDN>(prefix);

            AlbumDN newAlbum = album.ConstructFromLite<AlbumDN>(AlbumOperation.Clone);
            newAlbum.Name = modelo.StringValue;

            return OperationClient.DefaultConstructResult(this, newAlbum, newPrefix); 
        }
    }
}
