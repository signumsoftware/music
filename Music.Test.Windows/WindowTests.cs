using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Signum.Windows.UIAutomation;
using System.Windows.Automation;
using Signum.Entities.DynamicQuery;
using Signum.Test;
using Signum.Entities;
using Signum.Engine;
using Signum.Test.Environment;

namespace Music.Test.Windows
{
    [TestClass]
    public class WindowTests
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            Common.Start();
        }

        [TestMethod]
        public void Search()
        {
            using (MainWindowProxy win = Common.StartAndLogin())
            {
                //win.Core.MenuItemInvoke("Music");
                using (SearchWindowProxy albums = new SearchWindowProxy(win.MenuItemOpenWindow("Music", "Albums")))
                {
                    albums.AddFilterString("Entity.Name", FilterOperation.Contains, "A");
                    albums.AddFilterString("Entity.Author", FilterOperation.EqualTo, "Smashing");

                    //albums.Search();
                    albums.SortColumn("Name", OrderType.Descending);
                    albums.SortColumn("Author", OrderType.Descending);
                    using (NormalWindowProxy<AlbumEntity> album = albums.ViewElementAt<AlbumEntity>(0))
                    {
                        album.ValueLineValue(a => a.Name, "Sartorum");
                        Assert.AreEqual("Sartorum", album.ValueLineValue(a => a.Name));

                        album.ValueLine(a => a.Year).Value = 1234;
                        Assert.AreEqual(album.ValueLine(a => a.Year).Value, 1234);

                        album.EntityLine(a => a.Label).Autocomplete("Son");
                        Assert.AreEqual(album.EntityLine(a => a.Label).LiteValue.EntityType, typeof(LabelEntity));
                        album.EntityCombo(a => a.Author).SelectToString("Smashing");

                        var songs = album.EntityList(a => a.Songs);

                        songs.SelectElementAt(0);
                        songs.Remove();
                        songs.CreateButton.ButtonInvoke();

                        album.ValueLineValue(a => a.Songs.First().Name, "Armengol");
                        album.ValueLineValue(a => a.Songs.First().Duration, TimeSpan.FromSeconds(230));

                        songs.CreateButton.ButtonInvoke();

                        album.ValueLineValue(a => a.Songs.First().Name, "Jean-Luc");
                        album.ValueLineValue(a => a.Songs.First().Duration, TimeSpan.FromSeconds(190));

                        album.Execute(AlbumOperation.Save);

                        using (var message = album.Element.WaitMessageBoxChild())
                        {
                            message.OkButton.ButtonInvoke();
                        }

                        album.Reload(true);

                        album.Close();
                    }
                }
            }

        }
    }
}
