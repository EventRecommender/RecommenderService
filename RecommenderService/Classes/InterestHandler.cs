using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;

namespace RecommenderService.Classes
{
	public class InterestHandler
	{
		string connectionString { get; set; }
		MySqlConnection connection { get; set; }
		

		public InterestHandler()
		{
			connectionString = @"server=mysql_recommender;userid=root;password=duper;database=recommender_db";
			connection = new MySqlConnection(connectionString);
		}

		public Tuple<ErrorStatus,Dictionary<string, double>> GetUserInterests(string User_ID)
		{
			connection.Open();

			//Check if user exist:
			ErrorStatus userCheck = ServiceTools.CheckIfUserExist(User_ID, "interest", connection);

			if (userCheck != ErrorStatus.UserAlreadyExist)
			{
				connection.Close();
				return new Tuple<ErrorStatus, Dictionary<string, double>>(userCheck,new Dictionary<string, double>());
			}

			string SQLstatement =	$"SELECT * " +
									$"FROM interest " +
									$"WHERE userid = {User_ID}";

			MySqlCommand command = new MySqlCommand(SQLstatement, connection);
			MySqlDataReader dataReader = command.ExecuteReader();

			//Save values in dictionary
			Dictionary<string, double> dict = new Dictionary<string, double>();
			while (dataReader.Read())
			{
				dict[(string)dataReader[1]] = (double)dataReader[2];
			}
			dataReader.Close();
			command.Dispose();


			connection.Close();
			return new Tuple<ErrorStatus, Dictionary<string, double>>(ErrorStatus.Success, dict);
		}

		public ErrorStatus CreateUserInterests(string user_ID, List<string> initial_types)
		{
			connection.Open();

			//Check if user exist:
			ErrorStatus userCheck = ServiceTools.CheckIfUserExist(user_ID, "interest", connection);

			if (userCheck != ErrorStatus.UserNotFound) //The user should not exist.
			{
				connection.Close();
				return userCheck;
			}

			//Calculate interests:
			Dictionary<string, double> interest = GetSimilarInterests(initial_types);

			StringBuilder sb = new StringBuilder($"INSERT INTO interest (userid, tag, interestvalue) VALUES ");

			List<string> rows = new List<string>();
			foreach(KeyValuePair<string, double> kvp in interest)
			{
				rows.Add(string.Format("('{0}','{1}', '{2}')", MySqlHelper.EscapeString(user_ID), MySqlHelper.EscapeString(kvp.Key), MySqlHelper.EscapeString((kvp.Value).ToString())));
			}
			sb.Append(string.Join(",", rows));
			sb.Append(";");

			string SQLstatement = sb.ToString();
			
			if (string.IsNullOrEmpty(SQLstatement))
			{
				connection.Close();
				return ErrorStatus.QueryStringEmpty;
			}

			MySqlCommand command = new MySqlCommand(SQLstatement, connection);
			MySqlDataAdapter adapter = new MySqlDataAdapter();

			command.CommandType = CommandType.Text;
			adapter.InsertCommand = command;
			adapter.InsertCommand.ExecuteNonQuery();

			command.Dispose();
			adapter.Dispose();

			connection.Close();

			return ErrorStatus.Success;
		}

		public Dictionary<string, double> GetSimilarInterests(List<string> initial_types)
		{
			//Get similar users:
			List<int> similarUsersList = GetSimilarUsers(initial_types);


			// Get interests from similar users:
			StringBuilder sb = new StringBuilder("");
			foreach(int user in similarUsersList) 
			{
				sb.Append($"'{user}',");
			}

			if (sb.Length > 0)
			{
				sb.Length--; //removes trailing comma
			}
			else
			{
				sb.Append("''");//no users
			}
			
			string similarUsers = sb.ToString();
			sb.Clear();

			string SQLstatement =	$"SELECT * " +
									$"FROM interest " +
									$"WHERE userid IN ({similarUsers})";

			MySqlCommand command = new MySqlCommand(SQLstatement, connection);
			MySqlDataReader dataReader = command.ExecuteReader();

			Dictionary<string, double> dict = new Dictionary<string, double>();

			foreach (string type in initial_types)
			{
				dict[type] = 100 / initial_types.Count;
			}

			while (dataReader.Read())
			{
				dict[(string)dataReader[1]] = (dict[(string)dataReader[1]] + ((double)dataReader[2] / (similarUsersList.Count + 1)));
			}

			dataReader.Close();
			command.Dispose();

			return dict;
		}

