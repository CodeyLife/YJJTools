using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GPSUtility
{
	//WGS-84：是国际标准，GPS坐标（Google Earth使用、或者GPS模块）
	//GCJ-02：中国坐标偏移标准，Google Map、高德、腾讯使用
	//BD-09 ：百度坐标偏移标准，Baidu Map使用

	#region 数据申明

	//public static LocationServiceStatus status;
	public static float desiredAccuracy = 70;

    #endregion


	/// <summary>圆周率</summary>
	private const double PI = 3.1415926535897932384626;
	private const double X_PI = PI * 3000.0 / 180.0;


	private static float metersPerLat;
	private static float metersPerLon;
	private static Vector2 _localOrigin = Vector2.zero;
	private static float _LatOrigin { get { return _localOrigin.x; } }
	private static float _LonOrigin { get { return _localOrigin.y; } }
	private static void FindMetersPerLat(float lat) // Compute lengths of degrees
	{
		// Set up "Constants"
		float m1 = 111132.92f;    // latitude calculation term 1
		float m2 = -559.82f;        // latitude calculation term 2
		float m3 = 1.175f;      // latitude calculation term 3
		float m4 = -0.0023f;        // latitude calculation term 4
		float p1 = 111412.84f;    // longitude calculation term 1
		float p2 = -93.5f;      // longitude calculation term 2
		float p3 = 0.118f;      // longitude calculation term 3

		lat = lat * Mathf.Deg2Rad;

		// Calculate the length of a degree of latitude and longitude in meters
		metersPerLat = m1 + (m2 * Mathf.Cos(2 * (float)lat)) + (m3 * Mathf.Cos(4 * (float)lat)) + (m4 * Mathf.Cos(6 * (float)lat));
		metersPerLon = (p1 * Mathf.Cos((float)lat)) + (p2 * Mathf.Cos(3 * (float)lat)) + (p3 * Mathf.Cos(5 * (float)lat));
	}
	/// <summary>
	/// 经纬度转坐标
	/// </summary>
	/// <param name="gps"></param>
	/// <returns></returns>
	public static Vector3 ConvertGPStoUCS(Vector2 gps)
	{
		
		float zPosition = metersPerLat * (gps.x - _LatOrigin); //Calc current lat
		float xPosition = metersPerLon * (gps.y - _LonOrigin); //Calc current lat
		return new Vector3((float)xPosition, 0, (float)zPosition);
	}
	public static void SetOrigin(Vector2 or)
    {
		_localOrigin = or;
		FindMetersPerLat(_LatOrigin);
    }

	/// <summary>
	/// wgs84转墨卡托
	/// </summary>
	/// <param name=""></param>
	public static Vector2 Lonlat2mercator(double lng, double lat)
	{
		Vector2 result = Vector2.zero;
		var m = lng * 20037508.342789 / 180;
		var n = Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
		n = n * 20037508.34789 / 180;
		result.x = (float)m;
		result.y = (float)n;
		return result;
	}
	public static void Lonlat2mercator(double lng, double lat,out double mktlng,out double mktlat)
	{
		var m = lng * 20037508.342789 / 180;
		var n = Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
		n = n * 20037508.34789 / 180;
		mktlng = m;
		mktlat = n;
	}

	public static void  Mercator2LonLat(double x, double y, out double lon, out double lat)
	{
		 lon = x / 20037508.34 * 180;
		 lat = y / 20037508.34 * 180;
	  	lat = 180 / Math.PI * (2 * Math.Atan(Math.Exp(lat * Math.PI / 180)) - Math.PI / 2);
		
	}


	/// <summary>
	/// 地理位置是否位于中国以外
	/// </summary>
	/// <param name="wgLat">WGS-84坐标纬度</param>
	/// <param name="wgLon">WGS-84坐标经度</param>
	/// <returns>  true：国外 false：国内
	/// </returns>
	public static bool OutOfChina(double wgLat, double wgLon)
	{
		if (wgLon < 72.004 || wgLon > 137.8347) return true;
		if (wgLat < 0.8293 || wgLat > 55.8271) return true;

		return false;
	}
	/// <summary>
	/// WGS-84坐标系转火星坐标系 (GCJ-02)
	/// </summary>
	/// <param name="wgLat">WGS-84坐标纬度</param>
	/// <param name="wgLon">WGS-84坐标经度</param>
	/// <param name="mgLat">输出：GCJ-02坐标纬度</param>
	/// <param name="mgLon">输出：GCJ-02坐标经度</param>
	public static void WGS84ToGCJ02(double wgLat, double wgLon, out double mgLat, out double mgLon)
	{
		if (OutOfChina(wgLat, wgLon))
		{
			mgLat = wgLat;
			mgLon = wgLon;
		}
		else
		{
			double dLat;
			double dLon;
			Delta(wgLat, wgLon, out dLat, out dLon);
			mgLat = wgLat + dLat;
			mgLon = wgLon + dLon;
		}

	}

	/// <summary>
	/// 火星坐标系 (GCJ-02)转WGS-84坐标系
	/// </summary>
	/// <param name="mgLat">GCJ-02坐标纬度</param>
	/// <param name="mgLon">GCJ-02坐标经度</param>
	/// <param name="wgLat">输出：WGS-84坐标纬度</param>
	/// <param name="wgLon">输出：WGS-84坐标经度</param>
	public static void GCJ02ToWGS84(double mgLat, double mgLon, out double wgLat, out double wgLon)
	{
		if (OutOfChina(mgLat, mgLon))
		{
			wgLat = mgLat;
			wgLon = mgLon;
		}
		else
		{
			double dLat;
			double dLon;
			Delta(mgLat, mgLon, out dLat, out dLon);
			wgLat = mgLat - dLat;
			wgLon = mgLon - dLon;
		}
	}

	/// <summary>
	/// 百度坐标系 (BD-09)转火星坐标系 (GCJ-02)
	/// </summary>
	/// <param name="bdLat">百度坐标系纬度</param>
	/// <param name="bdLon">百度坐标系经度</param>
	/// <param name="mgLat">输出：GCJ-02坐标纬度</param>
	/// <param name="mgLon">输出：GCJ-02坐标经度</param>         
	public static void BD09ToGCJ02(double bdLat, double bdLon, out double mgLat, out double mgLon)
	{
		double x = bdLon - 0.0065;
		double y = bdLat - 0.006;
		double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * X_PI);
		double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * X_PI);
		mgLat = z * Math.Sin(theta);
		mgLon = z * Math.Cos(theta);
	}

	/// <summary>
	/// 百度坐标系 (BD-09)转WGS-84坐标系
	/// </summary>
	/// <param name="bdLat">百度坐标系纬度</param>
	/// <param name="bdLon">百度坐标系经度</param>
	/// <param name="wgLat">输出：WGS-84坐标纬度</param>
	/// <param name="wgLon">输出：WGS-84坐标经度</param>
	public static void BD09ToWGS84(double bdLat, double bdLon, out double wgLat, out double wgLon)
	{
		double mgLat;
		double mgLon;

		BD09ToGCJ02(bdLat, bdLon, out mgLat, out mgLon);
		GCJ02ToWGS84(mgLat, mgLon, out wgLat, out wgLon);
    }
	// 百度坐标转换为WGS坐标的方法
	public static double[] BaiduToWGS(double bdLon, double bdLat)
	{
		double x_pi = 3.14159265358979324 * 3000.0 / 180.0;
		double x = bdLon - 0.0065;
		double y = bdLat - 0.006;
		double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_pi);
		double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * x_pi);
		double wgsLon = z * Math.Cos(theta);
		double wgsLat = z * Math.Sin(theta);
		return new double[] { wgsLon, wgsLat };
	}


