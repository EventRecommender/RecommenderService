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

			//Check if user exist:
			string SQLstatement = $"SELECT COUNT(*)" +
										$" FROM interest" +
										$" WHERE userid = {user_ID}";
			SqlCommand command = new SqlCommand(SQLstatement, connection);
			SqlDataReader dataReader = command.ExecuteReader();

			string idCountString = dataReader.GetString(0);
			try
			{
				var idCount = int.Parse(idCountString);

				if (idCount > 0)
				{
					dataReader.Close();
					command.Dispose();
					connection.Close();
					if (idCount > 1)
					{
						return ErrorStatus.DublicateUser;
					}
					return ErrorStatus.UserAlreadyExist;
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

			//Calculate interests:
			Dictionary<string, float> interest = GetSimilarInterests(initial_types);

			StringBuilder sb = new StringBuilder("");
			foreach(KeyValuePair<string, float> kvp in interest)
			{
				sb.AppendLine($"INSERT INTO interest (userid, tag, value) values({user_ID}, {kvp.Key}, {kvp.Value})");
			}
			SQLstatement = sb.ToString();

			command = new SqlCommand(SQLstatement, connection);
			SqlDataAdapter adapter = new SqlDataAdapter();

			adapter.InsertCommand = command;
			adapter.InsertCommand.ExecuteNonQuery();

			command.Dispose();
			adapter.Dispose();

			connection.Close();

			return ErrorStatus.Success;
		}

		public Dictionary<string, float> GetSimilarInterests(List<string> initial_types)
		{
			//Get similar users:
			List<int> similarUsersList = GetSimilarUsers(initial_types);


			// Get interests from similar users:
			StringBuilder sb = new StringBuilder("");
			foreach(int user in similarUsersList) 
			{
				sb.Append($"'{user}',");
			}
			sb.Remove(sb.Length - 1, 1); //removes trailing comma
			string similarUsers = sb.ToString();
			sb.Clear();

			string SQLstatement =	$"SELECT * " +
									$"FROM interest " +
									$"WHERE userid IN ({similarUsers})";

			SqlCommand command = new SqlCommand(SQLstatement, connection);
			SqlDataReader dataReader = command.ExecuteReader();

			Dictionary<string, float> dict = new Dictionary<string, float>();

			while (dataReader.Read())
			{
				if (!dict.ContainsKey((string)dataReader[1])) //check if NOT exist
				{
					dict[(string)dataReader[1]] = 25 / (similarUsersList.Count + 1);// +1 to account for the user
				}

				dict[(string)dataReader[1]] = (dict[(string)dataReader[1]] + ((float)dataReader[2] / (similarUsersList.Count + 1)));
			}

			dataReader.Close();
			command.Dispose();

			return dict;
		}

		public List<int> GetSimilarUsers(List<string> initial_types)
		{
			int val1 = 15;
			int val2 = 40; // used to stop outliers.

			StringBuilder sb = new StringBuilder("");
			foreach (string type in initial_types)
			{
				sb.Append($"'{type}',");
			}
			sb.Remove(sb.Length - 1, 1); //removes trailing comma
			string types = sb.ToString();
			sb.Clear();

			string SQLstatement = $"SELECT DISTINCT userid " +
									$"FROM (SELECT userid, COUNT(userid) as count" +
									$"			FROM interest" +
									$"			WHERE tag IN ('{types}')" +
									$"			AND tag IN" +
									$"				(SELECT * " +
									$"						FROM interest" +
									$"						WHERE value BETWEEN {val1} AND {val2})) " +
									$"WHERE count = 4"; // "count" is the number of types which are the same as those in initial_types.



			SqlCommand command = new SqlCommand(SQLstatement, connection);
			SqlDataReader dataReader = command.ExecuteReader();

			List<int> similarUsersList = new List<int>();

			while (dataReader.Read())
			{
				similarUsersList.Add((int)dataReader[0]);
			}
			dataReader.Close();
			command.Dispose();

			return similarUsersList;
		}

	}
}
