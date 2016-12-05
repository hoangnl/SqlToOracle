using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Xml;
using System.Collections;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace SqlToOracle.Core.Connect
{
    public class OracleHelper
    {
		#region private utility methods & constructors

		private OracleHelper() {}

		private static void AttachParameters(OracleCommand command, OracleParameter[] commandParameters)
		{
			foreach (OracleParameter p in commandParameters)
			{
				//check for derived output value with no value assigned
				if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
				{
					p.Value = DBNull.Value;
				}				
				command.Parameters.Add(p);
			}
		}

		private static void AssignParameterValues(OracleParameter[] commandParameters, object[] parameterValues)
		{
			if ((commandParameters == null) || (parameterValues == null)) 
			{
				//do nothing if we get no data
				return;
			}

			// we must have the same number of values as we pave parameters to put them in
			if (commandParameters.Length != parameterValues.Length)
			{
				throw new ArgumentException("Parameter count does not match Parameter Value count.");
			}

			//iterate through the OracleParameters, assigning the values from the corresponding position in the 
			//value array
			for (int i = 0, j = commandParameters.Length; i < j; i++)
			{
				commandParameters[i].Value = parameterValues[i];
			}
		}
		
		private static void PrepareCommand(OracleCommand command, OracleConnection connection, OracleTransaction transaction, CommandType commandType, string commandText, OracleParameter[] commandParameters)
		{
			//if the provided connection is not open, we will open it
			if (connection.State != ConnectionState.Open)
			{
				connection.Open();
			}

			//associate the connection with the command
			command.Connection = connection;

			//set the command text (stored procedure name or SQL statement)
			command.CommandText = commandText;

			//if we were provided a transaction, assign it.
			if (transaction != null)
			{
				//command.Transaction = transaction;
			}

			//set the command type
			command.CommandType = commandType;

			//attach the command parameters if they are provided
			if (commandParameters != null)
			{
				AttachParameters(command, commandParameters);
			}

			return;
		}

		#endregion private utility methods & constructors

		#region ExecuteNonQuery
		
		public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteNonQuery(connectionString, commandType, commandText, (OracleParameter[])null);
		}
		
		public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
		{
			//create & open a OracleConnection, and dispose of it after we are done.
			using (OracleConnection cn = new OracleConnection(connectionString))
			{
				cn.Open();

				//call the overload that takes a connection in place of the connection string
				return ExecuteNonQuery(cn, commandType, commandText, commandParameters);
			}
		}

        public static int ExecuteNonQuery(string connectionString, string spName, params OracleParameter[] commandParameters)
		{
			//if we receive parameter values, we need to figure out where they go
            if ((commandParameters != null) && (commandParameters.Length > 0))			
            {
				//call the overload that takes an array of OracleParameters
				return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
			}
		}

        public static int ExecuteNonQueryByText(string connectionString, string spName, params OracleParameter[] commandParameters)
        {
            //if we receive parameter values, we need to figure out where they go
            if ((commandParameters != null) && (commandParameters.Length > 0))
            {
                //call the overload that takes an array of OracleParameters
                return ExecuteNonQuery(connectionString, CommandType.Text, spName, commandParameters);
            }
            //otherwise we can just call the SP without params
            else
            {
                return ExecuteNonQuery(connectionString, CommandType.Text, spName);
            }
        }
		
		public static int ExecuteNonQuery(OracleConnection connection, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteNonQuery(connection, commandType, commandText, (OracleParameter[])null);
		}
		
		public static int ExecuteNonQuery(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
		{	
			//create a command and prepare it for execution
			OracleCommand cmd = new OracleCommand();
			PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);
			
			//finally, execute the command.
			int retval = cmd.ExecuteNonQuery();
			
			// detach the OracleParameters from the command object, so they can be used again.
			cmd.Parameters.Clear();
			return retval;
		}
		
		public static int ExecuteNonQuery(OracleConnection connection, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OracleDataAccessParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
			}
		}
		
		public static int ExecuteNonQuery(OracleTransaction transaction, CommandType commandType, string commandText)
		{
			return ExecuteNonQuery(transaction, commandType, commandText, (OracleParameter[])null);
		}
		
		public static int ExecuteNonQuery(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			OracleCommand cmd = new OracleCommand();
			PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
			
			//finally, execute the command.
			int retval = cmd.ExecuteNonQuery();
			
			// detach the OracleParameters from the command object, so they can be used again.
			cmd.Parameters.Clear();
			return retval;
		}
		
		public static int ExecuteNonQuery(OracleTransaction transaction, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OracleDataAccessParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
			}
		}

		#endregion ExecuteNonQuery

		#region ExecuteDataSet
		
		public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteDataset(connectionString, commandType, commandText, (OracleParameter[])null);
		}
		
		public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
		{
			//create & open a OracleConnection, and dispose of it after we are done.
			using (OracleConnection cn = new OracleConnection(connectionString))
			{
				cn.Open();

				//call the overload that takes a connection in place of the connection string
				return ExecuteDataset(cn, commandType, commandText, commandParameters);
			}
		}

        public static DataSet ExecuteDataset(string connectionString, string spName, params OracleParameter[] commandParameters)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((commandParameters != null) && (commandParameters.Length > 0)) 
			{
				//call the overload that takes an array of OracleParameters
				return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
			}
		}
		
		public static DataSet ExecuteDataset(OracleConnection connection, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteDataset(connection, commandType, commandText, (OracleParameter[])null);
		}		
		
		public static DataSet ExecuteDataset(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			OracleCommand cmd = new OracleCommand();
			PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);
			
			//create the DataAdapter & DataSet
			OracleDataAdapter da =  new OracleDataAdapter(cmd);
			DataSet ds = new DataSet();

			//fill the DataSet using default values for DataTable names, etc.
			da.Fill(ds);
			
			// detach the OracleParameters from the command object, so they can be used again.			
			cmd.Parameters.Clear();
			
			//return the dataset
			return ds;						
		}		
		
		public static DataSet ExecuteDataset(OracleConnection connection, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OracleDataAccessParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
			}
		}
		
		public static DataSet ExecuteDataset(OracleTransaction transaction, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteDataset(transaction, commandType, commandText, (OracleParameter[])null);
		}		
		
		public static DataSet ExecuteDataset(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			OracleCommand cmd = new OracleCommand();
			PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
			
			//create the DataAdapter & DataSet
			OracleDataAdapter da = new OracleDataAdapter(cmd);
			DataSet ds = new DataSet();

			//fill the DataSet using default values for DataTable names, etc.
			da.Fill(ds);
			
			// detach the OracleParameters from the command object, so they can be used again.
			cmd.Parameters.Clear();
			
			//return the dataset
			return ds;
		}		
		
		public static DataSet ExecuteDataset(OracleTransaction transaction, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OracleDataAccessParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
			}
		}

		#endregion ExecuteDataSet
		
		#region ExecuteReader
		
		private enum OracleConnectionOwnership	
		{
			/// <summary>Connection is owned and managed by SqlHelper</summary>
			Internal, 
			/// <summary>Connection is owned and managed by the caller</summary>
			External
		}
		
		private static OracleDataReader ExecuteReader(OracleConnection connection, OracleTransaction transaction, CommandType commandType, string commandText, OracleParameter[] commandParameters, OracleConnectionOwnership connectionOwnership)
		{	
			//create a command and prepare it for execution
			OracleCommand cmd = new OracleCommand();
			PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters);
			
			//create a reader
			OracleDataReader dr;

			// call ExecuteReader with the appropriate CommandBehavior
			if (connectionOwnership == OracleConnectionOwnership.External)
			{
				dr = cmd.ExecuteReader();
			}
			else
			{
				dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
			}
			
			// detach the OracleParameters from the command object, so they can be used again.
			cmd.Parameters.Clear();
			
			return dr;
		}
		
		public static OracleDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteReader(connectionString, commandType, commandText, (OracleParameter[])null);
		}
		
		public static OracleDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
		{
			//create & open a OracleConnection
			OracleConnection cn = new OracleConnection(connectionString);
			cn.Open();

			try
			{
				//call the private overload that takes an internally owned connection in place of the connection string
				return ExecuteReader(cn, null, commandType, commandText, commandParameters,OracleConnectionOwnership.Internal);
			}
			catch
			{
				//if we fail to return the SqlDatReader, we need to close the connection ourselves
				cn.Close();
				throw;
			}
		}

        public static OracleDataReader ExecuteReader(string connectionString, string spName, params OracleParameter[] commandParameters)
        {
            //if we receive parameter values, we need to figure out where they go
            if ((commandParameters != null) && (commandParameters.Length > 0))
            {
                //call the overload that takes an array of OracleParameters
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params
            else
            {
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
        }
		
		public static OracleDataReader ExecuteReader(OracleConnection connection, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteReader(connection, commandType, commandText, (OracleParameter[])null);
		}
		
		public static OracleDataReader ExecuteReader(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
		{
			//pass through the call to the private overload using a null transaction value and an externally owned connection
			return ExecuteReader(connection, (OracleTransaction)null, commandType, commandText, commandParameters, OracleConnectionOwnership.External);
		}

		public static OracleDataReader ExecuteReader(OracleConnection connection, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				OracleParameter[] commandParameters = OracleDataAccessParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

				AssignParameterValues(commandParameters, parameterValues);

				return ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
			//otherwise we can just call the SP without params
			else 
			{
				return ExecuteReader(connection, CommandType.StoredProcedure, spName);
			}
		}

		public static OracleDataReader ExecuteReader(OracleTransaction transaction, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteReader(transaction, commandType, commandText, (OracleParameter[])null);
		}

		public static OracleDataReader ExecuteReader(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
		{
			//pass through to private overload, indicating that the connection is owned by the caller
			return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, OracleConnectionOwnership.External);
		}

		public static OracleDataReader ExecuteReader(OracleTransaction transaction, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				OracleParameter[] commandParameters = OracleDataAccessParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

				AssignParameterValues(commandParameters, parameterValues);

				return ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
			}
		}

		#endregion ExecuteReader

		#region ExecuteScalar
		
		public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteScalar(connectionString, commandType, commandText, (OracleParameter[])null);
		}

		public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
		{
			//create & open a OracleConnection, and dispose of it after we are done.
			using (OracleConnection cn = new OracleConnection(connectionString))
			{
				cn.Open();

				//call the overload that takes a connection in place of the connection string
				return ExecuteScalar(cn, commandType, commandText, commandParameters);
			}
		}

        public static object ExecuteScalar(string connectionString, string spName, params OracleParameter[] commandParameters)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((commandParameters != null) && (commandParameters.Length > 0)) 
			{
				//call the overload that takes an array of OracleParameters
				return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
			}
		}

		public static object ExecuteScalar(OracleConnection connection, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteScalar(connection, commandType, commandText, (OracleParameter[])null);
		}

		public static object ExecuteScalar(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			OracleCommand cmd = new OracleCommand();
			PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);
			
			//execute the command & return the results
			object retval = cmd.ExecuteScalar();
			
			// detach the OracleParameters from the command object, so they can be used again.
			cmd.Parameters.Clear();
			return retval;
			
		}

		public static object ExecuteScalar(OracleConnection connection, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OracleDataAccessParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
			}
		}

		public static object ExecuteScalar(OracleTransaction transaction, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteScalar(transaction, commandType, commandText, (OracleParameter[])null);
		}

		public static object ExecuteScalar(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			OracleCommand cmd = new OracleCommand();
			PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
			
			//execute the command & return the results
			object retval = cmd.ExecuteScalar();
			
			// detach the OracleParameters from the command object, so they can be used again.
			cmd.Parameters.Clear();
			return retval;
		}

		public static object ExecuteScalar(OracleTransaction transaction, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OracleDataAccessParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
			}
		}

		#endregion ExecuteScalar	

		#region ExecuteXmlReader
		
		public static XmlReader ExecuteXmlReader(OracleConnection connection, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteXmlReader(connection, commandType, commandText, (OracleParameter[])null);
		}
		
		public static XmlReader ExecuteXmlReader(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			OracleCommand cmd = new OracleCommand();
			PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);
			
			//create the DataAdapter & DataSet
			XmlReader retval = cmd.ExecuteXmlReader();
			
			// detach the OracleParameters from the command object, so they can be used again.
			cmd.Parameters.Clear();
			return retval;
			
		}
		
		public static XmlReader ExecuteXmlReader(OracleConnection connection, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OracleDataAccessParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
			}
		}

		public static XmlReader ExecuteXmlReader(OracleTransaction transaction, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteXmlReader(transaction, commandType, commandText, (OracleParameter[])null);
		}

		public static XmlReader ExecuteXmlReader(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			OracleCommand cmd = new OracleCommand();
			PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
			
			//create the DataAdapter & DataSet
			XmlReader retval = cmd.ExecuteXmlReader();
			
			// detach the OracleParameters from the command object, so they can be used again.
			cmd.Parameters.Clear();
			return retval;			
		}

		public static XmlReader ExecuteXmlReader(OracleTransaction transaction, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OracleDataAccessParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
			}
		}

		#endregion ExecuteXmlReader
    }
	public sealed class OracleDataAccessParameterCache
	{
		#region private methods, variables, and constructors

		//Since this class provides only static methods, make the default constructor private to prevent 
		//instances from being created with "new OracleDataAccessParameterCache()".
		private OracleDataAccessParameterCache() {}

		private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

		/// <summary>
		/// resolve at run time the appropriate set of OracleParameters for a stored procedure
		/// </summary>
		/// <param name="connectionString">a valid connection string for a OracleConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="includeReturnValueParameter">whether or not to include their return value parameter</param>
		/// <returns></returns>
		private static OracleParameter[] DiscoverSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
		{
			using (OracleConnection cn = new OracleConnection(connectionString)) 
			using (OracleCommand cmd = new OracleCommand(spName,cn))
			{
				cn.Open();
				cmd.CommandType = CommandType.StoredProcedure;

                
				//OracleCommandBuilder.DeriveParameters(cmd);

				if (!includeReturnValueParameter) 
				{
					cmd.Parameters.RemoveAt(0);
				}

				OracleParameter[] discoveredParameters = new OracleParameter[cmd.Parameters.Count];;

				cmd.Parameters.CopyTo(discoveredParameters, 0);

				return discoveredParameters;
			}
		}

		//deep copy of cached OracleParameter array
		private static OracleParameter[] CloneParameters(OracleParameter[] originalParameters)
		{
			OracleParameter[] clonedParameters = new OracleParameter[originalParameters.Length];

			for (int i = 0, j = originalParameters.Length; i < j; i++)
			{
				clonedParameters[i] = (OracleParameter)((ICloneable)originalParameters[i]).Clone();
			}

			return clonedParameters;
		}

		#endregion private methods, variables, and constructors

		#region caching functions

		/// <summary>
		/// add parameter array to the cache
		/// </summary>
		/// <param name="connectionString">a valid connection string for a OracleConnection</param>
		/// <param name="commandText">the stored procedure name or PL/SQL command</param>
		/// <param name="commandParameters">an array of SqlParamters to be cached</param>
		public static void CacheParameterSet(string connectionString, string commandText, params OracleParameter[] commandParameters)
		{
			string hashKey = connectionString + ":" + commandText;

			paramCache[hashKey] = commandParameters;
		}

		/// <summary>
		/// retrieve a parameter array from the cache
		/// </summary>
		/// <param name="connectionString">a valid connection string for a OracleConnection</param>
		/// <param name="commandText">the stored procedure name or PL/SQL command</param>
		/// <returns>an array of SqlParamters</returns>
		public static OracleParameter[] GetCachedParameterSet(string connectionString, string commandText)
		{
			string hashKey = connectionString + ":" + commandText;

			OracleParameter[] cachedParameters = (OracleParameter[])paramCache[hashKey];
			
			if (cachedParameters == null)
			{			
				return null;
			}
			else
			{
				return CloneParameters(cachedParameters);
			}
		}

		#endregion caching functions

		#region Parameter Discovery Functions

		/// <summary>
		/// Retrieves the set of OracleParameters appropriate for the stored procedure
		/// </summary>
		/// <remarks>
		/// This method will query the database for this information, and then store it in a cache for future requests.
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a OracleConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <returns>an array of OracleParameters</returns>
		public static OracleParameter[] GetSpParameterSet(string connectionString, string spName)
		{
			return GetSpParameterSet(connectionString, spName, false);
		}

		/// <summary>
		/// Retrieves the set of OracleParameters appropriate for the stored procedure
		/// </summary>
		/// <remarks>
		/// This method will query the database for this information, and then store it in a cache for future requests.
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a OracleConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="includeReturnValueParameter">a bool value indicating whether the return value parameter should be included in the results</param>
		/// <returns>an array of OracleParameters</returns>
		public static OracleParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
		{
			string hashKey = connectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter":"");

			OracleParameter[] cachedParameters;
			
			cachedParameters = (OracleParameter[])paramCache[hashKey];

			if (cachedParameters == null)
			{			
				cachedParameters = (OracleParameter[])(paramCache[hashKey] = DiscoverSpParameterSet(connectionString, spName, includeReturnValueParameter));
			}
			
			return CloneParameters(cachedParameters);
		}

		#endregion Parameter Discovery Functions

	}
}
