using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Signum.Test;
using Signum.Entities;
using Signum.Test.Environment;

namespace Music.Test
{
    public class AlbumFromBandModel : ModelEntity
    {
        public AlbumFromBandModel()
        { 
        }


        string name;
        [StringLengthValidator(Min=3, AllowNulls=false)]
        public string Name
        {
            get { return name; }
            set { Set(ref name, value, () => Name); }
        }

        int year;
        [NumberBetweenValidatorAttribute(1000,Int32.MaxValue)]
        public int Year
        {
            get { return year; }
            set { Set(ref year, value, () => Year); }
        }

        LabelDN label;
        [NotNullValidator]
        public LabelDN Label
        {
            get { return label; }
            set { Set(ref label, value, () => Label); }
        }
    }
}
