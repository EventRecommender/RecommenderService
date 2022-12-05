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
			string SQLstatement = $"SELECT COUNT(*)" +
										$" FROM interest" +
										$" WHERE UserID = {user_ID}";
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

			//Calculate interests

			List<string> interest = GetSimilarInterests(initial_types);


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

		public List<string> GetSimilarInterests(List<string> initial_types)
		{
			connection.Open();

			int val1 = 15;
			int val2 = 40; // used to stop outliers.

			string SQLstatement =	$"SELECT DISTINCT UserID " +
									$"FROM (SELECT UserID, COUNT(UserID) as count" +
									$"			FROM Interest" +
									$"			WHERE TypeID IN ('{initial_types[0]}', '{initial_types[1]}', '{initial_types[2]}', '{initial_types[3]}')" +
									$"			AND TypeID IN" +
									$"				(SELECT * " +
									$"						FROM Interest" +
									$"						WHERE Value BETWEEN {val1} AND {val2})) " +
									$"WHERE count = 4"; // "count" is the number of types which are the same as those in initial_types.



			SqlCommand command = new SqlCommand(SQLstatement, connection);
			SqlDataReader dataReader = command.ExecuteReader();

			List<int> similarUsers = new List<int>();

			while (dataReader.Read())
			{
				similarUsers.Add((int)dataReader[0]);
			}

			command.Dispose();

			connection.Close();
			return default;
		}

	}
}
