using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.CAD
{
  public class StatePlanePoint
  {
    public double X { get; set; } = double.MinValue;
    public double Y { get; set; } = double.MinValue;
    public decimal OriginalLatitude { get; set; } = 0;
    public decimal OriginalLongitude { get; set; } = 0;
    public decimal Latitude { get; set; } = 0;
    public decimal Longitude { get; set; } = 0;
    //public decimal ShortLongitude { get; set; } = 0;
    //public decimal ShortLatitude { get; set; } = 0;
    public bool IsValid
    {
      get
      {
        return X != double.MinValue;
      }
    }



    public StatePlanePoint()
    {

    }
    public StatePlanePoint(double NewX, double NewY)
    {
      if (NewX == 0 || NewY == 0) return;
      X = NewX;
      Y = NewY;
      ToLatLong();
    }

    private void ToLatLong()
    {
      string source_wkt = @"PROJCS[""NAD_1983_HARN_StatePlane_Florida_East_FIPS_0901_Feet"", GEOGCS[""GCS_North_American_1983_HARN"", DATUM[""NAD83_High_Accuracy_Regional_Network"", SPHEROID[""GRS_1980"", 6378137.0, 298.257222101]], PRIMEM[""Greenwich"", 0.0], UNIT[""Degree"", 0.0174532925199433]], PROJECTION[""Transverse_Mercator""], PARAMETER[""False_Easting"", 656166.6666666665], PARAMETER[""False_Northing"", 0.0], PARAMETER[""Central_Meridian"", -81.0], PARAMETER[""Scale_Factor"", 0.9999411764705882], PARAMETER[""Latitude_Of_Origin"", 24.33333333333333], UNIT[""Foot_US"", 0.3048006096012192]]";
      var x = new ProjNet.CoordinateSystems.CoordinateSystemFactory();
      var projsource = x.CreateFromWkt(source_wkt);

      //var csource = 
      var ctarget = ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84;
      var t = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
      var trans = t.CreateFromCoordinateSystems(projsource, ctarget);
      double[] point = { X, Y };
      double[] convpoint = trans.MathTransform.Transform(point);
      OriginalLongitude = (decimal)convpoint[0];
      OriginalLatitude = (decimal)convpoint[1];
      Longitude = OriginalLongitude;
      Latitude = OriginalLatitude;
      //Longitude = Program.Truncate(OriginalLongitude, 5);
      //Latitude = Program.Truncate(OriginalLatitude, 5);
      //ShortLongitude = Program.Truncate(OriginalLongitude, 4);
      //ShortLatitude = Program.Truncate(OriginalLatitude, 4);
    }


  }
}
