using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.GIS
{
  public class Point
  {
    public double X { get; set; } = double.MinValue;
    public double Y { get; set; } = double.MinValue;
    public double Latitude { get; set; } = double.MinValue;
    public double Longitude { get; set; } = double.MinValue;
    public bool IsValid
    {
      get
      {
        return X != double.MinValue;
      }
    }
    public Point()
    {

    }
    public Point(double NewX, double NewY)
    {
      if (NewX == 0 || NewY == 0) return;
      X = NewX;
      Y = NewY;
      ToLatLong();
    }
    private void ToLatLong()
    {
      //string source_wkt = @"PROJCS[""WGS 84 / Pseudo-Mercator"",GEOGCS[""Popular Visualisation CRS"",DATUM[""Popular_Visualisation_Datum"",SPHEROID[""Popular Visualisation Sphere"",6378137,0,AUTHORITY[""EPSG"",""7059""]],TOWGS84[0,0,0,0,0,0,0],AUTHORITY[""EPSG"",""6055""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.01745329251994328,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4055""]],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]],PROJECTION[""Mercator_1SP""],PARAMETER[""central_meridian"",0],PARAMETER[""scale_factor"",1],PARAMETER[""false_easting"",0],PARAMETER[""false_northing"",0],AUTHORITY[""EPSG"",""3785""],AXIS[""X"",EAST],AXIS[""Y"",NORTH]]";
      var x = new ProjNet.CoordinateSystems.CoordinateSystemFactory();
      var projsource = ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator;

      //var csource = x.CreateFromWkt(source_wkt);
      var ctarget = ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84;
      var t = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
      var trans = t.CreateFromCoordinateSystems(projsource, ctarget);
      double[] point = { X, Y };
      double[] convpoint = trans.MathTransform.Transform(point);
      Longitude = convpoint[0];
      Latitude = convpoint[1];
    }

  }
}
