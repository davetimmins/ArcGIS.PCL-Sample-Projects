using System;
using System.Collections;

/// <summary>
/// Licensed Materials - Property of Eagle Technology Group Ltd.
/// 
/// (c) Copyright Eagle Technology Group Ltd. 2007
/// 
/// All rights reserved.
/// 
/// Created on 30/04/2007
/// 
/// Project: Framework
/// 
/// Package: eagle.workflow.affectedarea
/// 
/// File: GenerateEQFunctionHandler.java
/// 
/// Author: jxs
/// </summary>

namespace eagle.workflow.affectedarea
{

	using Logger = org.apache.log4j.Logger;
	using IGeometry = com.esri.arcgis.geometry.IGeometry;
	using IGeometry2 = com.esri.arcgis.geometry.IGeometry2;
	using IGeometryBridge2 = com.esri.arcgis.geometry.IGeometryBridge2;
	using IPoint = com.esri.arcgis.geometry.IPoint;
	using IEnvelope = com.esri.arcgis.geometry.IEnvelope;
	using Envelope = com.esri.arcgis.geometry.Envelope;
	using IPointCollection4 = com.esri.arcgis.geometry.IPointCollection4;
	using IPolygon = com.esri.arcgis.geometry.IPolygon;
	using ISpatialReference = com.esri.arcgis.geometry.ISpatialReference;
	using ITopologicalOperator = com.esri.arcgis.geometry.ITopologicalOperator;
	using ITopologicalOperator2 = com.esri.arcgis.geometry.ITopologicalOperator2;
	using Point = com.esri.arcgis.geometry.Point;
	using Polygon = com.esri.arcgis.geometry.Polygon;
	using esriSRProjCS2Type = com.esri.arcgis.geometry.esriSRProjCS2Type;
	using esriSRProjCS4Type = com.esri.arcgis.geometry.esriSRProjCS4Type;
	using esriSRGeoCSType = com.esri.arcgis.geometry.esriSRGeoCSType;
	using IGeoTransformation = com.esri.arcgis.geometry.IGeoTransformation;
	using esriTransformDirection = com.esri.arcgis.geometry.esriTransformDirection;
	using esriSRGeoTransformation2Type = com.esri.arcgis.geometry.esriSRGeoTransformation2Type;
	using ConfigurationLoader = eagle.framework.configuration.ConfigurationLoader;
	using ConfigurationType = eagle.framework.configuration.ConfigurationType;
	using IRelationalOperator = com.esri.arcgis.geometry.IRelationalOperator;


	using IServerContext = com.esri.arcgis.server.IServerContext;
	using FrameworkLogger = eagle.framework.utility.FrameworkLogger;
	using ConfigurationLoader = eagle.framework.configuration.ConfigurationLoader;
	using ConfigurationType = eagle.framework.configuration.ConfigurationType;
	using IDatabaseAccess = eagle.framework.dal.connections.IDatabaseAccess;
	using FrameworkException = eagle.framework.exception.FrameworkException;
	using FrameworkExceptionType = eagle.framework.exception.FrameworkExceptionType;
	using IParameterList = eagle.framework.exception.IParameterList;
	using ParameterList = eagle.framework.exception.ParameterList;
	using FunctionHandler = eagle.framework.function.FunctionHandler;
	using RequestObj = eagle.framework.request.RequestObj;
	using ResponseEnum = eagle.framework.response.ResponseEnum;
	using ResponseObj = eagle.framework.response.ResponseObj;
	using CoordinateTransformer = eagle.framework.utility.CoordinateTransformer;
	using GeometryUtil = eagle.framework.utility.GeometryUtil;
	using SpatialReferenceEnvironment = com.esri.arcgis.geometry.SpatialReferenceEnvironment;

	public class GenerateEQFunctionHandler1999V2 : FunctionHandler
	{
		internal static Logger logger = FrameworkLogger.getLogger(typeof(GenerateEQFunctionHandler1999V2));
		private int IntIndexer;

		// private int m_matrixIndex;

		private int IntMaxIsoseismal;

		internal bool IsoSeismalDone = false;

		private readonly int m_isoseismalMax = 10;

		internal const int m_isoseismalMin = 3;

		private readonly double[] m_dbl_a1 = new double[] {4.87, 4.25, 4.00};

		private readonly double[] m_dbl_a2 = new double[] {1.25, 1.28, 1.63};

		private readonly double[] m_dbl_a3 = new double[] {-3.77, -3.73, -4.03};

		private readonly double[] m_dbl_a4 = new double[] {0.0083, 0.017, 0.0044};

		private readonly double[] m_dbl_a5 = new double[] {-0.68, 0.54, 0.0};

		private readonly double[] m_dbl_ad = new double[] {9.63, 10.05, 0.0};

		private readonly double[] m_dbl_b1 = new double[] {4.97, 4.20, 10.08};

		private readonly double[] m_dbl_b2 = new double[] {1.03, 1.11, 1.77};

		private readonly double[] m_dbl_b3 = new double[] {-3.39, -3.27, -8.02};

		private readonly double[] m_dbl_b4 = new double[] {0.015, 0.021, 0.012};

		private readonly double[] m_dbl_b5 = new double[] {-0.41, 0.31, 0.0};

		private readonly double[] m_dbl_bd = new double[] {5.74, 5.65, 0.0};

		private readonly double[] m_dbl_a2r = new double[] {0.048, 0.0, 0.0};

		private readonly double[] m_dbl_a2v = new double[] {0.21, 0.0, 0.0};

