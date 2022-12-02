using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderService.Classes
{
	public class InterestHandler
	{
		string connectionString { get; set; }
		SqlConnection connection { get; set; }
		

		public InterestHandler()
		{
			connectionString = @"";
			connection = new SqlConnection(connectionString);
		}

		public ErrorStatus CreateUserInterests(string user_ID, List<string> initial_types)
		{
			connection.Open();

			//Check if user exist
			string SQLstatement = $"SELECT COUNT(*) WHERE UserID = {user_ID}";
			SqlCommand command = new SqlCommand(SQLstatement, connection);
			SqlDataReader dataReader = command.ExecuteReader();

			string idCountString = dataReader.GetString(0);
			try
			{
				var idCount = int.Parse(idCountString);

				if (idCount != 1)
				{
					dataReader.Close();
					command.Dispose();
					connection.Close();
					if (idCount > 1)
					{
						return ErrorStatus.DublicateUser;
					}
					return ErrorStatus.UserNotFound;
				}
				dataReader.Close();
				command.Dispose();
			}
			catch (FormatException)
			{
				dataReader.Close();
				command.Dispose();
				connection.Close();

				throw; //TODO: Do stuff
			}
			//Check for user done

			//Calculate other interests

			SQLstatement = "";
			command = new SqlCommand(SQLstatement, connection);
			SqlDataAdapter adapter = new SqlDataAdapter();

			adapter.InsertCommand = command;
			adapter.InsertCommand.ExecuteNonQuery();

			command.Dispose();

			//Calculation done

			connection.Close();

			return ErrorStatus.Success;
		}

	}
}
