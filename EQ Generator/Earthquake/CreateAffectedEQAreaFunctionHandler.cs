using System;
using System.Collections;
using System.Collections.Generic;

namespace eagle.workflow.affectedarea
{


	using Logger = org.apache.log4j.Logger;

	using Polygon = com.esri.arcgis.geometry.Polygon;
	using IServerContext = com.esri.arcgis.server.IServerContext;

	using IDatabaseAccess = eagle.framework.dal.connections.IDatabaseAccess;
	using FrameworkException = eagle.framework.exception.FrameworkException;
	using FrameworkExceptionType = eagle.framework.exception.FrameworkExceptionType;
	using IParameterList = eagle.framework.exception.IParameterList;
	using ParameterList = eagle.framework.exception.ParameterList;
	using Factory = eagle.framework.factory.Factory;
	using FunctionHandler = eagle.framework.function.FunctionHandler;
	using ResponseEnum = eagle.framework.response.ResponseEnum;
	using ResponseObj = eagle.framework.response.ResponseObj;
	using FrameworkLogger = eagle.framework.utility.FrameworkLogger;

	public class CreateAffectedEQAreaFunctionHandler : FunctionHandler
	{
		internal readonly int NON_ACTIVEBANDID = 999;
		internal static Logger logger = FrameworkLogger.getLogger(typeof(CreateAffectedEQAreaFunctionHandler));

		public CreateAffectedEQAreaFunctionHandler(IDatabaseAccess databaseAccess, IDatabaseAccess sdeAccess, IServerContext serverContext) : base(databaseAccess, sdeAccess, serverContext)
		{

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public eagle.framework.response.ResponseObj m_execute(AffectedArea in_affectedArea, java.util.Vector in_ListIsoseismalList, java.util.Vector in_BandOrder, com.esri.arcgis.geometry.Polygon in_activePolygon, eagle.workflow.WorkflowType in_workflow) throws Exception
		public virtual ResponseObj m_execute(AffectedArea in_affectedArea, ArrayList in_ListIsoseismalList, ArrayList in_BandOrder, Polygon in_activePolygon, WorkflowType in_workflow)
		{

			int p_intActiveBandID = in_affectedArea.ActiveBand;

			in_affectedArea.Source = "GNS : DOWRICK RHODES 1999 V2";
			if (p_intActiveBandID == 0)
			{
				in_affectedArea.ActiveBand = NON_ACTIVEBANDID;
				in_affectedArea.Source = "GNS-DOWRICK RHODES 1999 V2  - No Valid Isoseismals";
				IParameterList p_params = new ParameterList();
				p_params.m_addParameter("message","No Valid Isoseismals were created ");
			}

			in_affectedArea.GenericEvent = false;

			AffectedAreaMapperSDE p_affectedAreaMapperSde = new AffectedAreaMapperSDE(base.SdeAccess);

			// The check to see if the data is in the table already
			bool p_blnDoesExist = p_affectedAreaMapperSde.m_doesExist(AffectedAreaDB.generalTableName, AffectedAreaDB.eventIDFieldName, in_affectedArea.EventID);

			ResponseObj p_ResponseObject = new ResponseObj();

			if (p_blnDoesExist)
			{

				logger.debug("EventID already exists, checking senderRefID");

				AffectedAreaDetailsMapperSDE p_affectedAreaDetailsMapperSde = new AffectedAreaDetailsMapperSDE(base.SdeAccess);

				// Check if senderRefID matches
				// if it does
				if (p_affectedAreaDetailsMapperSde.m_senderRefIDMatches(in_affectedArea))
				{
					logger.debug("SenderRefID matches");
					// Do nothing, leave the flow to lead this path to success :D

				}
				else
				{
					logger.debug("SenderRefID doesn't match");
					IParameterList p_params = new ParameterList();
					p_params.m_addParameter("eventID", in_affectedArea.EventID);

					throw new FrameworkException(FrameworkExceptionType.dbIDAlreadyExists, p_params);
				}
			}
			else if (in_ListIsoseismalList.Count > 0)
			{

				for (int p_intBandCount = 0; p_intBandCount < in_ListIsoseismalList.Count; p_intBandCount++)
				{

					List<int?> p_arrBandIDs = new List<int?>();

					int p_intBandID = (int)((int?) in_BandOrder[p_intBandCount]);

					p_arrBandIDs.Add(p_intBandID);
					in_affectedArea.BandID = p_arrBandIDs;

					p_affectedAreaMapperSde.m_storeAffectedArea((Polygon) in_ListIsoseismalList[p_intBandCount], in_affectedArea);

				}
			}
			else
			{

				p_affectedAreaMapperSde.m_createEmptyAffectedArea(in_affectedArea);
			}

			p_ResponseObject.m_addKeyAndValue("AffectedArea", in_affectedArea);
			//p_ResponseObject.m_addKeyAndValue("Shape", in_ListIsoseismalList);
			if (in_activePolygon.Empty)
			{
				Console.WriteLine("ACTIVE polygone is Empty in the Create Affected Area");
			}
			else
			{
				Console.WriteLine("ACTIVE polygone is NOT  Empty in the Create Affected Area");
			}

			p_ResponseObject.m_addKeyAndValue("Shape", in_activePolygon);

			p_ResponseObject.m_addKeyAndValue("Status", ResponseEnum.Success);

			// Validate the response
			Factory.m_validateReqResObject("/eagle/framework/configuration/WorkflowAffectedAreaConfig.xml", "WF_Config_CreateEventEQDetails_Response", p_ResponseObject.get_params());

			return p_ResponseObject;
		}

	}

}