/// <summary>
/// 度分秒经纬度(必须含有'°')和数字经纬度转换
/// </summary>
/// <param name="digitalDegree">度分秒经纬度</param>
/// <return>数字经纬度</return>
static public double ConvertDegreesToDigital(string degrees)
    {
        const double num = 60;
        double digitalDegree = 0.0;
        int d = degrees.IndexOf('°');           //度的符号对应的 Unicode 代码为：00B0[1]（六十进制），显示为°。
        if (d < 0)
        {
            return digitalDegree;
        }
        string degree = degrees.Substring(0, d);
        digitalDegree += Convert.ToDouble(degree);

        int m = degrees.IndexOf('′');           //分的符号对应的 Unicode 代码为：2032[1]（六十进制），显示为′。
        if (m < 0)
        {
            return digitalDegree;
        }
        string minute = degrees.Substring(d + 1, m - d - 1);
        digitalDegree += ((Convert.ToDouble(minute)) / num);

        int s = degrees.IndexOf('″');           //秒的符号对应的 Unicode 代码为：2033[1]（六十进制），显示为″。
        if (s < 0)
        {
            return digitalDegree;
        }
        string second = degrees.Substring(m + 1, s - m - 1);
        digitalDegree += (Convert.ToDouble(second) / (num * num));

        return digitalDegree;
    }

	/// <summary>
	/// 以逗号分割的分数经纬度
	/// </summary>
	/// <param name="lngAndlat"></param>
	public static void Convert2Digital(string lngAndlat, out double lng, out double lat)
    {
		var arr = lngAndlat.Split(',');
	    lng = ConvertDegreesToDigital(arr[0]);
	    lat = ConvertDegreesToDigital(arr[1]);
	}
    #region 辅助
    private static void Delta(double Lat, double Lon, out double dLat, out double dLon)
	{
		const double AXIS = 6378245.0;
		const double EE = 0.00669342162296594323;

		dLat = TransformLat(Lon - 105.0, Lat - 35.0);
		dLon = TransformLon(Lon - 105.0, Lat - 35.0);
		double radLat = Lat / 180.0 * PI;
		double magic = Math.Sin(radLat);
		magic = 1 - EE * magic * magic;
		double sqrtMagic = Math.Sqrt(magic);
		dLat = (dLat * 180.0) / ((AXIS * (1 - EE)) / (magic * sqrtMagic) * PI);
		dLon = (dLon * 180.0) / (AXIS / sqrtMagic * Math.Cos(radLat) * PI);
	}
	private static double TransformLat(double x, double y)
	{
		double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
		ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
		ret += (20.0 * Math.Sin(y * PI) + 40.0 * Math.Sin(y / 3.0 * PI)) * 2.0 / 3.0;
		ret += (160.0 * Math.Sin(y / 12.0 * PI) + 320 * Math.Sin(y * PI / 30.0)) * 2.0 / 3.0;
		return ret;
	}

	private static double TransformLon(double x, double y)
	{
		double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
		ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
		ret += (20.0 * Math.Sin(x * PI) + 40.0 * Math.Sin(x / 3.0 * PI)) * 2.0 / 3.0;
		ret += (150.0 * Math.Sin(x / 12.0 * PI) + 300.0 * Math.Sin(x / 30.0 * PI)) * 2.0 / 3.0;
		return ret;
	}
    #endregion
}

