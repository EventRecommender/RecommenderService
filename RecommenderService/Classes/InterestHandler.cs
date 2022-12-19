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
using System.Globalization;

namespace RecommenderService.Classes
{
	public class InterestHandler
	{
		string connectionString { get; set; }

		public InterestHandler()
		{
			connectionString = @"server=mysql_recommender;userid=root;password=duper;database=recommender_db";
		}

		public InterestHandler(string _connectionString)
		{
			connectionString = _connectionString;
		}

		public Tuple<ErrorStatus,Dictionary<string, double>> GetUserInterests(string User_ID)
		{
			MySqlConnection connection = new(connectionString);
			connection.Open();

			//Check if user exist:
			ErrorStatus userCheck = ServiceTools.CheckIfUserExist(User_ID, "interest", connection);

			if (userCheck != ErrorStatus.UserAlreadyExist)
			{
				connection.Close();
				return new Tuple<ErrorStatus, Dictionary<string, double>>(userCheck,new Dictionary<string, double>());
			}


			//get user interests
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
			MySqlConnection connection = new(connectionString);
			connection.Open();

			//Check if user exist:
			ErrorStatus userCheck = ServiceTools.CheckIfUserExist(user_ID, "interest", connection);

			if (userCheck != ErrorStatus.UserNotFound) //The user should not exist.
			{
				connection.Close();
				return userCheck;
			}

			connection.Close();

			//Get similar users:
			Dictionary<string, double> interest = GetSimilarInterests(initial_types);


			//Create SQL string for inserting into db
			StringBuilder sb = new StringBuilder($"INSERT INTO interest (userid, tag, interestvalue) VALUES ");
			List<string> rows = new List<string>();
			foreach(KeyValuePair<string, double> kvp in interest)
			{
				rows.Add(string.Format("('{0}', '{1}', '{2}')", MySqlHelper.EscapeString(user_ID), MySqlHelper.EscapeString(kvp.Key), MySqlHelper.EscapeString(Math.Round(kvp.Value, 3).ToString("n", CultureInfo.InvariantCulture))));
			}
			sb.Append(string.Join(",", rows));
			sb.Append(";");

			string SQLstatement = sb.ToString();
			
			if (string.IsNullOrEmpty(SQLstatement))
			{
				connection.Close();
				return ErrorStatus.QueryStringEmpty;
			}

			//Execute SQL
			connection.Open();
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
			MySqlConnection connection = new(connectionString);
			//Get similar users:
			List<int> similarUsersList = GetSimilarUsers(initial_types);

			connection.Open();
			// Create SQL list of similar users
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


			//Get interests from similar users
			string SQLstatement =	$"SELECT * " +
									$"FROM interest " +
									$"WHERE userid IN ({similarUsers})";

			MySqlCommand command = new MySqlCommand(SQLstatement, connection);
			MySqlDataReader dataReader = command.ExecuteReader();


			//Create dictionary with average interests from similar users
			Dictionary<string, double> dict = new Dictionary<string, double>();
			foreach (string type in initial_types)
			{
				dict[type] = ((double)100 / initial_types.Count);
			}
			Dictionary<string, double> dictCopy = dict;

			/*
			while (dataReader.Read())
			{
				if (dict.ContainsKey((string)dataReader[1]) == false)
				{
					dict[(string)dataReader[1]] = ((double)dataReader[2] / 2);
				}
				else
				{
					double currentVal = dict[(string)dataReader[1]];
					
					//current val				=		(current val				+		new val)		/	2	
					dict[(string)dataReader[1]] = ((currentVal + (double)dataReader[2]) / 2);
				}
			}
			*/

			while (dataReader.Read())
			{
				if (dict.ContainsKey((string)dataReader[1]) == false)
				{
					dict[(string)dataReader[1]] = (double)dataReader[2];
				}
				else
				{
					double currentVal = dict[(string)dataReader[1]];

					//current val				=		(current val				+		new val)		/	2	
					dict[(string)dataReader[1]] = (currentVal + (double)dataReader[2]);
				}
			}

			foreach (var kvp in dict)
			{
				dict[kvp.Key] = dict[kvp.Key] / similarUsers.Count() + 1;

			}



			// duct tape solution to somthing that could be done better.
			// since double is not perfect it does not sum directly to 100
			//-----------------
			double sum = dict.Sum(x => x.Value);
			double sumAvg = (100 / sum);

			foreach (KeyValuePair<string, double> kvp in dict)
			{
				double val = kvp.Value * sumAvg;

				dict[kvp.Key] = val;
			}
			//------------------

			dataReader.Close();
			command.Dispose();
			connection.Close();

			return dict;
		}

		public List<int> GetSimilarUsers(List<string> initial_types)
		{
			MySqlConnection connection = new(connectionString);
			connection.Open();

			int similarValMin = (int)Math.Round((100 / initial_types.Count) * 0.90); // 90% of the value
			int similarValMax = (int)Math.Round((100 / initial_types.Count) * 1.10); // 110% of the value

			int val1 = similarValMin;
			int val2 = similarValMax;
			int countNumber = 3; //amount of tags which other users have to have within the limit
			int limit = 300; //Limit the amount of similar users

			//Create SQL list of the initial_types
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


			//Create SQL transaction
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
									$"WHERE count >= {countNumber} " + // "count" is the number of types which are the same as those in initial_types.
									$"LIMIT {limit}";


			MySqlCommand command = new MySqlCommand(SQLstatement, connection);
			MySqlDataReader dataReader = command.ExecuteReader();


			//Get similar users
			List<int> similarUsersList = new List<int>();
			while (dataReader.Read())
			{
				similarUsersList.Add((int)dataReader[0]);
			}
			dataReader.Close();
			command.Dispose();
			connection.Close();

			return similarUsersList;
		}

		public ErrorStatus UpdateUserInterests(string User_ID, List<string> activity_types, UpdateType Update_type)
		{
			MySqlConnection connection = new(connectionString);
			//Determine value to update with
			int updateVal = 0;

			if (Update_type == UpdateType.Like)
			{
				updateVal = 5;
			}
			else if (Update_type == UpdateType.Dislike)
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
				if (Update_type == UpdateType.Like)
				{
					if (dict.ContainsKey(tag) == false)
					{
						dict[tag] = updateVal;
					}
					else
					{
						dict[tag] = dict[tag] + updateVal;
					}
				}
				else if (Update_type == UpdateType.Dislike)
				{
					if (dict.ContainsKey(tag) == false)
					{
						if (dict[tag] > Math.Abs(updateVal))
						{
							dict[tag] = updateVal;
						}
					}
					else
					{
						if (dict[tag] > Math.Abs(updateVal))
						{
							dict[tag] = dict[tag] + updateVal;
						}
					}

					
				}
				
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
			MySqlConnection connection = new(connectionString);
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

		public void ClearDatabase()
		{
			MySqlConnection connection = new(connectionString);
			connection.Open();
			string SQLstatement = "DELETE FROM interest";

			//Execute SQL
			MySqlCommand command = new MySqlCommand(SQLstatement, connection);
			MySqlDataAdapter adapter = new MySqlDataAdapter();


			command.CommandType = CommandType.Text;
			adapter.InsertCommand = command;
			adapter.InsertCommand.ExecuteNonQuery();

			command.Dispose();
			adapter.Dispose();

			connection.Close();
		}//for testing only. Dont use in production

	}
}