		public List<int> GetSimilarUsers(List<string> initial_types)
		{
			int val1 = 15;
			int val2 = 40; // used to stop outliers.
			int countNumber = 4; //amount of tags which other users have to have within the limit

			StringBuilder sb = new StringBuilder("");
			foreach (string type in initial_types)
			{
				sb.Append($"'{type}',");
			}
			if (sb.Length > 1)
			{
				sb.Length--; //removes trailing comma
			}
			else
			{
				throw new Exception("StringBuilder is empty");
			}
			
			string types = sb.ToString();

			sb.Clear();


			string SQLstatement =	$"SELECT DISTINCT id " +
									$"FROM (SELECT userid AS id, COUNT(userid) as count " +
										$"FROM interest " +
										$"WHERE tag IN " +
											$"(SELECT DISTINCT tag " +
											$"FROM interest " +
											$"WHERE tag IN ({types}) " + 
											$"AND interestvalue BETWEEN {val1} AND {val2}" +
											$") " +
										$"GROUP BY userid" +
										$") AS temp " +
									$"WHERE count = {countNumber}"; // "count" is the number of types which are the same as those in initial_types.


			MySqlCommand command = new MySqlCommand(SQLstatement, connection);
			MySqlDataReader dataReader = command.ExecuteReader();

			List<int> similarUsersList = new List<int>();

			while (dataReader.Read())
			{
				similarUsersList.Add((int)dataReader[0]);
			}
			dataReader.Close();
			command.Dispose();

			return similarUsersList;
		}

		public ErrorStatus UpdateUserInterests(string User_ID, List<string> activity_types, int Update_type)
		{
			//Determine value to update with
			int updateVal = 0;

			if (Update_type == 0)
			{
				updateVal = 5;
			}
			else if (Update_type == 1)
			{
				updateVal = -5;
			}

			//Check if user exist
			connection.Open();
			ErrorStatus userCheck = ServiceTools.CheckIfUserExist(User_ID, "interest", connection);
			connection.Close();

			if (userCheck != ErrorStatus.UserAlreadyExist)
			{
				return userCheck;
			}


			//Get user interests
			Tuple<ErrorStatus,Dictionary<string,double>> tuple = GetUserInterests(User_ID);

			if (tuple.Item1 != ErrorStatus.Success)
			{
				connection.Close();
				return tuple.Item1;
			}

			Dictionary<string, double> dict = tuple.Item2;

			//update values in dictionary
			foreach (var tag in activity_types) 
			{
				dict[tag] = dict[tag] + updateVal;
				
			}


			//Normalize dictionary
			double sum = dict.Sum(x => x.Value);
			double sumAvg = (100 / sum);

			foreach (KeyValuePair<string, double> kvp in dict) 
			{
				double val = kvp.Value * sumAvg;
				
				dict[kvp.Key] = val;

			}


			//Update database with new values
			StringBuilder sb = new StringBuilder("");

			connection.Open();

			string SQLstatement;
			foreach (KeyValuePair<string, double> kvp in dict)
			{
				SQLstatement = ($"UPDATE interest " +
								$"SET interestvalue = '{kvp.Value}' " +
								$"WHERE tag = '{kvp.Key}' " +
								$"AND userid = '{User_ID}' ") ;
				MySqlCommand command = new MySqlCommand(SQLstatement, connection);
				MySqlDataAdapter adapter = new MySqlDataAdapter();

				adapter.InsertCommand = command;
				adapter.InsertCommand.ExecuteNonQuery();

				command.Dispose();
				adapter.Dispose();
			}

			//Finished
			connection.Close();
			return ErrorStatus.Success;
		}

		public ErrorStatus RemoveUserInterest(string User_ID)
		{
			connection.Open();
			//Check if user exist
			ErrorStatus userCheck = ServiceTools.CheckIfUserExist(User_ID, "interest", connection);

			if (userCheck != ErrorStatus.UserAlreadyExist)
			{
				connection.Close();
				return userCheck;
			}

			//Delete records related to user.
			string SQLstatement =	$"DELETE " +
									$"FROM interest " +
									$"WHERE userid = {User_ID}";

			MySqlCommand command = new MySqlCommand(SQLstatement, connection);
			MySqlDataAdapter adapter = new MySqlDataAdapter();

			adapter.InsertCommand = command;
			adapter.InsertCommand.ExecuteNonQuery();

			command.Dispose();
			adapter.Dispose();


			connection.Close();
			return ErrorStatus.Success;
		}
	}
}
