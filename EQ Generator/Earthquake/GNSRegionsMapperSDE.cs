namespace eagle.workflow.affectedarea
{

	using ConfigurationLoader = eagle.framework.configuration.ConfigurationLoader;
	using ConfigurationType = eagle.framework.configuration.ConfigurationType;
	using DatabaseAccessSDE = eagle.framework.dal.connections.DatabaseAccessSDE;
	using IDatabaseAccess = eagle.framework.dal.connections.IDatabaseAccess;
	using DatabaseMapperSDE = eagle.framework.dal.mappers.DatabaseMapperSDE;


	using IFeature = com.esri.arcgis.geodatabase.IFeature;
	using IFeatureBuffer = com.esri.arcgis.geodatabase.IFeatureBuffer;
	using IFeatureClass = com.esri.arcgis.geodatabase.IFeatureClass;
	using IFeatureCursor = com.esri.arcgis.geodatabase.IFeatureCursor;
	using IFields = com.esri.arcgis.geodatabase.IFields;
	using IQueryFilter = com.esri.arcgis.geodatabase.IQueryFilter;
	using ITable = com.esri.arcgis.geodatabase.ITable;
	using QueryFilter = com.esri.arcgis.geodatabase.QueryFilter;
	using IGeometry = com.esri.arcgis.geometry.IGeometry;
	using Polygon = com.esri.arcgis.geometry.Polygon;

	using FrameworkException = eagle.framework.exception.FrameworkException;
	using FrameworkExceptionType = eagle.framework.exception.FrameworkExceptionType;
	using IParameterList = eagle.framework.exception.IParameterList;
	using ParameterList = eagle.framework.exception.ParameterList;

	public class GNSRegionsMapperSDE : DatabaseMapperSDE
	{
		protected internal readonly string generalSchemaName;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public GNSRegionsMapperSDE(eagle.framework.dal.connections.IDatabaseAccess in_sdeAccess) throws Exception
		public GNSRegionsMapperSDE(IDatabaseAccess in_sdeAccess) : base((DatabaseAccessSDE) in_sdeAccess)
		{


			generalSchemaName = ConfigurationLoader.getString(ConfigurationType.sdeSchema.m_getValue());
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void m_createRegion(com.esri.arcgis.geometry.Polygon in_polygon, String in_strRegionName) throws Exception
		public virtual void m_createRegion(Polygon in_polygon, string in_strRegionName)
		{
			IFeatureClass p_featureClass = base.featureWorkspace.openFeatureClass(generalSchemaName + "." + AffectedAreaDB.gnsRegionsTableName);

			IFeatureBuffer p_featureBuffer = p_featureClass.createFeatureBuffer();

			IFields p_ifields = p_featureBuffer.Fields;

			p_featureBuffer.setValue(p_ifields.findField(AffectedAreaDB.regionIDFieldName), in_strRegionName);

			p_featureBuffer.ShapeByRef = (IGeometry) in_polygon;

			IFeatureCursor p_insertCursor = p_featureClass.IFeatureClass_insert(true);

			p_insertCursor.insertFeature(p_featureBuffer);

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void m_updateRegion(com.esri.arcgis.geometry.Polygon in_polygon, String in_strRegionName)throws Exception
		public virtual void m_updateRegion(Polygon in_polygon, string in_strRegionName)
		{

			IFeatureClass p_featureClass = base.featureWorkspace.openFeatureClass(generalSchemaName + "." + AffectedAreaDB.gnsRegionsTableName);


			IQueryFilter p_iqueryFilter = (IQueryFilter) base.serverContext.createObject(QueryFilter.Clsid);
			p_iqueryFilter.WhereClause = string.Format("{0} = '{1}'", AffectedAreaDB.regionIDFieldName, in_strRegionName);

			IFeatureCursor p_icursor = p_featureClass.search(p_iqueryFilter, false);

			IFeature p_feature = p_icursor.nextFeature();

			p_feature.setValue(p_feature.Fields.findField(AffectedAreaDB.regionIDFieldName), in_strRegionName);

			p_feature.ShapeByRef = (IGeometry) in_polygon;

			p_feature.store();

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public com.esri.arcgis.geometry.Polygon m_getRegionalPolygone(String in_strRegionName)throws Exception
	public virtual Polygon m_getRegionalPolygone(string in_strRegionName)
	{
			Polygon p_resultPolygon;
			IFeatureClass p_featureClass = base.featureWorkspace.openFeatureClass(generalSchemaName + "." + AffectedAreaDB.gnsRegionsTableName);


			IQueryFilter p_iqueryFilter = (IQueryFilter) base.serverContext.createObject(QueryFilter.Clsid);
			p_iqueryFilter.WhereClause = string.Format("{0} = '{1}'", AffectedAreaDB.regionIDFieldName, in_strRegionName);

			IFeatureCursor p_icursor = p_featureClass.search(p_iqueryFilter, false);

			IFeature p_feature = p_icursor.nextFeature();

			p_resultPolygon = (Polygon)p_feature.Shape;
			return p_resultPolygon;
	}

	}

}