		private readonly double[] m_dbl_a3s = new double[] {0.16, 0.0, 0.0};

		private readonly double[] m_dbl_a3v = new double[] {-1.47, 0.0, 0.0};

		private readonly double[] m_dbl_b2r = new double[] {0.058, 0.0, 0.0};

		private readonly double[] m_dbl_b2v = new double[] {0.18, 0.0, 0.0};

		private readonly double[] m_dbl_b3s = new double[] {0.23, 0.0, 0.0};

		private readonly double[] m_dbl_b3v = new double[] {-1.35, 0.0, 0.0};

		private readonly double X_MIN_ENVELOP = 929000;
		private readonly double Y_MIN_ENVELOP = 4310000;

		private readonly double X_MAX_ENVELOP = 4370000;
		private readonly double Y_MAX_ENVELOP = 7758000;

		private IPoint m_epiCenter;

		private double[] m_dblArray_ra = new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

		private double[] m_dblArray_rb = new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

		private double m_dbl_ca, m_dbl_sa, m_dbl_d2r, m_dbl_x, m_dbl_y, m_dbl_xa, m_dbl_xb;

		private double[] m_dbl_Array_xrel = new double[] {-1.0, -0.95, -0.9, -0.8, -0.7, -0.6, -0.5, -0.4, -0.3, -0.2, -0.1, 0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 0.95, 1.0, 0.95, 0.9, 0.8, 0.7, 0.6, 0.5, 0.4, 0.3, 0.2, 0.1, 0.0, -0.1, -0.2, -0.3, -0.4, -0.5, -0.6, -0.7, -0.8, -0.9, -0.95, -1.0};

//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: private double[][] dblArray_xr = new double[10][45];
		private double[][] dblArray_xr = RectangularArrays.ReturnRectangularDoubleArray(10, 45);

//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: private double[][] dblArray_yr = new double[10][45];
		private double[][] dblArray_yr = RectangularArrays.ReturnRectangularDoubleArray(10, 45);

		private double[] m_dblArray_VolcanicRegion;

		private double[] m_dblArray_RegionA_appx;
		private double[] m_dblArray_RegionC_appx;
		private double[] m_dblArray_RegionE_appx;

		private Polygon m_polyVolcanic;
		private Polygon m_polyRegionA;
		private Polygon m_polyRegionC;
		private Polygon m_polyRegionE;


		private double m_dbl_Epicentre_NZmgE = 0;

		private double m_dbl_Epicentre_NZmgN = 0;

		private double m_dbl_Epicentre_NZtmE = 0;

		private double m_dbl_Epicentre_NZtmN = 0;


		private double m_dbl_Depth_to_fault_centriod_KM_hc = 0;
		internal readonly int M_DEFAULT_TO_DEPTH_HC_CONSTANT = 100;

		private double m_dbl_DepthToFaultTop_KM_ht = 0;
		internal readonly int M_DEFAULT_TO_TOP_HT_CONSTANT = 70;


		internal readonly double M_DEFAULT_TO_TOP_HT_CONSTANT_A = 0.5;
		internal readonly double M_DEFAULT_TO_TOP_HT_CONSTANT_B = 0.85;



		private double m_dbl_StrikeDegNorthOfE = 0; // GNS DEFAULTS
		internal readonly double STRIKE_ANGLE_DEFAULT = 33; // GNS DEFAULTS
		internal readonly double STRIKE_ANGLE_REGION_A = 130; // GNS DEFAULTS
		internal readonly double STRIKE_ANGLE_REGION_C = 40; // GNS DEFAULTS
		internal readonly double STRIKE_ANGLE_REGION_E = 179; // GNS DEFAULTS


		private double m_dbl_Magnitude = 0.0;
		internal readonly double M_MAGNITUDE_CONSTANT = 5.39; // GNIS DEFAULTS
		internal readonly double M_MAGNITUDE_MIN_HT_CONSTANT = 30.0; // GNIS DEFAULTS


		// private String m_str_EventID = "EQ3";

		private char m_chr_Mechanism = 'X';

		// Test Data End

		private double m_dbl_crint = 0.0;

		private double m_dbl_ss = 0.0;

		private double m_dbl_rev = 0.0;

		private double m_dbl_volc = 0.0;

		private ISpatialReference m_incommingCoordinateSystem;

		private ISpatialReference m_outGoingCoordinateSystem;

		private ISpatialReference m_incommingRegionalCoordinateSystem;


		private CoordinateTransformer m_InSystemTransformer;

		private CoordinateTransformer m_OutsystemTransformer;

		private CoordinateTransformer m_regionalTransformer;
		private Polygon m_activePolygone;

		private ArrayList[] m_ListIsoseismalList = new ArrayList[10];

		private ArrayList BandOrder = new ArrayList();

		private Envelope m_validateEnvelope;
		// public EQAffectedArea m_affectedArea;

		//private  AffectedAreaMapperSDE m_affectedAreaMapperSde;

		private GNSRegionsMapperSDE m_GNSRegionsMapper;

		public IServerContext p_serverContext;

		private IPoint m_MZTM_EpiCenter;

		public ResponseObj p_responseObj = new ResponseObj();


		public GenerateEQFunctionHandler1999V2(IDatabaseAccess databaseAccess, IDatabaseAccess sdeAccess, IServerContext serverContext) : base(databaseAccess, sdeAccess, serverContext)
		{

			p_serverContext = serverContext;

		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void Init() throws Exception
		 private void Init()
		 {
			  m_InSystemTransformer = new CoordinateTransformer();

			m_InSystemTransformer.m_createProjectedTransformer(p_serverContext, esriSRProjCS2Type.esriSRProjCS_NZGD1949NewZealandMapGrid);

			m_OutsystemTransformer = new CoordinateTransformer();
			m_OutsystemTransformer.m_createProjectedTransformer(p_serverContext, esriSRProjCS4Type.esriSRProjCS_NZGD2000_New_Zealand_Transverse_Mercator);

			m_regionalTransformer = new CoordinateTransformer();
			m_regionalTransformer.m_createGeographicTransformer(p_serverContext, esriSRGeoCSType.esriSRGeoCS_NZGD1949);

			m_incommingRegionalCoordinateSystem = m_regionalTransformer.m_getSpatialReference();

			m_incommingCoordinateSystem = m_InSystemTransformer.m_getSpatialReference();

			m_outGoingCoordinateSystem = m_OutsystemTransformer.m_getSpatialReference();


			m_GNSRegionsMapper = new GNSRegionsMapperSDE(base.SdeAccess);

			m_activePolygone = (Polygon) p_serverContext.createObject("esriGeometry.Polygon");
			m_activePolygone.SpatialReferenceByRef = m_outGoingCoordinateSystem;

		 }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public eagle.framework.response.ResponseObj m_execute(AffectedArea in_affectedArea) throws Exception
		public virtual ResponseObj m_execute(AffectedArea in_affectedArea)
		{


			m_dbl_Epicentre_NZmgE = in_affectedArea.EpicenterEasting; //in_dbl_Epicentre_NZmgE;
			m_dbl_Epicentre_NZmgN = in_affectedArea.EpicenterNorthing; //in_dbl_Epicentre_NZmgN;
			m_dbl_Depth_to_fault_centriod_KM_hc = in_affectedArea.Depth; //in_dbl_Depth_to_fault_centriod_KM;
			m_dbl_DepthToFaultTop_KM_ht = in_affectedArea.Top; //in_dbl_DepthToFaultTop;
			m_dbl_StrikeDegNorthOfE = in_affectedArea.Strikeangle; //in_dbl_StrikeDegNorthOfE;
			m_dbl_Magnitude = in_affectedArea.Magnitude; //in_dbl_Magnitude;
			m_chr_Mechanism = in_affectedArea.Mechanism; //in_chr_Mechanism;

			ResponseObj p_ResponseObject = new ResponseObj();




			Init();
			Validate();
			m_prepareRegions();
			m_setupLaunchSequence();
			m_generateQuake();

			p_ResponseObject.m_addKeyAndValue("RESULT", m_MakePolygons());
			p_ResponseObject.m_addKeyAndValue("ACTIVEBAND", new int?(IntMaxIsoseismal));
			p_ResponseObject.m_addKeyAndValue("BANDORDER", BandOrder);

			p_ResponseObject.m_addKeyAndValue("NZTM_EASTING", new double?(m_dbl_Epicentre_NZtmE));
			p_ResponseObject.m_addKeyAndValue("NZTM_NORTHING", new double?(m_dbl_Epicentre_NZtmN));
			p_ResponseObject.m_addKeyAndValue("EPI_CENTER", m_MZTM_EpiCenter);
			p_ResponseObject.m_addKeyAndValue("Status", ResponseEnum.Success);
			p_ResponseObject.m_addKeyAndValue("ACTIVE_BAND_POLYGON", m_activePolygone);


			return p_ResponseObject;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void m_setupLaunchSequence() throws Exception
		private void m_setupLaunchSequence()
		{
			/*
			 Currently HT is not set through Scott's code and is always 0. The user 
			should be able to set HT. If it is not set then it should default as 
			follows: 
	
			For centroids less than 100 km 
			  W = 0.5 * 10**(Mw - 5.39) 
			  W = MIN (W, 30.) 
			  ht = hc - 0.5 * W * 0.85 
			  ht = MAX (ht, 0) 
	
			For centroid depths greater than 100 km, ht is set to the hc 
			*/
			IRelationalOperator p_relationalOperator;

			// User has not specified the HT or has specified but Depth is les then 100 km 		
			if ((m_dbl_DepthToFaultTop_KM_ht == 0))
			{
				if (m_dbl_Depth_to_fault_centriod_KM_hc < M_DEFAULT_TO_DEPTH_HC_CONSTANT) // 100
				{
					double p_W = Math.Pow(10.0, (m_dbl_Magnitude - M_MAGNITUDE_CONSTANT)); // 5.39
					p_W = Math.Min(p_W, M_MAGNITUDE_MIN_HT_CONSTANT); // 30

					m_dbl_DepthToFaultTop_KM_ht = m_dbl_Depth_to_fault_centriod_KM_hc - (M_DEFAULT_TO_TOP_HT_CONSTANT_A * p_W * M_DEFAULT_TO_TOP_HT_CONSTANT_B); // 0.5
					// and
					// 0.85
					m_dbl_DepthToFaultTop_KM_ht = Math.Max(m_dbl_DepthToFaultTop_KM_ht, 0);

				}
				else
				{
	//				 User has not specified the HT or has specified but Depth is greather then 100 km 		
					  m_dbl_DepthToFaultTop_KM_ht = m_dbl_Depth_to_fault_centriod_KM_hc;
				}

			}
			// - end

			/*
			 * should default to X except where hc >= 70 in which case it should
			 * default to D (deep).
			 */

			// Machanism is UnKnown ot the User has choosen  X TO be DEFAULT
			if (m_chr_Mechanism == 'X')
			{
				if (m_dbl_Depth_to_fault_centriod_KM_hc > M_DEFAULT_TO_TOP_HT_CONSTANT)
				{
					m_chr_Mechanism = 'D';
				}
				/*
				 * If the epicentre falls in the following area (Defined Volcanic
				 * Region)then it should default to V (volcanic).
				 */

				 p_relationalOperator = (IRelationalOperator) m_polyVolcanic;
				if (p_relationalOperator.contains(m_MZTM_EpiCenter))
				{
					m_chr_Mechanism = 'V';
				}
			}

			/*
			The attpx value should be refined based on the epicentre 
			Region A   -39.00, 175.53, -39.00, 172.00, -33.00, 170.00, attpx = 130 
						 -33.00, 177.00, -36.90, 177.00, -37.87, 176.33, 
						-38.33, 175.64 
	
			Region C   39.45, 175.42, -38.33, 175.64, -37.87, 176.33, attpx = 40 
						-36.90, 177.00, -36.00, 177.50, -36.00, 178.00, 
						-36.96, 177.56, -37.34, 177.42, -38.52, 176.37 
	
			Region E  40.50, 174.50, -39.45, 175.42, -39.00, 175.53, attpx = 179 
						-39.00, 171.00 
	
			else       attpx = 33 
	
			 */
			//	 Use has NO INPUT has been detected.
			if (m_dbl_StrikeDegNorthOfE == 0)
			{
				m_dbl_StrikeDegNorthOfE = STRIKE_ANGLE_DEFAULT; // 33

				p_relationalOperator = (IRelationalOperator) m_polyRegionA;
				if (p_relationalOperator.contains(m_MZTM_EpiCenter))
				{
					m_dbl_StrikeDegNorthOfE = STRIKE_ANGLE_REGION_A;

				}
				p_relationalOperator = (IRelationalOperator) m_polyRegionC;
				if (p_relationalOperator.contains(m_MZTM_EpiCenter))
				{
					m_dbl_StrikeDegNorthOfE = STRIKE_ANGLE_REGION_C;
				}
				p_relationalOperator = (IRelationalOperator) m_polyRegionE;
				if (p_relationalOperator.contains(m_MZTM_EpiCenter))
				{
					m_dbl_StrikeDegNorthOfE = STRIKE_ANGLE_REGION_E;
				}
			}
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void m_prepareRegions() throws Exception
		private void m_prepareRegions()
		{

			bool p_IschangeRequired = ConfigurationLoader.getBoolean(ConfigurationType.isRegionchanged.m_getValue());
			int p_numberOfCoordinates;
			// Regenerate the Regional Polygones using the coordinates from the
			// Config

			p_numberOfCoordinates = ConfigurationLoader.getInteger(ConfigurationType.numberOfCoordinates_VOLCANIC_REGION.m_getValue());
			m_dblArray_VolcanicRegion = ConfigurationLoader.getdoubleArray(ConfigurationType.coordinates_VOLCANIC_REGION.m_getValue(), p_numberOfCoordinates);

			p_numberOfCoordinates = ConfigurationLoader.getInteger(ConfigurationType.numberOfCoordinates_REGION_A.m_getValue());
			m_dblArray_RegionA_appx = ConfigurationLoader.getdoubleArray(ConfigurationType.coordinates_REGION_A.m_getValue(), p_numberOfCoordinates);

			p_numberOfCoordinates = ConfigurationLoader.getInteger(ConfigurationType.numberOfCoordinates_REGION_C.m_getValue());
			m_dblArray_RegionC_appx = ConfigurationLoader.getdoubleArray(ConfigurationType.coordinates_REGION_C.m_getValue(), p_numberOfCoordinates);

			p_numberOfCoordinates = ConfigurationLoader.getInteger(ConfigurationType.numberOfCoordinates_REGION_E.m_getValue());
			m_dblArray_RegionE_appx = ConfigurationLoader.getdoubleArray(ConfigurationType.coordinates_REGION_E.m_getValue(), p_numberOfCoordinates);


			if (p_IschangeRequired)
			{
				m_polyVolcanic = m_makeRegions(m_dblArray_VolcanicRegion, "REGION_VOLCANIC");
				m_polyRegionA = m_makeRegions(m_dblArray_RegionA_appx, "REGION_A");
				m_polyRegionC = m_makeRegions(m_dblArray_RegionC_appx, "REGION_C");
				m_polyRegionE = m_makeRegions(m_dblArray_RegionE_appx, "REGION_E");

			}
			// Use the existing polygones from the DB, If they dont exist create them on the fly and store in DB
			else
			{

				m_polyVolcanic = m_getRegions(m_dblArray_VolcanicRegion,"REGION_VOLCANIC");
				m_polyRegionA = m_getRegions(m_dblArray_RegionA_appx,"REGION_A");
				m_polyRegionC = m_getRegions(m_dblArray_RegionC_appx,"REGION_C");
				m_polyRegionE = m_getRegions(m_dblArray_RegionE_appx,"REGION_E");
			}

		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private com.esri.arcgis.geometry.Polygon m_getRegions(double[] in_dblArray_Region,String in_strRegionName) throws Exception
		private Polygon m_getRegions(double[] in_dblArray_Region, string in_strRegionName)
		{
			Polygon p_poly;
			if (m_GNSRegionsMapper.m_doesExist(AffectedAreaDB.gnsRegionsTableName, AffectedAreaDB.regionIDFieldName, in_strRegionName))
			{
				p_poly = m_GNSRegionsMapper.m_getRegionalPolygone(in_strRegionName);
			}
			else
			{
				p_poly = m_makeRegions(in_dblArray_Region, in_strRegionName);

			}
			return p_poly;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private com.esri.arcgis.geometry.Polygon m_makeRegions(double[] in_dblArray_Region,String in_strRegionName) throws Exception
		private Polygon m_makeRegions(double[] in_dblArray_Region, string in_strRegionName)
		{


			IPointCollection4 p_PointColl;
			IPoint[] p_Points;
			IGeometryBridge2 p_GeoBrg;
			IPolygon p_poly = (Polygon) p_serverContext.createObject("esriGeometry.Polygon");
			try
			{
				p_Points = new Point[in_dblArray_Region.Length];

				p_GeoBrg = (IGeometryBridge2) p_serverContext.createObject("esriGeometry.GeometryEnvironment");


				p_PointColl = (Polygon) p_serverContext.createObject("esriGeometry.Polygon");

				IGeoTransformation iGeoTransformation = (IGeoTransformation) m_OutsystemTransformer.m_getSpatialReferenceenvironment().createGeoTransformation(esriSRGeoTransformation2Type.esriSRGeoTransformation_NZGD1949_TO_NZGD2000_NTV2);


				int p_count = 0;
				for (int i = 0; i < in_dblArray_Region.Length; i++)
				{

					Point p_tempPoint = (Point) p_serverContext.createObject("esriGeometry.Point");

					p_tempPoint.putCoords(in_dblArray_Region[i + 1], in_dblArray_Region[i]);
					i++;
					p_tempPoint.SpatialReferenceByRef = m_incommingRegionalCoordinateSystem;
					p_tempPoint.projectEx(m_OutsystemTransformer.m_getProjectedCoordinateSystem(), esriTransformDirection.esriTransformForward, iGeoTransformation, false, 0.0, 0.0);

					double x = p_tempPoint.X;
					double y = p_tempPoint.Y;

					p_Points[p_count++] = p_tempPoint;


				}

				p_GeoBrg.setPoints(p_PointColl, p_Points);
				 p_poly = (IPolygon) p_PointColl;
				p_poly.SpatialReferenceByRef = m_outGoingCoordinateSystem;
				//p_poly.project(m_outGoingCoordinateSystem);

				IGeometry2 pGeoMetry2 = (IGeometry2)p_poly;
				/*
				  
				 pGeoMetry2.projectEx(m_OutsystemTransformer
						.m_getProjectedCoordinateSystem(),
						esriTransformDirection.esriTransformForward,
						iGeoTransformation, false, 0.0, 0.0);
				
			*/

				//GNSRegionsMapperSDE m_GNSRegionsMapper = new GNSRegionsMapperSDE(
				//		super.getSdeAccess());

				// The check to see if the data is in the table already
				bool p_blnDoesExist = m_GNSRegionsMapper.m_doesExist(AffectedAreaDB.gnsRegionsTableName, AffectedAreaDB.regionIDFieldName, in_strRegionName);


				ITopologicalOperator p_topology1 = (ITopologicalOperator) pGeoMetry2;
				p_topology1.simplify();
				//p_topology1.clip(m_validateEnvelope);

				p_poly = (Polygon) p_topology1;
				//p_poly.close();
				double p_polyArea = ((Polygon) p_poly).Area;
				if (p_polyArea <= 0)
				{
					Console.WriteLine("Empty Region Created--------------");
				}

				if (p_blnDoesExist)
				{
					m_GNSRegionsMapper.m_updateRegion((Polygon)p_poly, in_strRegionName);
				}
				else
				{
					m_GNSRegionsMapper.m_createRegion((Polygon)p_poly, in_strRegionName);
				}

			}

			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			return (Polygon)p_poly;
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void Validate() throws Exception
		private void Validate()
		{


			 m_validateEnvelope = (Envelope) p_serverContext.createObject("esriGeometry.Envelope");
			 m_validateEnvelope.XMin = X_MIN_ENVELOP;
			 m_validateEnvelope.YMin = Y_MIN_ENVELOP;

			 m_validateEnvelope.XMax = X_MAX_ENVELOP;
			 m_validateEnvelope.YMax = Y_MAX_ENVELOP;
			m_validateEnvelope.SpatialReferenceByRef = m_incommingCoordinateSystem;

			m_epiCenter = (IPoint) p_serverContext.createObject("esriGeometry.Point");
			m_epiCenter.X = m_dbl_Epicentre_NZmgE;
			m_epiCenter.Y = m_dbl_Epicentre_NZmgN;

			m_epiCenter.SpatialReferenceByRef = m_incommingCoordinateSystem;



			if (!m_validateEnvelope.contains(m_epiCenter))
			{
				IParameterList p_params = new ParameterList();
				p_params.m_addParameter("message","Epi Center Outside Defined Boundary (NZMG)");

				p_params.m_addParameter("EPI Center Easting",Convert.ToString(m_dbl_Epicentre_NZmgE));
				p_params.m_addParameter("EPI Center Northing",Convert.ToString(m_dbl_Epicentre_NZmgN));

				p_params.m_addParameter("Defined Boundary Envelop  XMIN",Convert.ToString(X_MIN_ENVELOP));

				p_params.m_addParameter("Defined Boundary Envelop  XMIN",Convert.ToString(X_MIN_ENVELOP));
				p_params.m_addParameter("Defined Boundary Envelop  YMIN",Convert.ToString(Y_MIN_ENVELOP));
				p_params.m_addParameter("Defined Boundary Envelop  XMAX",Convert.ToString(X_MAX_ENVELOP));
				p_params.m_addParameter("Defined Boundary Envelop  YMAX",Convert.ToString(X_MAX_ENVELOP));
				throw new FrameworkException(FrameworkExceptionType.parameterNotValid, p_params);
			}

			// Transform the EPI CENTER from NZMG to NZTM
			IPoint p_tempPoint = (IPoint) p_serverContext.createObject("esriGeometry.Point");

			p_tempPoint.putCoords(m_dbl_Epicentre_NZmgE, m_dbl_Epicentre_NZmgN);
			IGeometry2 p_igeo2ONPoint = (IGeometry2) p_tempPoint;

			p_igeo2ONPoint.SpatialReferenceByRef = m_incommingCoordinateSystem;

			p_igeo2ONPoint.project(m_OutsystemTransformer.m_getProjectedCoordinateSystem());

			IPoint p_convertedPoint = (IPoint)p_igeo2ONPoint;
			if (p_convertedPoint != null)
			{
				logger.debug("p_convertedPoint.isEmpty()=" + p_convertedPoint.Empty);
			}
			else
			{
				logger.debug("p_convertedPoint is null");
			}

			if (p_convertedPoint == null || p_convertedPoint.Empty)
			{
				IParameterList p_params = new ParameterList();
				p_params.m_addParameter("message","Epi Center Outside Defined Boundary (NZMG)");

				p_params.m_addParameter("EPI Center Easting",Convert.ToString(m_dbl_Epicentre_NZmgE));
				p_params.m_addParameter("EPI Center Northing",Convert.ToString(m_dbl_Epicentre_NZmgN));

				p_params.m_addParameter("Defined Boundary Envelop  XMIN",Convert.ToString(X_MIN_ENVELOP));

				p_params.m_addParameter("Defined Boundary Envelop  XMIN",Convert.ToString(X_MIN_ENVELOP));
				p_params.m_addParameter("Defined Boundary Envelop  YMIN",Convert.ToString(Y_MIN_ENVELOP));
				p_params.m_addParameter("Defined Boundary Envelop  XMAX",Convert.ToString(X_MAX_ENVELOP));
				p_params.m_addParameter("Defined Boundary Envelop  YMAX",Convert.ToString(X_MAX_ENVELOP));
				throw new FrameworkException(FrameworkExceptionType.parameterNotValid, p_params);
			}

			m_dbl_Epicentre_NZtmE = p_convertedPoint.X;
			m_dbl_Epicentre_NZtmN = p_convertedPoint.Y;
			// transform the Validation envelope to NZTM
			m_validateEnvelope.project(m_OutsystemTransformer.m_getProjectedCoordinateSystem());


			m_MZTM_EpiCenter = (IPoint) p_serverContext.createObject("esriGeometry.Point");

			m_MZTM_EpiCenter.X = m_dbl_Epicentre_NZtmE;
			m_MZTM_EpiCenter.Y = m_dbl_Epicentre_NZtmN;
			m_MZTM_EpiCenter.SpatialReferenceByRef = m_outGoingCoordinateSystem;


			double xmin = m_validateEnvelope.XMin;
			double ymin = m_validateEnvelope.YMin;
			double xmax = m_validateEnvelope.XMax;
			double ymax = m_validateEnvelope.YMax;

			/*
			  
			 if (m_dbl_Depth_to_fault_centriod_KM_hc < 0) {
				
				IParameterList p_params = new ParameterList();
				p_params.m_addParameter("message","Depth to Fault Centriod cannot be Negative");
				p_params.m_addParameter("hcdepth", String.valueOf(m_dbl_Depth_to_fault_centriod_KM_hc));
				
				throw new FrameworkException(FrameworkExceptionType.parameterNotValid, p_params);
			}
			
			if ((m_dbl_Magnitude <= 0)||(m_dbl_Magnitude >= 20)) {
				
				IParameterList p_params = new ParameterList();
				p_params.m_addParameter("magnitude", String.valueOf(m_dbl_Magnitude));
				p_params.m_addParameter("message", "Magnitude values have to be 0< Magnitude < 20");
				
				throw new FrameworkException(FrameworkExceptionType.parameterNotValid, p_params);
			}		
			
			if (m_dbl_DepthToFaultTop_KM_ht < 0) {
				IParameterList p_params = new ParameterList();
				p_params.m_addParameter("Depth To Top", String.valueOf(m_dbl_DepthToFaultTop_KM_ht));
				p_params.m_addParameter("message", "Depth to Fault Top cannot be Negative");
				
				throw new FrameworkException(FrameworkExceptionType.parameterNotValid, p_params);
			}	
			*/
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void m_generateQuake() throws Exception
		private void m_generateQuake()
		{


			int p_intIsoseimalID;
			switch (m_chr_Mechanism)
			{
			case 'D': // deep earthquake, no specific mechanism allowed for
				IntIndexer = 2;
				if (m_dbl_DepthToFaultTop_KM_ht < 50)
				{
					m_dbl_DepthToFaultTop_KM_ht = m_dbl_Depth_to_fault_centriod_KM_hc;
				}
				break;

			case 'X': // unknown mechanism, use main seismic region model, crustal
				IntIndexer = 1;
				m_dbl_crint = 1.0;
				break;

			case 'S':
				IntIndexer = 0;
				m_dbl_ss = 1.0;
				break;

			case 'R':
				IntIndexer = 0;
				m_dbl_rev = 1.0;
				break;
			case 'N': // crustal normal
				IntIndexer = 0;
				break;
			case 'I': // Interface, reverse mechanism assumed
				IntIndexer = 0;
				m_dbl_crint = 1.0;
				m_dbl_rev = 1.0;
				break;
			case 'V': // central volcanic region, normal mechanism assumed
				IntIndexer = 0;
				m_dbl_volc = 1.0;
				break;
			default:
				// print*, mech // ' is a non-standard mechanism dummy. "X" will be
				// assumed'
				IntIndexer = 1;
				m_dbl_crint = 1.0;
				break;

			}


			// ------------------------------------------------------------------------------
			// Calculate major and minor axes of the isoseismals (D&R unit is km)
			// ------------------------------------------------------------------------------

			m_dbl_a2[IntIndexer] = m_dbl_a2[IntIndexer] + m_dbl_a2r[IntIndexer] * m_dbl_rev + m_dbl_a2v[IntIndexer] * m_dbl_volc;
			m_dbl_b2[IntIndexer] = m_dbl_b2[IntIndexer] + m_dbl_b2r[IntIndexer] * m_dbl_rev + m_dbl_b2v[IntIndexer] * m_dbl_volc;
			m_dbl_a3[IntIndexer] = m_dbl_a3[IntIndexer] + m_dbl_a3s[IntIndexer] * m_dbl_ss + m_dbl_a3v[IntIndexer] * m_dbl_volc;
			m_dbl_b3[IntIndexer] = m_dbl_b3[IntIndexer] + m_dbl_b3s[IntIndexer] * m_dbl_ss + m_dbl_b3v[IntIndexer] * m_dbl_volc;

			for (p_intIsoseimalID = m_isoseismalMin; p_intIsoseimalID <= m_isoseismalMax; p_intIsoseimalID++)
			{
				m_dbl_xa = 2.0 * (p_intIsoseimalID - m_dbl_a1[IntIndexer] - m_dbl_a2[IntIndexer] * m_dbl_Magnitude - m_dbl_a4[IntIndexer] * m_dbl_Depth_to_fault_centriod_KM_hc - m_dbl_a5[IntIndexer] * m_dbl_crint) / m_dbl_a3[IntIndexer];
				m_dbl_xa = Math.Pow(10, m_dbl_xa) - m_dbl_ad[IntIndexer] * m_dbl_ad[IntIndexer] - m_dbl_DepthToFaultTop_KM_ht * m_dbl_DepthToFaultTop_KM_ht;
				if (m_dbl_xa > 0.0)
				{
					m_dblArray_ra[p_intIsoseimalID - 1] = Math.Sqrt(m_dbl_xa);
				}

				m_dblArray_ra[p_intIsoseimalID - 1] = m_dblArray_ra[p_intIsoseimalID - 1] * 1000; // convert
				// km
				// to m
				m_dbl_xb = 2.0 * (p_intIsoseimalID - m_dbl_b1[IntIndexer] - m_dbl_b2[IntIndexer] * m_dbl_Magnitude - m_dbl_b4[IntIndexer] * m_dbl_Depth_to_fault_centriod_KM_hc - m_dbl_b5[IntIndexer] * m_dbl_crint) / m_dbl_b3[IntIndexer];
				m_dbl_xb = Math.Pow(10, m_dbl_xb) - m_dbl_bd[IntIndexer] * m_dbl_bd[IntIndexer] - m_dbl_DepthToFaultTop_KM_ht * m_dbl_DepthToFaultTop_KM_ht;

				if (m_dbl_xb > 0.0)
				{
					m_dblArray_rb[p_intIsoseimalID - 1] = Math.Sqrt(m_dbl_xb);
				}

				m_dblArray_rb[p_intIsoseimalID - 1] = m_dblArray_rb[p_intIsoseimalID - 1] * 1000; // convert
				// km
				// to m

			}

			// ------------------------------------------------------------------------------
			// Generate isoseismal plot data
			// ------------------------------------------------------------------------------

			m_dbl_d2r = Math.Atan(1.0) / 45.0; // ! degrees to radians
			m_dbl_ca = Math.Cos(m_dbl_StrikeDegNorthOfE * m_dbl_d2r);
			m_dbl_sa = Math.Sin(m_dbl_StrikeDegNorthOfE * m_dbl_d2r);


				for (int j = 0; j < 23; j++)
				{
					for (p_intIsoseimalID = m_isoseismalMin; p_intIsoseimalID <= m_isoseismalMax; p_intIsoseimalID++)
					{
						m_dbl_x = m_dblArray_rb[p_intIsoseimalID - 1] * m_dbl_Array_xrel[j];
						m_dbl_y = m_dblArray_ra[p_intIsoseimalID - 1] * Math.Sqrt(1 - Math.Pow(m_dbl_Array_xrel[j], 2));
						dblArray_xr[p_intIsoseimalID - 1][j] = m_dbl_x * m_dbl_ca + m_dbl_y * m_dbl_sa + m_dbl_Epicentre_NZtmE;
						dblArray_yr[p_intIsoseimalID - 1][j] = -m_dbl_x * m_dbl_sa + m_dbl_y * m_dbl_ca + m_dbl_Epicentre_NZtmN;
						m_createIsoseismale(p_intIsoseimalID, Math.Round(dblArray_xr[p_intIsoseimalID - 1][j]), Math.Round(dblArray_yr[p_intIsoseimalID - 1][j]));
					}
				}

				for (int j = 23; j < 45; j++)
				{
					for (p_intIsoseimalID = m_isoseismalMin; p_intIsoseimalID <= m_isoseismalMax; p_intIsoseimalID++)
					{
						m_dbl_x = m_dblArray_rb[p_intIsoseimalID - 1] * m_dbl_Array_xrel[j];
						m_dbl_y = -m_dblArray_ra[p_intIsoseimalID - 1] * Math.Sqrt(1 - Math.Pow(m_dbl_Array_xrel[j], 2));
						dblArray_xr[p_intIsoseimalID - 1][j] = m_dbl_x * m_dbl_ca + m_dbl_y * m_dbl_sa + m_dbl_Epicentre_NZtmE;
						dblArray_yr[p_intIsoseimalID - 1][j] = -m_dbl_x * m_dbl_sa + m_dbl_y * m_dbl_ca + m_dbl_Epicentre_NZtmN;
						m_createIsoseismale(p_intIsoseimalID, Math.Round(dblArray_xr[p_intIsoseimalID - 1][j]), Math.Round(dblArray_yr[p_intIsoseimalID - 1][j]));
					}
				}


		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void m_createIsoseismale(int in_intIsoseismaleID, double in_dblX, double in_dblY) throws Exception
		private void m_createIsoseismale(int in_intIsoseismaleID, double in_dblX, double in_dblY)
		{

			IPoint p_tempPoint = (IPoint) p_serverContext.createObject("esriGeometry.Point");
			p_tempPoint.putCoords(in_dblX, in_dblY);

			ArrayList m_tempList = (ArrayList) m_ListIsoseismalList[in_intIsoseismaleID - 1];

			if (m_tempList == null)
			{
				m_tempList = new ArrayList();
			}

			m_tempList.Add(p_tempPoint);
			m_ListIsoseismalList[in_intIsoseismaleID - 1] = m_tempList;
		}

		private void printResults()
		{
			try
			{
				for (int I = m_isoseismalMin; I <= m_isoseismalMax; I++)
				{
					ArrayList t = (ArrayList) m_ListIsoseismalList[I - 1];
					Console.WriteLine("FOR MM" + (I));
					for (int j = 0; j < t.Count; j++)
					{
						Point p = (Point) t[j];
						if (!p.Empty)
						{
							Console.WriteLine(" Point number " + (j + 1) + "  X = " + p.X + " Y = " + p.Y);
						}
					}
				}
				Console.WriteLine("Done--");
			}
			catch (java.io.IOException e)
			{
				Console.WriteLine("I/O Exception at Coordinate Transformer");
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private java.util.Vector m_MakePolygons() throws Exception
		private ArrayList m_MakePolygons()
		{
			ArrayList p_vtrPolygon = new ArrayList();

			IPolygon prev_poly;
				IGeometryBridge2 p_GeoBrg;
				p_GeoBrg = (IGeometryBridge2) p_serverContext.createObject("esriGeometry.GeometryEnvironment");
				IPointCollection4 p_PointColl; // = new Polygon();

				for (int p_intIsoseimalID = m_isoseismalMin; p_intIsoseimalID <= m_isoseismalMax; p_intIsoseimalID++)
				{
					ArrayList p_points = (ArrayList) m_ListIsoseismalList[p_intIsoseimalID - 1];
					IPoint[] p_Points = new Point[p_points.Count];
					for (int p = 0; p < p_points.Count; p++)
					{
						p_Points[p] = (Point) p_serverContext.createObject("esriGeometry.Point");
					}

					for (int p_pointCount = 0; p_pointCount < p_points.Count; p_pointCount++)
					{
						IPoint p_tempPoint = (IPoint) p_points[p_pointCount];

						if (!p_tempPoint.Empty)
						{
							(p_Points[p_pointCount]).X = p_tempPoint.X;
							(p_Points[p_pointCount]).Y = p_tempPoint.Y;
						}

					}
					p_PointColl = (Polygon) p_serverContext.createObject("esriGeometry.Polygon");
					p_GeoBrg.setPoints(p_PointColl, p_Points);
					IPolygon p_poly = (IPolygon) p_PointColl;
					p_poly.SpatialReferenceByRef = m_outGoingCoordinateSystem;
					p_poly.project(m_outGoingCoordinateSystem);


					ITopologicalOperator2 p_topology = (ITopologicalOperator2) p_PointColl;
					//p_topology.clip(m_validateEnvelope);



					p_poly = (Polygon) p_topology;

					ITopologicalOperator p_topology1 = (ITopologicalOperator) p_poly;
					p_topology1.simplify();
					p_topology1.clip(m_validateEnvelope);
					p_poly = (Polygon) p_topology1;

					double p_polyArea = ((Polygon) p_poly).Area;
					if (p_polyArea > 0)
					{

							p_vtrPolygon.Add(p_poly);
							if (!IsoSeismalDone)
							{
								IntMaxIsoseismal = p_intIsoseimalID;
								m_activePolygone = (Polygon)p_poly;
								IsoSeismalDone = true;
							}

							BandOrder.Add(p_intIsoseimalID);

					}
				}

			/*
			if 	(p_vtrPolygon.size()>0){
				for(int polyIndex=p_vtrPolygon.size();polyIndex>1;polyIndex--){
					Polygon p_resultPoly = (Polygon)GeometryUtil.m_getDifferenceFromPolygons((IPolygon)p_vtrPolygon.get(polyIndex-2), 
							(IPolygon)p_vtrPolygon.get(polyIndex-1));
					
				
						p_vtrPolygon.set(polyIndex-1, p_resultPoly);
					
				}
			}
			*/

			return p_vtrPolygon;
		}
	}

}